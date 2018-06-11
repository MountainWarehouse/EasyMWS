using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using MountainWarehouse.EasyMWS.CallbackLogic;
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

		internal FeedProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options, IMarketplaceWebServiceClient mwsClient, IFeedSubmissionProcessor feedSubmissionProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
		  : this(region, merchantId, options, mwsClient, logger)
		{
			_feedSubmissionProcessor = feedSubmissionProcessor;
			_callbackActivator = callbackActivator;
		}

		internal FeedProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options, IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();

			_feedSubmissionProcessor = _feedSubmissionProcessor ?? new FeedSubmissionProcessor(_region, _merchantId, mwsClient, _logger, _options);
		}

		public void PollFeeds(IFeedSubmissionCallbackService feedSubmissionService)
		{
			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(feedSubmissionService);

			SubmitNextFeedInQueueToAmazon(feedSubmissionService);

			RequestFeedSubmissionStatusesFromAmazon(feedSubmissionService);

			RequestNextFeedSubmissionInQueueFromAmazon(feedSubmissionService);

			PerformCallbacksForPreviouslySubmittedFeeds(feedSubmissionService);
		}

		private bool IsValidFeedSubmissionReportHash(IFeedSubmissionCallbackService feedSubmissionService, (FeedSubmissionEntry feedSubmissionCallback, MemoryStream reportContent, string contentMd5) reportInfo)
		{
			if (MD5ChecksumHelper.IsChecksumCorrect(reportInfo.reportContent, reportInfo.contentMd5))
			{
				_logger.Info($"Checksum verification succeeded for feed submission report for {reportInfo.feedSubmissionCallback.RegionAndTypeComputed}");
				return true;
			}
			else
			{
				_logger.Warn($"Checksum verification failed for feed submission report for {reportInfo.feedSubmissionCallback.RegionAndTypeComputed}");
				_feedSubmissionProcessor.MoveToRetryQueue(feedSubmissionService, reportInfo.feedSubmissionCallback);
				return false;
			}
		}

		private void PerformCallbacksForPreviouslySubmittedFeeds(IFeedSubmissionCallbackService feedSubmissionService)
		{
			var previouslySubmittedFeeds = feedSubmissionService.GetAll()
				.Where(fse => fse.AmazonRegion == _region && fse.MerchantId == _merchantId && fse.Details != null && fse.Details.FeedSubmissionReport != null);

			foreach (var feedSubmissionEntry in previouslySubmittedFeeds)
			{
				try
				{
					using (var feedSubmissionReport = ZipHelper.ExtractArchivedSingleFileToStream(feedSubmissionEntry.Details.FeedSubmissionReport))
					{
						ExecuteMethodCallback(feedSubmissionEntry, feedSubmissionReport);
						feedSubmissionService.Delete(feedSubmissionEntry);
					}
				}
				catch (SqlException e)
				{
					_logger.Error(e.Message, e);
				}
				catch (Exception e)
				{
					feedSubmissionEntry.SubmissionRetryCount++;
					feedSubmissionService.Update(feedSubmissionEntry);
					_logger.Error($"Method callback failed for {feedSubmissionEntry.RegionAndTypeComputed}. Placing feed submission entry in retry queue. Current retry count is :{feedSubmissionEntry.SubmissionRetryCount}. {e.Message}", e);
				}
			}

			feedSubmissionService.SaveChanges();
		}

		public void QueueFeed(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			try
			{
				if (callbackMethod == null)
				{
					throw new ArgumentNullException(nameof(callbackMethod), "The callback method cannot be null, as it has to be invoked once the report has been downloaded, in order to provide access to the report content.");
				}

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
					SubmissionRetryCount = 0,
					FeedSubmissionId = null,
					FeedType = propertiesContainer.FeedType,
					Details = new FeedSubmissionDetails
					{
						FeedContent = ZipHelper.CreateArchiveFromContent(propertiesContainer.FeedContent)
					}
				};
				
				var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);
				feedSubmission.Data = serializedCallback.Data;
				feedSubmission.TypeName = serializedCallback.TypeName;
				feedSubmission.MethodName = serializedCallback.MethodName;
				feedSubmission.DataTypeName = serializedCallback.DataTypeName;

				feedSubmissionService.Create(feedSubmission);
				feedSubmissionService.SaveChanges();

				_logger.Info($"The following feed was queued for submission to Amazon {feedSubmission.RegionAndTypeComputed}.");
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void PurgeQueue(IFeedSubmissionCallbackService feedSubmissionService)
		{
			var entriesToDelete = feedSubmissionService.GetAll().Where(rre => rre.AmazonRegion == _region && rre.MerchantId == _merchantId);
			feedSubmissionService.DeleteRange(entriesToDelete);
			feedSubmissionService.SaveChanges();
		}

		public void SubmitNextFeedInQueueToAmazon(IFeedSubmissionCallbackService feedSubmissionService)
		{
			var feedSubmission = _feedSubmissionProcessor.GetNextFromQueueOfFeedsToSubmit(feedSubmissionService);

			if (feedSubmission == null) return;
		
			var feedSubmissionId = _feedSubmissionProcessor.SubmitFeedToAmazon(feedSubmission);

			feedSubmission.LastSubmitted = DateTime.UtcNow;
			feedSubmissionService.Update(feedSubmission);
			feedSubmissionService.SaveChanges();

			if (string.IsNullOrEmpty(feedSubmissionId))
			{
				_feedSubmissionProcessor.MoveToRetryQueue(feedSubmissionService, feedSubmission);
				_logger.Warn($"AmazonMWS feed submission request failed for {feedSubmission.RegionAndTypeComputed}");
			}
			else if (feedSubmissionId == HttpStatusCode.BadRequest.ToString())
			{
				_feedSubmissionProcessor.RemoveFromQueue(feedSubmissionService, feedSubmission);
				_logger.Warn($"AmazonMWS feed submission failed for {feedSubmission.RegionAndTypeComputed}. The feed submission request was removed from queue.");
			}
			else
			{
				_feedSubmissionProcessor.MoveToQueueOfSubmittedFeeds(feedSubmissionService, feedSubmission, feedSubmissionId);
				_logger.Info($"AmazonMWS feed submission request succeeded for {feedSubmission.RegionAndTypeComputed}. FeedSubmissionId:'{feedSubmissionId}'");
			}

		}

		public void RequestFeedSubmissionStatusesFromAmazon(IFeedSubmissionCallbackService feedSubmissionService)
		{
			var feedSubmissionIds = _feedSubmissionProcessor.GetIdsForSubmittedFeedsFromQueue(feedSubmissionService).ToList();

			if (!feedSubmissionIds.Any())
				return;

			var feedSubmissionResults = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(feedSubmissionIds, _merchantId);

			if (feedSubmissionResults != null)
			{
				_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(feedSubmissionService, feedSubmissionResults);
			}
		}

		public void RequestNextFeedSubmissionInQueueFromAmazon(IFeedSubmissionCallbackService feedSubmissionService)
		{
			var nextFeedWithProcessingComplete = _feedSubmissionProcessor.GetNextFromQueueOfProcessingCompleteFeeds(feedSubmissionService);
			if (nextFeedWithProcessingComplete == null) return;

			var processingReportInfo = _feedSubmissionProcessor.GetFeedSubmissionResultFromAmazon(nextFeedWithProcessingComplete);
			if (processingReportInfo.processingReport == null)
			{
				_logger.Warn($"AmazonMWS feed submission result request failed for {nextFeedWithProcessingComplete.RegionAndTypeComputed}");
				return;
			}

			var hasValidHash = IsValidFeedSubmissionReportHash(feedSubmissionService, (nextFeedWithProcessingComplete, processingReportInfo.processingReport, processingReportInfo.md5hash));
			if (hasValidHash)
			{
				nextFeedWithProcessingComplete.Details.FeedContent = null;

				using (var streamReader = new StreamReader(processingReportInfo.processingReport))
				{
					var reportContent = streamReader.ReadToEnd();
					var zippedProcessingReport = ZipHelper.CreateArchiveFromContent(reportContent);
					nextFeedWithProcessingComplete.Details.FeedSubmissionReport = zippedProcessingReport;
				}

				feedSubmissionService.Update(nextFeedWithProcessingComplete);
				feedSubmissionService.SaveChanges();
			}
		}

		public void ExecuteMethodCallback(FeedSubmissionEntry feedSubmission, Stream stream)
		{
			_logger.Info(
				$"Attempting to perform method callback for the next submitted feed in queue : {feedSubmission.RegionAndTypeComputed}.");

			var callback = new Callback(feedSubmission.TypeName, feedSubmission.MethodName,
				feedSubmission.Data, feedSubmission.DataTypeName);

			_callbackActivator.CallMethod(callback, stream);
		}
	}
}
