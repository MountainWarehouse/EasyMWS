using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class FeedSubmissionProcessor : IFeedSubmissionProcessor
	{
		private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
		private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
		private readonly AmazonRegion _region;
		private readonly string _merchantId;

		internal FeedSubmissionProcessor(AmazonRegion region, string merchantId, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
		}

		public FeedSubmissionEntry GetNextFromQueueOfFeedsToSubmit(IFeedSubmissionCallbackService feedSubmissionService) =>
			string.IsNullOrEmpty(_merchantId) ? null : feedSubmissionService.GetAll()
				.FirstOrDefault(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
				&& IsFeedInASubmitFeedQueue(fscs)
				&& IsFeedReadyForSubmission(fscs));

		public string SubmitFeedToAmazon(FeedSubmissionEntry feedSubmission)
		{
			var missingInformationExceptionMessage = "Cannot submit queued feed to amazon due to missing feed submission information";

			if (feedSubmission?.FeedSubmissionData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission data is null.");
			if (string.IsNullOrEmpty(feedSubmission.Details?.FeedContent)) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed content is missing.");
			if (string.IsNullOrEmpty(feedSubmission?.FeedType)) throw new ArgumentException($"{missingInformationExceptionMessage}: Feed type is missing.");

			_logger.Info($"Attempting to submit the next feed in queue to Amazon: {feedSubmission.RegionAndTypeComputed}.");

			var feedSubmissionData = feedSubmission.GetPropertiesContainer();

			using (var stream = StreamHelper.CreateNewMemoryStream(feedSubmission.Details.FeedContent))
			{
				var submitFeedRequest = new SubmitFeedRequest
				{
					Merchant = feedSubmission.MerchantId,
					FeedType = feedSubmission.FeedType,
					FeedContent = stream,
					MarketplaceIdList = feedSubmissionData.MarketplaceIdList == null ? null : new IdList {Id = feedSubmissionData.MarketplaceIdList},
					PurgeAndReplace = feedSubmissionData.PurgeAndReplace ?? false,
					ContentMD5 = MD5ChecksumHelper.ComputeHashForAmazon(stream)
				};

				var response = _marketplaceWebServiceClient.SubmitFeed(submitFeedRequest);
				stream.Close();

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info($"Request to MWS.SubmitFeed was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']", new RequestInfo(timestamp, requestId));

				return response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId;
			}
		}

		public void MoveToQueueOfSubmittedFeeds(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission, string feedSubmissionId)
		{
			feedSubmission.FeedSubmissionId = feedSubmissionId;
			feedSubmission.SubmissionRetryCount = 0;
			feedSubmissionService.Update(feedSubmission);
			feedSubmissionService.SaveChanges();

			_logger.Info($"Moving {feedSubmission.RegionAndTypeComputed} to queue of feed submissions that await processing results.");
		}

		public void MoveToRetryQueue(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission)
		{
			feedSubmission.SubmissionRetryCount++;
			feedSubmissionService.Update(feedSubmission);
			feedSubmissionService.SaveChanges();

			_logger.Warn($"Moving {feedSubmission.RegionAndTypeComputed} to retry queue. Retry count is now '{feedSubmission.SubmissionRetryCount}'.");
		}

		public IEnumerable<string> GetIdsForSubmittedFeedsFromQueue(IFeedSubmissionCallbackService feedSubmissionService) =>
			string.IsNullOrEmpty(_merchantId) ? new List<string>().AsEnumerable() : feedSubmissionService.Where(
				rrcs => rrcs.AmazonRegion == _region && rrcs.MerchantId == _merchantId
				        && rrcs.FeedSubmissionId != null
						&& rrcs.IsProcessingComplete == false
				).Select(f => f.FeedSubmissionId);

		public List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(IEnumerable<string> feedSubmissionIdList, string merchant)
		{
			_logger.Info($"Attempting to request feed submission statuses for all feeds in queue.");

			var request = new GetFeedSubmissionListRequest() {FeedSubmissionIdList = new IdList(), Merchant = merchant};
			request.FeedSubmissionIdList.Id.AddRange(feedSubmissionIdList);
			var response = _marketplaceWebServiceClient.GetFeedSubmissionList(request);

			var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			_logger.Info($"Request to MWS.GetFeedSubmissionList was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']", new RequestInfo(timestamp, requestId));

			var responseInfo = new List<(string FeedSubmissionId, string IsProcessingComplete)>();

			foreach (var feedSubmissionInfo in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
			{
				responseInfo.Add((feedSubmissionInfo.FeedSubmissionId, feedSubmissionInfo.FeedProcessingStatus));
			}

			_logger.Info($"AmazonMWS request for feed submission statuses succeeded.");
			return responseInfo;
		}

		public void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionCallbackService feedSubmissionService, List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses)
		{
			foreach (var feedSubmissionInfo in feedProcessingStatuses)
			{
				var feedSubmissionCallback = feedSubmissionService.FirstOrDefault(fsc => fsc.FeedSubmissionId == feedSubmissionInfo.FeedSubmissionId);
				if(feedSubmissionCallback == null) continue;

				if (feedSubmissionInfo.FeedProcessingStatus == "_DONE_")
				{
					feedSubmissionCallback.IsProcessingComplete = true;
					feedSubmissionCallback.SubmissionRetryCount = 0;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_AWAITING_ASYNCHRONOUS_REPLY_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_IN_PROGRESS_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_IN_SAFETY_NET_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_SUBMITTED_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_UNCONFIRMED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount = 0;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_CANCELLED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount++;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
				else
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount++;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
			}

			feedSubmissionService.SaveChanges();
		}

		public FeedSubmissionEntry GetNextFromQueueOfProcessingCompleteFeeds(IFeedSubmissionCallbackService feedSubmissionService)
			=> string.IsNullOrEmpty(_merchantId) ? null : feedSubmissionService.FirstOrDefault(
				ffscs => ffscs.AmazonRegion == _region && ffscs.MerchantId == _merchantId
				&& ffscs.FeedSubmissionId != null
				&& ffscs.IsProcessingComplete == true
				&& IsReadyForRequestingSubmissionResult(ffscs));

		public (Stream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionEntry feedSubmissionEntry)
		{
			_logger.Info($"Attempting to request the feed submission result for the next feed in queue from Amazon: {feedSubmissionEntry.RegionAndTypeComputed}.");

			var reportResultStream = new MemoryStream();
			var request = new GetFeedSubmissionResultRequest
			{
				FeedSubmissionId = feedSubmissionEntry.FeedSubmissionId,
				Merchant = feedSubmissionEntry.MerchantId,
				FeedSubmissionResult = reportResultStream
			};

			var response = _marketplaceWebServiceClient.GetFeedSubmissionResult(request);

			var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			_logger.Info($"Request to MWS.GetFeedSubmissionResult was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']", new RequestInfo(timestamp, requestId));
			_logger.Info($"Feed submission result request from Amazon has succeeded for {feedSubmissionEntry.RegionAndTypeComputed}.");

			return (reportResultStream, response?.GetFeedSubmissionResultResult?.ContentMD5);
		}

		public void RemoveFromQueue(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry entry)
		{
			feedSubmissionService.Delete(entry);
			feedSubmissionService.SaveChanges();
		}

		public void CleanUpFeedSubmissionQueue(IFeedSubmissionCallbackService feedSubmissionService)
		{
			_logger.Info("Executing cleanup of feed submission requests queue.");
			var expiredFeedSubmissions = feedSubmissionService.GetAll()
				.Where(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
							   && fscs.FeedSubmissionId == null
				               && fscs.SubmissionRetryCount > _options.FeedSubmissionMaxRetryCount);

			if (expiredFeedSubmissions.Any())
			{
				_logger.Warn("The following feed submission requests have exceeded their retry limit and will now be deleted :");
				foreach (var feedSubmission in expiredFeedSubmissions)
				{
					feedSubmissionService.Delete(feedSubmission);
					_logger.Warn($"Feed submission request {feedSubmission.RegionAndTypeComputed} deleted from queue.");
				}
			}
			

			var expiredFeedProcessingResultRequestIds = feedSubmissionService.GetAll()
				.Where(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
							   && fscs.FeedSubmissionId != null
				               && fscs.SubmissionRetryCount > _options.FeedResultFailedChecksumMaxRetryCount);

			foreach (var feedSubmission in expiredFeedProcessingResultRequestIds)
			{
				feedSubmissionService.Delete(feedSubmission);
			}

			feedSubmissionService.SaveChanges();
		}

		private bool IsFeedReadyForSubmission(FeedSubmissionEntry feedSubmission)
		{
			var isInRetryQueueAndReadyForRetry = feedSubmission.SubmissionRetryCount > 0
			        && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted, 
					feedSubmission.SubmissionRetryCount, _options.FeedSubmissionRetryInitialDelay, 
					_options.FeedSubmissionRetryInterval, _options.FeedSubmissionRetryType);

			var isNotInRetryState = feedSubmission.SubmissionRetryCount == 0;

			return isInRetryQueueAndReadyForRetry || isNotInRetryState;
		}

		private bool IsReadyForRequestingSubmissionResult(FeedSubmissionEntry feedSubmission)
		{
			var isInRetryQueueAndReadyForRetry = feedSubmission.SubmissionRetryCount > 0
			        && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted,
				        feedSubmission.SubmissionRetryCount, _options.FeedResultFailedChecksumRetryInterval,
				        _options.FeedResultFailedChecksumRetryInterval, RetryPeriodType.ArithmeticProgression);

			var isNotInRetryState = feedSubmission.SubmissionRetryCount == 0;

			return isInRetryQueueAndReadyForRetry || isNotInRetryState;
		}

		private bool IsFeedInASubmitFeedQueue(FeedSubmissionEntry feedSubmission)
		{
			return feedSubmission.FeedSubmissionId == null;
		}
	}
}
