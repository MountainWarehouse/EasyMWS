using System;
using System.IO;
using System.Linq;
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
	internal class FeedProcessor : IQueueingProcessor<FeedSubmissionPropertiesContainer>, IFeedProcessor
	{
		private readonly IFeedSubmissionCallbackService _feedService;
		private readonly IFeedSubmissionProcessor _feedSubmissionProcessor;
		private readonly ICallbackActivator _callbackActivator;
		private readonly IEasyMwsLogger _logger;

		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

		public event EventHandler<FeedSubmittedEventArgs> FeedSubmitted;

		internal FeedProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options, IFeedSubmissionCallbackService feedService, IMarketplaceWebServiceClient mwsClient, IFeedSubmissionProcessor feedSubmissionProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
		  : this(region, merchantId, options, mwsClient, logger)
		{
			_feedService = feedService;
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

			_feedService = _feedService ?? new FeedSubmissionCallbackService(options: _options, logger: logger);
			_feedSubmissionProcessor = _feedSubmissionProcessor ?? new FeedSubmissionProcessor(_region, _merchantId, mwsClient, _feedService, _logger, _options);
		}

		public void Poll()
		{
			try
			{
				_feedSubmissionProcessor.CleanUpFeedSubmissionQueue();

				SubmitNextFeedInQueueToAmazon();
				_feedService.SaveChanges();

				RequestFeedSubmissionStatusesFromAmazon();
				_feedService.SaveChanges();

				var reportInfo = RequestNextFeedSubmissionInQueueFromAmazon();
				_feedService.SaveChanges();

				if (reportInfo.reportContent != null) ProcessReportInfo(reportInfo);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		private void ProcessReportInfo((FeedSubmissionEntry feedSubmissionCallback, Stream reportContent, string contentMd5) reportInfo)
		{
			if (MD5ChecksumHelper.IsChecksumCorrect(reportInfo.reportContent, reportInfo.contentMd5))
			{
				PerformCallback(reportInfo.feedSubmissionCallback, reportInfo.reportContent);
				_logger.Info($"Checksum verification succeeded for feed submission report for {reportInfo.feedSubmissionCallback.RegionAndTypeComputed}");
			}
			else
			{
				_logger.Warn($"Checksum verification failed for feed submission report for {reportInfo.feedSubmissionCallback.RegionAndTypeComputed}");
				_feedSubmissionProcessor.MoveToRetryQueue(reportInfo.feedSubmissionCallback);
				_feedService.SaveChanges();
			}
		}

		private void PerformCallback(FeedSubmissionEntry feedSubmission, Stream feedSubmissionReport)
		{
			try
			{
				if (feedSubmission.MethodName != null)
				{
					ExecuteMethodCallback(feedSubmission, feedSubmissionReport);
				}
				else
				{
					InvokeFeedSubmittedEvent(feedSubmission, feedSubmissionReport);
				}
				_feedSubmissionProcessor.RemoveFromQueue(feedSubmission.Id);
				_feedService.SaveChanges();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void Queue(FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			try
			{
				_feedService.Create(GetSerializedFeedSubmissionCallback(propertiesContainer, callbackMethod, callbackData));
				_feedService.SaveChanges();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void Queue(FeedSubmissionPropertiesContainer propertiesContainer)
		{
			Queue(propertiesContainer, null, null);
		}

		public void SubmitNextFeedInQueueToAmazon()
		{
			var feedSubmission = _feedSubmissionProcessor.GetNextFromQueueOfFeedsToSubmit();

			if (feedSubmission == null) return;
			
			try
			{
				var feedSubmissionId = _feedSubmissionProcessor.SubmitFeedToAmazon(feedSubmission);

				feedSubmission.LastSubmitted = DateTime.UtcNow;
				_feedService.Update(feedSubmission);

				if (string.IsNullOrEmpty(feedSubmissionId))
				{
					_feedSubmissionProcessor.MoveToRetryQueue(feedSubmission);
					_logger.Warn($"AmazonMWS feed submission request failed for {feedSubmission.RegionAndTypeComputed}");
				}
				else
				{
					_feedSubmissionProcessor.MoveToQueueOfSubmittedFeeds(feedSubmission, feedSubmissionId);
					_logger.Info($"AmazonMWS feed submission request succeeded for {feedSubmission.RegionAndTypeComputed}. FeedSubmissionId:'{feedSubmissionId}'");
				}
			}
			catch (Exception e)
			{
				_feedSubmissionProcessor.MoveToRetryQueue(feedSubmission);
				_logger.Error(e.Message, e);
			}
		}

		public void RequestFeedSubmissionStatusesFromAmazon()
		{
			try
			{
				var feedSubmissionIds = _feedSubmissionProcessor.GetIdsForSubmittedFeedsFromQueue().ToList();

				if (!feedSubmissionIds.Any())
					return;

				var feedSubmissionResults = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(feedSubmissionIds, _merchantId);

				_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(feedSubmissionResults);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public (FeedSubmissionEntry feedSubmissionCallback, Stream reportContent, string contentMd5) RequestNextFeedSubmissionInQueueFromAmazon()
		{
			var nextFeedWithProcessingComplete = _feedSubmissionProcessor.GetNextFromQueueOfProcessingCompleteFeeds();
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

		private void InvokeFeedSubmittedEvent(FeedSubmissionEntry feedSubmission, Stream reportContent)
		{
			_logger.Info(
				$"Invoking FeedSubmitted event for the next submitted feed in queue : {feedSubmission.RegionAndTypeComputed}.");

			var feedPropertiesContainer = feedSubmission.GetPropertiesContainer();
			FeedSubmitted?.Invoke(this, new FeedSubmittedEventArgs
			{
				FeedSubmissionReport = reportContent,
				AmazonRegion = feedSubmission.AmazonRegion,
				MerchantId = feedSubmission.MerchantId,
				FeedSubmissionId = feedSubmission.FeedSubmissionId,
				FeedType = feedPropertiesContainer.FeedType
			});
		}

		private FeedSubmissionEntry GetSerializedFeedSubmissionCallback(
		  FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			if (propertiesContainer == null) throw new ArgumentNullException();
			
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			var feedSubmission = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = _region,
				MerchantId = _merchantId,
				LastSubmitted = DateTime.MinValue,
				IsProcessingComplete = false,
				HasErrors = false,
				SubmissionErrorData = null,
				SubmissionRetryCount = 0,
				FeedSubmissionId = null,
				FeedType = propertiesContainer.FeedType
			};

			if (callbackMethod != null)
			{
				var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);
				feedSubmission.Data = serializedCallback.Data;
				feedSubmission.TypeName = serializedCallback.TypeName;
				feedSubmission.MethodName = serializedCallback.MethodName;
				feedSubmission.DataTypeName = serializedCallback.DataTypeName;
			}

			return feedSubmission;
		}
	}
}
