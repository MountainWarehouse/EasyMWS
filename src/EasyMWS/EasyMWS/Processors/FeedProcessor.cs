using System;
using System.IO;
using System.Linq;
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

			_feedService = _feedService ?? new FeedSubmissionCallbackService();
			_feedSubmissionProcessor = _feedSubmissionProcessor ?? new FeedSubmissionProcessor(mwsClient, _feedService, options);
		}

		public void Poll()
		{
			try
			{
				CleanUpFeedSubmissionQueue();
				SubmitNextFeedInQueueToAmazon();
				_feedService.SaveChanges();

				RequestFeedSubmissionStatusesFromAmazon();
				_feedService.SaveChanges();

				var amazonProcessingReport = RequestNextFeedSubmissionInQueueFromAmazon();
				_feedService.SaveChanges();

				// TODO: log a warning for each hash miss-match, and recommend to the user to notify Amazon that a corrupted body was received. 
				if (amazonProcessingReport.feedSubmissionCallback != null)
				{
					if (MD5ChecksumHelper.IsChecksumCorrect(amazonProcessingReport.reportContent, amazonProcessingReport.contentMd5))
					{
						ExecuteCallback(amazonProcessingReport.feedSubmissionCallback, amazonProcessingReport.reportContent);
					}
					else
					{
						_feedSubmissionProcessor.MoveToRetryQueue(amazonProcessingReport.feedSubmissionCallback);
					}
				}
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

		public void CleanUpFeedSubmissionQueue()
		{
			var expiredFeedSubmissions = _feedService.GetAll()
				.Where(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
				               && fscs.FeedSubmissionId == null
				               && fscs.SubmissionRetryCount > _options.FeedSubmissionMaxRetryCount);

			foreach (var feedSubmission in expiredFeedSubmissions)
			{
				_feedService.Delete(feedSubmission);
			}

			var expiredFeedProcessingResultRequests = _feedService.GetAll()
				.Where(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
				               && fscs.FeedSubmissionId != null
				               && fscs.SubmissionRetryCount > _options.FeedResultFailedChecksumMaxRetryCount);

			foreach (var feedSubmission in expiredFeedProcessingResultRequests)
			{
				_feedService.Delete(feedSubmission);
			}
		}

		public void SubmitNextFeedInQueueToAmazon()
		{
			var feedSubmission = _feedSubmissionProcessor.GetNextFromQueueOfFeedsToSubmit(_region, _merchantId);

			if (feedSubmission == null) return;

			try
			{
				var feedSubmissionId = _feedSubmissionProcessor.SubmitFeedToAmazon(feedSubmission);

				feedSubmission.LastSubmitted = DateTime.UtcNow;
				_feedService.Update(feedSubmission);

				if (string.IsNullOrEmpty(feedSubmissionId))
				{
					_feedSubmissionProcessor.MoveToRetryQueue(feedSubmission);
				}
				else
				{
					_feedSubmissionProcessor.MoveToQueueOfSubmittedFeeds(feedSubmission, feedSubmissionId);
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
				var submittedFeeds = _feedSubmissionProcessor.GetAllSubmittedFeedsFromQueue(_region, _merchantId).ToList();

				if (!submittedFeeds.Any())
					return;

				var feedSubmissionIdList = submittedFeeds.Select(x => x.FeedSubmissionId);

				var feedSubmissionResults = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(feedSubmissionIdList, _merchantId);

				_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(feedSubmissionResults);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public (FeedSubmissionCallback feedSubmissionCallback, Stream reportContent, string contentMd5) RequestNextFeedSubmissionInQueueFromAmazon()
		{
			try
			{
				var nextFeedWithProcessingComplete =
					_feedSubmissionProcessor.GetNextFromQueueOfProcessingCompleteFeeds(_region, _merchantId);

				if (nextFeedWithProcessingComplete == null) return (null, null, null);

				var processingReportInfo = _feedSubmissionProcessor.GetFeedSubmissionResultFromAmazon(nextFeedWithProcessingComplete);

				return (nextFeedWithProcessingComplete, processingReportInfo.processingReport, processingReportInfo.md5hash);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
				return (null, null, null);
			}
		}

		public void ExecuteCallback(FeedSubmissionCallback feedSubmissionCallback, Stream stream)
		{
			try
			{
				if (feedSubmissionCallback == null || stream == null) return;

				var callback = new Callback(feedSubmissionCallback.TypeName, feedSubmissionCallback.MethodName,
					feedSubmissionCallback.Data, feedSubmissionCallback.DataTypeName);

				_callbackActivator.CallMethod(callback, stream);

				_feedSubmissionProcessor.RemoveFromQueue(feedSubmissionCallback);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		private FeedSubmissionCallback GetSerializedFeedSubmissionCallback(
		  FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			if (propertiesContainer == null || callbackMethod == null) throw new ArgumentNullException();
			var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);

			return new FeedSubmissionCallback(serializedCallback)
			{
				AmazonRegion = _region,
				MerchantId = _merchantId,
				LastSubmitted = DateTime.MinValue,
				IsProcessingComplete = false,
				HasErrors = false,
				SubmissionErrorData = null,
				SubmissionRetryCount = 0,
				FeedSubmissionId = null,
				FeedSubmissionData = JsonConvert.SerializeObject(propertiesContainer),
			};
		}
	}
}
