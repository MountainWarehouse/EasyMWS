﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using MountainWarehouse.EasyMWS.CallbackLogic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class FeedProcessor : IFeedQueueingProcessor
	{
		private readonly IFeedSubmissionProcessor _feedSubmissionProcessor;
		private readonly ICallbackActivator _callbackActivator;
		private readonly IEasyMwsLogger _logger;

		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

        public event EventHandler<FeedUploadedEventArgs> FeedUploadedInternal;
		public event EventHandler<FeedRequestFailedEventArgs> FeedRequestFailedInternal;

		/// <summary>
		/// Constructor to be used for UnitTesting/Mocking (in the absence of a dedicated DependencyInjection framework)
		/// </summary>
		internal FeedProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, EasyMwsOptions options, IMarketplaceWebServiceClient mwsClient, IFeedSubmissionProcessor feedSubmissionProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
		  : this(region, merchantId, mWSAuthToken, options, mwsClient, logger)
		{
			_feedSubmissionProcessor = feedSubmissionProcessor;
			_callbackActivator = callbackActivator;

			RegisterEvents();
		}

		internal FeedProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, EasyMwsOptions options, IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();

			_feedSubmissionProcessor = _feedSubmissionProcessor ?? new FeedSubmissionProcessor(_region, _merchantId, mWSAuthToken, mwsClient, _logger, _options);

			RegisterEvents();
		}

		private void RegisterEvents()
		{
			_feedSubmissionProcessor.FeedEntryWasMarkedForDelete -= OnFeedRequestFailedInternal;
			_feedSubmissionProcessor.FeedEntryWasMarkedForDelete += OnFeedRequestFailedInternal;
		}

		public void PollFeeds(IFeedSubmissionEntryService feedSubmissionService)
		{
			_logger.Debug("Executing polling action for feed requests.");

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(feedSubmissionService);

			SubmitNextFeedInQueueToAmazon(feedSubmissionService);

			RequestFeedSubmissionStatusesAllFromAmazon(feedSubmissionService);

			DownloadNextFeedSubmissionResultFromAmazon(feedSubmissionService);

			PublishEventsForPreviouslySubmittedFeeds(feedSubmissionService);
		}

		private void OnFeedRequestFailedInternal(object sender, FeedRequestFailedEventArgs e) => FeedRequestFailedInternal?.Invoke(null, e);

		private void OnFeedUploaded(FeedUploadedEventArgs e) => FeedUploadedInternal?.Invoke(this, e);

		private void PublishEventsForPreviouslySubmittedFeeds(IFeedSubmissionEntryService feedSubmissionService)
		{
			var previouslySubmittedFeeds = feedSubmissionService.GetAllFromQueueOfFeedsReadyForCallback(_merchantId, _region);

			foreach (var feedSubmissionEntry in previouslySubmittedFeeds)
			{
				try
				{
                    var processingReportContent = ZipHelper.ExtractArchivedSingleFileToStream(feedSubmissionEntry.Details.FeedSubmissionReport);
                    var feedType = feedSubmissionEntry.FeedType;
                    var handlerId = feedSubmissionEntry.TargetHandlerId;
                    var handledArgs = feedSubmissionEntry.TargetHandlerArgs == null ? null : new ReadOnlyDictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(feedSubmissionEntry.TargetHandlerArgs));
                    var eventArgs = new FeedUploadedEventArgs(processingReportContent,feedType, handlerId, handledArgs);

                    _logger.Debug($"Attempting publish FeedUploaded for the next submitted feed in queue : {feedSubmissionEntry.EntryIdentityDescription}");
                    OnFeedUploaded(eventArgs);
                    feedSubmissionService.Delete(feedSubmissionEntry);
					_logger.Info($"Event publishing has succeeded for {feedSubmissionEntry.EntryIdentityDescription}.");
				}
				catch (SqlException e)
				{
					_logger.Error($"Event publishing failed for {feedSubmissionEntry.EntryIdentityDescription} due to an internal error '{e.Message}'. The event publishing will be retried at the next poll request", e);
					feedSubmissionService.Unlock(feedSubmissionEntry, "Unlocking single feed submission entry - an SQL exception occurred while trying to invoke callback.");
					feedSubmissionService.Update(feedSubmissionEntry);
				}
				catch (Exception e)
				{
					_logger.Error($"Event publishing failed for {feedSubmissionEntry.EntryIdentityDescription}. Current retry count is :{feedSubmissionEntry.FeedSubmissionRetryCount}. {e.Message}", e);
					feedSubmissionEntry.InvokeCallbackRetryCount++;
					feedSubmissionService.Unlock(feedSubmissionEntry, "Unlocking single feed submission entry - an exception occurred while trying to invoke callback.");
					feedSubmissionService.Update(feedSubmissionEntry);
				}
			}

			feedSubmissionService.SaveChanges();
		}

		public void QueueFeed(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer, string targetEventId = null, Dictionary<string, object> targetEventArgs = null)
        {
			try
			{
				if (propertiesContainer == null) throw new ArgumentNullException();

				var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

                var feedSubmission = new FeedSubmissionEntry(serializedPropertiesContainer)
                {
                    AmazonRegion = _region,
                    MerchantId = _merchantId,
                    LastSubmitted = DateTime.MinValue,
                    DateCreated = DateTime.UtcNow,
                    IsProcessingComplete = false,
                    HasErrors = false,
                    SubmissionErrorData = null,
                    FeedSubmissionRetryCount = 0,
                    FeedSubmissionId = null,
                    FeedType = propertiesContainer.FeedType,
                    TargetHandlerId = targetEventId,
                    TargetHandlerArgs = targetEventArgs == null ? null : JsonConvert.SerializeObject(targetEventArgs),
                    InstanceId = _options?.EventPublishingOptions?.RestrictInvocationToOriginatingInstance?.HashedInstanceId,
                    Details = new FeedSubmissionDetails
					{
						FeedContent = ZipHelper.CreateArchiveFromContent(propertiesContainer.FeedContent)
					}
				};

				feedSubmissionService.Unlock(feedSubmission, "Unlocking single feed submission entry - newly created, ready for processing.");
				feedSubmissionService.Create(feedSubmission);
				feedSubmissionService.SaveChanges();

				_logger.Debug($"The following feed was queued for submission to Amazon {feedSubmission.EntryIdentityDescription}.");
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void PurgeQueue(IFeedSubmissionEntryService feedSubmissionService)
		{
			var entriesToDelete = feedSubmissionService.GetAll().Where(rre => rre.AmazonRegion == _region && rre.MerchantId == _merchantId);
			feedSubmissionService.DeleteRange(entriesToDelete);
			feedSubmissionService.SaveChanges();
		}

		public void SubmitNextFeedInQueueToAmazon(IFeedSubmissionEntryService feedSubmissionService)
		{
			var feedSubmission = feedSubmissionService.GetNextFromQueueOfFeedsToSubmit(_merchantId, _region);

			if (feedSubmission == null) return;
		
			_feedSubmissionProcessor.SubmitFeedToAmazon(feedSubmissionService, feedSubmission);
		}

		public void RequestFeedSubmissionStatusesAllFromAmazon(IFeedSubmissionEntryService feedSubmissionService)
		{
			var allFeedSubmissions = feedSubmissionService.GetAllSubmittedFeedsFromQueue(_merchantId, _region).ToList();
			const int batchSize = 20;
			var numberOfBatches = (allFeedSubmissions.Count + batchSize - 1) / batchSize;

            for (int batchNumber = 0; batchNumber < numberOfBatches; batchNumber++)
            {
				var nextBatchOfEntriesToProcess = allFeedSubmissions.Skip(batchSize * batchNumber).Take(batchSize).ToList();
				RequestFeedSubmissionStatusesBatchFromAmazon(feedSubmissionService, nextBatchOfEntriesToProcess);
			}
		}

		public void RequestFeedSubmissionStatusesBatchFromAmazon(IFeedSubmissionEntryService feedSubmissionService, List<FeedSubmissionEntry> feedSubmissionsBatch)
		{
			if (!feedSubmissionsBatch.Any())
				return;

			feedSubmissionService.LockMultipleEntries(feedSubmissionsBatch, "Amazon status update has started");

			var feedSubmissionIds = feedSubmissionsBatch.Select(_ => _.FeedSubmissionId);
			var feedSubmissionResults = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(feedSubmissionService, feedSubmissionIds, _merchantId);
			if (feedSubmissionResults != null)
			{
				_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(feedSubmissionService, feedSubmissionResults);
			}

			feedSubmissionService.UnlockMultipleEntries(feedSubmissionsBatch, "Amazon status update has been completed");
		}

		public void DownloadNextFeedSubmissionResultFromAmazon(IFeedSubmissionEntryService feedSubmissionService)
		{
			var nextFeedWithProcessingComplete = feedSubmissionService.GetNextFromQueueOfProcessingCompleteFeeds(_merchantId, _region);
			if (nextFeedWithProcessingComplete == null) return;

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(feedSubmissionService, nextFeedWithProcessingComplete);
		}
	}
}
