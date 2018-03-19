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
				&& fscs.FeedSubmissionId == null
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
						&& rrcs.ResultReceived == false
				);

		private bool IsFeedReadyForSubmission(FeedSubmissionCallback feedSubmission)
		{
			return true;
		}
	}
}
