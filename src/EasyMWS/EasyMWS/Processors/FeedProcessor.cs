using System;
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
			try
			{
				_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(feedSubmissionService);

				SubmitNextFeedInQueueToAmazon(feedSubmissionService);

				RequestFeedSubmissionStatusesFromAmazon(feedSubmissionService);

				var reportInfo = RequestNextFeedSubmissionInQueueFromAmazon(feedSubmissionService);

				if (reportInfo.reportContent != null) ProcessReportInfo(feedSubmissionService, reportInfo);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		private void ProcessReportInfo(IFeedSubmissionCallbackService feedSubmissionService, (FeedSubmissionEntry feedSubmissionCallback, Stream reportContent, string contentMd5) reportInfo)
		{
			if (MD5ChecksumHelper.IsChecksumCorrect(reportInfo.reportContent, reportInfo.contentMd5))
			{
				PerformCallback(feedSubmissionService, reportInfo.feedSubmissionCallback, reportInfo.reportContent);
				_logger.Info($"Checksum verification succeeded for feed submission report for {reportInfo.feedSubmissionCallback.RegionAndTypeComputed}");
			}
			else
			{
				_logger.Warn($"Checksum verification failed for feed submission report for {reportInfo.feedSubmissionCallback.RegionAndTypeComputed}");
				_feedSubmissionProcessor.MoveToRetryQueue(feedSubmissionService, reportInfo.feedSubmissionCallback);
			}
		}

		private void PerformCallback(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission, Stream feedSubmissionReport)
		{
			try
			{
				ExecuteMethodCallback(feedSubmission, feedSubmissionReport);
				_feedSubmissionProcessor.RemoveFromQueue(feedSubmissionService, feedSubmission);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
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
						FeedContent = propertiesContainer.FeedContent
					}
				};
				
				var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);
				feedSubmission.Data = serializedCallback.Data;
				feedSubmission.TypeName = serializedCallback.TypeName;
				feedSubmission.MethodName = serializedCallback.MethodName;
				feedSubmission.DataTypeName = serializedCallback.DataTypeName;

				feedSubmissionService.Create(feedSubmission);
				feedSubmissionService.SaveChanges();

				_logger.Info($"EasyMwsClient: The following feed was queued for submission to Amazon {feedSubmission.RegionAndTypeComputed}.");
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
			
			try
			{
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
			catch (Exception e)
			{
				_feedSubmissionProcessor.MoveToRetryQueue(feedSubmissionService, feedSubmission);
				_logger.Error(e.Message, e);
			}
		}

		public void RequestFeedSubmissionStatusesFromAmazon(IFeedSubmissionCallbackService feedSubmissionService)
		{
			try
			{
				var feedSubmissionIds = _feedSubmissionProcessor.GetIdsForSubmittedFeedsFromQueue(feedSubmissionService).ToList();

				if (!feedSubmissionIds.Any())
					return;

				var feedSubmissionResults = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(feedSubmissionIds, _merchantId);

				_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(feedSubmissionService, feedSubmissionResults);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public (FeedSubmissionEntry feedSubmissionCallback, Stream reportContent, string contentMd5) RequestNextFeedSubmissionInQueueFromAmazon(IFeedSubmissionCallbackService feedSubmissionService)
		{
			var nextFeedWithProcessingComplete = _feedSubmissionProcessor.GetNextFromQueueOfProcessingCompleteFeeds(feedSubmissionService);
			if (nextFeedWithProcessingComplete == null) return (null, null, null);

			try
			{
				var processingReportInfo = _feedSubmissionProcessor.GetFeedSubmissionResultFromAmazon(nextFeedWithProcessingComplete);

				return (nextFeedWithProcessingComplete, processingReportInfo.processingReport, processingReportInfo.md5hash);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
				return (null, null, null);
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
