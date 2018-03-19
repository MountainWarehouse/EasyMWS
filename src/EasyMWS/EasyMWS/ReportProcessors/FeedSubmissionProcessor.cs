using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketplaceWebService;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Services;

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

		private bool IsFeedReadyForSubmission(FeedSubmissionCallback feedSubmission)
		{
			return true;
		}
	}
}
