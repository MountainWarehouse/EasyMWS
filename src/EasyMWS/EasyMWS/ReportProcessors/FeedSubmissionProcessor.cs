using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.ReportProcessors
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

		public FeedSubmissionCallback GetNextFeedToSubmitFromQueue(AmazonRegion region, string merchantId) =>
			string.IsNullOrEmpty(merchantId) ? null : _feedSubmissionCallbackService.GetAll()
				.FirstOrDefault(fscs => fscs.AmazonRegion == region && fscs.MerchantId == merchantId
				&& IsFeedInASubmitFeedQueue(fscs)
				&& IsFeedReadyForSubmission(fscs));

		public string SubmitSingleQueuedFeedToAmazon(FeedSubmissionCallback feedSubmission, string merchantId)
		{
			if(feedSubmission == null || string.IsNullOrEmpty(merchantId))
				throw new ArgumentNullException("Cannot submit queued feed to amazon due to missing feed submission information or empty merchant ID");

			var feedSubmissionData = JsonConvert.DeserializeObject<FeedSubmissionPropertiesContainer>(feedSubmission.FeedSubmissionData);
			var submitFeedRequest = new SubmitFeedRequest
			{
				Merchant = merchantId,
				FeedType = feedSubmissionData.FeedType,
				FeedContent = StreamHelper.CreateNewMemoryStream(feedSubmissionData.FeedContent),
				MarketplaceIdList = feedSubmissionData.MarketplaceIdList == null ? null : new IdList { Id = feedSubmissionData.MarketplaceIdList },
				PurgeAndReplace = feedSubmissionData.PurgeAndReplace ?? false,
				ContentMD5 = feedSubmissionData.ContentMD5Value
			};

			var response = _marketplaceWebServiceClient.SubmitFeed(submitFeedRequest);

			return response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId;
		}

		public void AllocateFeedSubmissionForRetry(FeedSubmissionCallback feedSubmission)
		{
			feedSubmission.SubmissionRetryCount++;
			_feedSubmissionCallbackService.Update(feedSubmission);
		}

		public void MoveToQueueOfSubmittedFeeds(FeedSubmissionCallback feedSubmission, string feedSubmissionId)
		{
			feedSubmission.FeedSubmissionId = feedSubmissionId;
			feedSubmission.SubmissionRetryCount = 0;
			_feedSubmissionCallbackService.Update(feedSubmission);
		}

		public IEnumerable<FeedSubmissionCallback> GetAllSubmittedFeeds(AmazonRegion region, string merchantId) =>
			string.IsNullOrEmpty(merchantId) ? new List<FeedSubmissionCallback>().AsEnumerable() : _feedSubmissionCallbackService.Where(
				rrcs => rrcs.AmazonRegion == region && rrcs.MerchantId == merchantId
				        && rrcs.FeedSubmissionId != null
						&& rrcs.IsProcessingComplete == false
				);

		public List<(string FeedSubmissionId, string FeedProcessingStatus)> GetFeedSubmissionResults(IEnumerable<string> feedSubmissionIdList, string merchant)
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

		public void MoveFeedsToQueuesAccordingToProcessingStatus(List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses)
		{
			foreach (var feedInfo in feedProcessingStatuses)
			{
				var feedSubmissionCallback = _feedSubmissionCallbackService.FirstOrDefault(fsc => fsc.FeedSubmissionId == feedInfo.FeedSubmissionId);

				if (feedInfo.FeedProcessingStatus == "_DONE_")
				{
					feedSubmissionCallback.IsProcessingComplete = true;
					feedSubmissionCallback.SubmissionRetryCount = 0;
					_feedSubmissionCallbackService.Update(feedSubmissionCallback);
				} else if (feedInfo.FeedProcessingStatus == "_AWAITING_ASYNCHRONOUS_REPLY_"
				           || feedInfo.FeedProcessingStatus == "_IN_PROGRESS_"
				           || feedInfo.FeedProcessingStatus == "_IN_SAFETY_NET_"
				           || feedInfo.FeedProcessingStatus == "_SUBMITTED_"
				           || feedInfo.FeedProcessingStatus == "_UNCONFIRMED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.SubmissionRetryCount++;
					_feedSubmissionCallbackService.Update(feedSubmissionCallback);
				} else if (feedInfo.FeedProcessingStatus == "_CANCELLED_")
				{
					// TODO: log that the feed has been removed from Queue. investigate if it's worth it to move the feed to the initial queue.
					_feedSubmissionCallbackService.Delete(feedSubmissionCallback);
				}
				else
				{
					// TODO: log that the feed has been removed from Queue. investigate if it's worth it to move the feed to the initial queue.
					_feedSubmissionCallbackService.Delete(feedSubmissionCallback);
				}
			}
		}

		private bool IsFeedReadyForSubmission(FeedSubmissionCallback feedSubmission)
		{
			var isInInitialRetryStateAndReadyForRetry = feedSubmission.SubmissionRetryCount > 0
			        && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted, 
					feedSubmission.SubmissionRetryCount, _options.FeedInitialSubmissionRetryInitialDelay, 
					_options.FeedInitialSubmissionRetryInterval, RetryPeriodType.ArithmeticProgression);

			var isNotInRetryState = feedSubmission.SubmissionRetryCount == 0;

			return isInInitialRetryStateAndReadyForRetry || isNotInRetryState;
		}

		private bool IsFeedInASubmitFeedQueue(FeedSubmissionCallback feedSubmission)
		{
			return feedSubmission.FeedSubmissionId == null;
		}
	}
}
