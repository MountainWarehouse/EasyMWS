using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Constructor to be used for UnitTesting/Mocking (in the absence of a dedicated DependencyInjection framework)
        /// </summary>
        internal FeedProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, EasyMwsOptions options, IMarketplaceWebServiceClient mwsClient, IFeedSubmissionProcessor feedSubmissionProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
		  : this(region, merchantId, mWSAuthToken, options, mwsClient, logger)
		{
			_feedSubmissionProcessor = feedSubmissionProcessor;
			_callbackActivator = callbackActivator;
		}

		internal FeedProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, EasyMwsOptions options, IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();

			_feedSubmissionProcessor = _feedSubmissionProcessor ?? new FeedSubmissionProcessor(_region, _merchantId, mWSAuthToken, mwsClient, _logger, _options);
		}

		public void PollFeeds(IFeedSubmissionEntryService feedSubmissionService)
		{
			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(feedSubmissionService);

			SubmitNextFeedInQueueToAmazon(feedSubmissionService);

			RequestFeedSubmissionStatusesFromAmazon(feedSubmissionService);

			DownloadNextFeedSubmissionResultFromAmazon(feedSubmissionService);

			PerformCallbacksForPreviouslySubmittedFeeds(feedSubmissionService);
		}

        private void OnFeedUploaded(FeedUploadedEventArgs e)
        {
            EventHandler<FeedUploadedEventArgs> handler = FeedUploadedInternal;
            handler?.Invoke(this, e);
        }

        private void PerformCallbacksForPreviouslySubmittedFeeds(IFeedSubmissionEntryService feedSubmissionService)
		{
			var previouslySubmittedFeeds = feedSubmissionService.GetAllFromQueueOfFeedsReadyForCallback(_merchantId, _region);

			foreach (var feedSubmissionEntry in previouslySubmittedFeeds)
			{
				try
				{
                    var eventArgs = new FeedUploadedEventArgs
                    {
                        FeedType = feedSubmissionEntry.FeedType,
                        ProcessingReportContent = ZipHelper.ExtractArchivedSingleFileToStream(feedSubmissionEntry.Details.FeedSubmissionReport),
                        TargetHandlerId = feedSubmissionEntry.TargetHandlerId,
                        TargetHandlerArgs = feedSubmissionEntry.TargetHandlerArgs == null ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(feedSubmissionEntry.TargetHandlerArgs)
                    };

                    _logger.Info($"Attempting to perform method callback for the next submitted feed in queue : {feedSubmissionEntry.RegionAndTypeComputed}");
                    OnFeedUploaded(eventArgs);
                    feedSubmissionService.Delete(feedSubmissionEntry);
				}
				catch (SqlException e)
				{
					_logger.Error($"Method callback failed for {feedSubmissionEntry.RegionAndTypeComputed} due to an internal error '{e.Message}'. The callback will be retried at the next poll request", e);
					feedSubmissionEntry.IsLocked = false;
					feedSubmissionService.Update(feedSubmissionEntry);
				}
				catch (Exception e)
				{
					_logger.Error($"Method callback failed for {feedSubmissionEntry.RegionAndTypeComputed}. Current retry count is :{feedSubmissionEntry.FeedSubmissionRetryCount}. {e.Message}", e);
					feedSubmissionEntry.InvokeCallbackRetryCount++;
					feedSubmissionEntry.IsLocked = false;
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
                    IsLocked = false,
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
                    Details = new FeedSubmissionDetails
					{
						FeedContent = ZipHelper.CreateArchiveFromContent(propertiesContainer.FeedContent)
					}
				};



				feedSubmissionService.Create(feedSubmission);
				feedSubmissionService.SaveChanges();

				_logger.Info($"The following feed was queued for submission to Amazon {feedSubmission.RegionAndTypeComputed}.");
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

		public void RequestFeedSubmissionStatusesFromAmazon(IFeedSubmissionEntryService feedSubmissionService)
		{
			var feedSubmissionIds = feedSubmissionService.GetIdsForSubmittedFeedsFromQueue(_merchantId, _region).ToList();

			if (!feedSubmissionIds.Any())
				return;

			var feedSubmissionResults = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(feedSubmissionIds, _merchantId);
			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(feedSubmissionService, feedSubmissionResults);
		}

		public void DownloadNextFeedSubmissionResultFromAmazon(IFeedSubmissionEntryService feedSubmissionService)
		{
			var nextFeedWithProcessingComplete = feedSubmissionService.GetNextFromQueueOfProcessingCompleteFeeds(_merchantId, _region);
			if (nextFeedWithProcessingComplete == null) return;

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(feedSubmissionService, nextFeedWithProcessingComplete);
		}
	}
}
