using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class FeedSubmissionProcessor : IFeedSubmissionProcessor
	{
		private readonly IFeedSubmissionCallbackService _feedSubmissionCallbackService;
		private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
		private readonly EasyMwsOptions _options;

		internal FeedSubmissionProcessor(IMarketplaceWebServiceClient marketplaceWebServiceClient,
			IFeedSubmissionCallbackService feedSubmissionCallbackService, EasyMwsOptions options) : this(
			marketplaceWebServiceClient, options)
		{
			_feedSubmissionCallbackService = feedSubmissionCallbackService;
		}

		internal FeedSubmissionProcessor(IMarketplaceWebServiceClient marketplaceWebServiceClient, EasyMwsOptions options)
		{
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
			_feedSubmissionCallbackService = _feedSubmissionCallbackService ?? new FeedSubmissionCallbackService();
			_options = options;
		}

		public FeedSubmissionCallback GetNextFromQueueOfFeedsToSubmit(AmazonRegion region, string merchantId) =>
			string.IsNullOrEmpty(merchantId) ? null : _feedSubmissionCallbackService.GetAll()
				.FirstOrDefault(fscs => fscs.AmazonRegion == region && fscs.MerchantId == merchantId
				&& IsFeedInASubmitFeedQueue(fscs)
				&& IsFeedReadyForSubmission(fscs));

		public string SubmitFeedToAmazon(FeedSubmissionCallback feedSubmission)
		{
			var missingInformationExceptionMessage = "Cannot submit queued feed to amazon due to missing feed submission information";

			if (feedSubmission?.FeedSubmissionData == null) throw new ArgumentNullException(missingInformationExceptionMessage);

			var feedSubmissionData = JsonConvert.DeserializeObject<FeedSubmissionPropertiesContainer>(feedSubmission.FeedSubmissionData);
			if (feedSubmissionData?.FeedType == null) throw new ArgumentException(missingInformationExceptionMessage);

			using (var stream = StreamHelper.CreateNewMemoryStream(feedSubmissionData.FeedContent))
			{
				var submitFeedRequest = new SubmitFeedRequest
				{
					Merchant = feedSubmission.MerchantId,
					FeedType = feedSubmissionData.FeedType,
					FeedContent = stream,
					MarketplaceIdList = feedSubmissionData.MarketplaceIdList == null ? null : new IdList {Id = feedSubmissionData.MarketplaceIdList},
					PurgeAndReplace = feedSubmissionData.PurgeAndReplace ?? false,
					ContentMD5 = MD5ChecksumHelper.ComputeHashForAmazon(stream)
				};

				var response = _marketplaceWebServiceClient.SubmitFeed(submitFeedRequest);

				return response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId;
			}
		}

		public void MoveToQueueOfSubmittedFeeds(FeedSubmissionCallback feedSubmission, string feedSubmissionId)
		{
			feedSubmission.FeedSubmissionId = feedSubmissionId;
			feedSubmission.SubmissionRetryCount = 0;
			_feedSubmissionCallbackService.Update(feedSubmission);
		}

		public void MoveToRetryQueue(FeedSubmissionCallback feedSubmission)
		{
			feedSubmission.SubmissionRetryCount++;
			_feedSubmissionCallbackService.Update(feedSubmission);
		}

		public IEnumerable<FeedSubmissionCallback> GetAllSubmittedFeedsFromQueue(AmazonRegion region, string merchantId) =>
			string.IsNullOrEmpty(merchantId) ? new List<FeedSubmissionCallback>().AsEnumerable() : _feedSubmissionCallbackService.Where(
				rrcs => rrcs.AmazonRegion == region && rrcs.MerchantId == merchantId
				        && rrcs.FeedSubmissionId != null
						&& rrcs.IsProcessingComplete == false
				);

		public List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(IEnumerable<string> feedSubmissionIdList, string merchant)
		{
			var request = new GetFeedSubmissionListRequest() {FeedSubmissionIdList = new IdList(), Merchant = merchant};
			request.FeedSubmissionIdList.Id.AddRange(feedSubmissionIdList);
			var response = _marketplaceWebServiceClient.GetFeedSubmissionList(request);

			var responseInfo = new List<(string FeedSubmissionId, string IsProcessingComplete)>();

			foreach (var feedSubmissionInfo in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
			{
				responseInfo.Add((feedSubmissionInfo.FeedSubmissionId, feedSubmissionInfo.FeedProcessingStatus));
			}

			return responseInfo;
		}

		public void QueueFeedsAccordingToProcessingStatus(List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses)
		{
			foreach (var feedSubmissionInfo in feedProcessingStatuses)
			{
				var feedSubmissionCallback = _feedSubmissionCallbackService.FirstOrDefault(fsc => fsc.FeedSubmissionId == feedSubmissionInfo.FeedSubmissionId);
				if(feedSubmissionCallback == null) continue;

				if (feedSubmissionInfo.FeedProcessingStatus == "_DONE_")
				{
					feedSubmissionCallback.IsProcessingComplete = true;
					feedSubmissionCallback.SubmissionRetryCount = 0;
					_feedSubmissionCallbackService.Update(feedSubmissionCallback);
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_AWAITING_ASYNCHRONOUS_REPLY_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_IN_PROGRESS_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_IN_SAFETY_NET_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_SUBMITTED_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_UNCONFIRMED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount = 0;
					_feedSubmissionCallbackService.Update(feedSubmissionCallback);
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_CANCELLED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount++;
					_feedSubmissionCallbackService.Update(feedSubmissionCallback);
				}
				else
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount++;
					_feedSubmissionCallbackService.Update(feedSubmissionCallback);
				}
			}
		}

		public FeedSubmissionCallback GetNextFromQueueOfProcessingCompleteFeeds(AmazonRegion region, string merchant)
			=> string.IsNullOrEmpty(merchant) ? null : _feedSubmissionCallbackService.FirstOrDefault(
				ffscs => ffscs.AmazonRegion == region && ffscs.MerchantId == merchant
				&& ffscs.FeedSubmissionId != null
				&& ffscs.IsProcessingComplete == true
				&& IsReadyForRequestingSubmissionResult(ffscs));

		public (Stream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionCallback feedSubmissionCallback)
		{
			var reportResultStream = new MemoryStream();
			var request = new GetFeedSubmissionResultRequest
			{
				FeedSubmissionId = feedSubmissionCallback.FeedSubmissionId,
				Merchant = feedSubmissionCallback.MerchantId,
				FeedSubmissionResult = reportResultStream
			};

			var response = _marketplaceWebServiceClient.GetFeedSubmissionResult(request);

			return (reportResultStream, response.GetFeedSubmissionResultResult.ContentMD5);
		}

		public void RemoveFromQueue(FeedSubmissionCallback feedSubmissionCallback)
		{
			_feedSubmissionCallbackService.Delete(feedSubmissionCallback);
		}

		private bool IsFeedReadyForSubmission(FeedSubmissionCallback feedSubmission)
		{
			var isInRetryQueueAndReadyForRetry = feedSubmission.SubmissionRetryCount > 0
			        && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted, 
					feedSubmission.SubmissionRetryCount, _options.FeedSubmissionRetryInitialDelay, 
					_options.FeedSubmissionRetryInterval, _options.FeedSubmissionRetryType);

			var isNotInRetryState = feedSubmission.SubmissionRetryCount == 0;

			return isInRetryQueueAndReadyForRetry || isNotInRetryState;
		}

		private bool IsReadyForRequestingSubmissionResult(FeedSubmissionCallback feedSubmission)
		{
			var isInRetryQueueAndReadyForRetry = feedSubmission.SubmissionRetryCount > 0
			        && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted,
				        feedSubmission.SubmissionRetryCount, _options.FeedResultFailedChecksumRetryInterval,
				        _options.FeedResultFailedChecksumRetryInterval, RetryPeriodType.ArithmeticProgression);

			var isNotInRetryState = feedSubmission.SubmissionRetryCount == 0;

			return isInRetryQueueAndReadyForRetry || isNotInRetryState;
		}

		private bool IsFeedInASubmitFeedQueue(FeedSubmissionCallback feedSubmission)
		{
			return feedSubmission.FeedSubmissionId == null;
		}
	}
}
