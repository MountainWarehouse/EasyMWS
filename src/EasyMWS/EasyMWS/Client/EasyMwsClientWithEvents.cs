using System;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;

namespace MountainWarehouse.EasyMWS.Client
{
	/// <summary>
	/// This EasyMws client can handle QueueReport, QueueFeed and Poll,
	/// additionally it exposes ReportDownloaded and FeedSubmitted events that are fired as soon as the corresponding action has taken place.
	/// </summary>
	public class EasyMwsClientWithEvents : IEasyMwsClientWithEvents
	{
		public event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;
		public event EventHandler<FeedSubmittedEventArgs> FeedSubmitted;

		private readonly EasyMwsOptions _options;
		private readonly AmazonRegion _amazonRegion;
		private readonly string _merchantId;
		private readonly IQueueingProcessor<ReportRequestPropertiesContainer> _reportProcessor;
		private readonly IQueueingProcessor<FeedSubmissionPropertiesContainer> _feedProcessor;
		private IEasyMwsLogger _logger;

		internal EasyMwsClientWithEvents(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey,
			IQueueingProcessor<ReportRequestPropertiesContainer> reportProcessor,
			IQueueingProcessor<FeedSubmissionPropertiesContainer> feedProcessor, IEasyMwsLogger easyMwsLogger,
			EasyMwsOptions options)
			: this(region, merchantId, accessKeyId, mwsSecretAccessKey, easyMwsLogger, options)
		{
			_reportProcessor = reportProcessor;
			_feedProcessor = feedProcessor;
		}

		/// <param name="region">The region of the account</param>
		/// <param name="merchantId">Seller ID. Required parameter.</param>
		/// <param name="accessKeyId">Your specific access key. Required parameter.</param>
		/// <param name="mwsSecretAccessKey">Your specific secret access key. Required parameter.</param>
		/// <param name="easyMwsLogger">An optional IEasyMwsLogger instance that can provide access to logs. It is strongly recommended to use a logger implementation already existing in the EasyMws package.</param>
		/// <param name="options">Configuration options for EasyMwsClient</param>
		public EasyMwsClientWithEvents(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey,
			IEasyMwsLogger easyMwsLogger = null, EasyMwsOptions options = null)
		{
			if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(accessKeyId) ||
				string.IsNullOrEmpty(mwsSecretAccessKey))
				throw new ArgumentNullException(
					"One or more required parameters provided to initialize the EasyMwsClient were null or empty.");

			_amazonRegion = region;
			_merchantId = merchantId;
			_options = options ?? EasyMwsOptions.Defaults();

			_logger = easyMwsLogger ?? new EasyMwsLogger(isEnabled: false);
			var mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(_amazonRegion));
			_reportProcessor = _reportProcessor ?? new ReportProcessor(_amazonRegion, _merchantId, _options, mwsClient, _logger);
			(_reportProcessor as IReportProcessor).ReportDownloaded += (sender, args) => { ReportDownloaded?.Invoke(sender, args); };

			_feedProcessor = _feedProcessor ?? new FeedProcessor(_amazonRegion, _merchantId, _options, mwsClient, _logger);
			(_feedProcessor as IFeedProcessor).FeedSubmitted += (sender, args) => { FeedSubmitted?.Invoke(sender, args); };
		}

		public void Poll()
		{
			_reportProcessor.Poll();
			_feedProcessor.Poll();
		}

		public void QueueReport(ReportRequestPropertiesContainer reportRequestContainer)
		{
			_reportProcessor.Queue(reportRequestContainer);
		}

		public void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer)
		{
			_feedProcessor.Queue(feedSubmissionContainer);
		}

		#region Helpers for creating the MarketplaceWebServiceClient

		private MarketplaceWebServiceConfig CreateConfig(AmazonRegion region)
		{
			string rootUrl;
			switch (region)
			{
				case AmazonRegion.Australia:
					rootUrl = MwsEndpoint.Australia.RegionOrMarketPlaceEndpoint;
					break;
				case AmazonRegion.China:
					rootUrl = MwsEndpoint.China.RegionOrMarketPlaceEndpoint;
					break;
				case AmazonRegion.Europe:
					rootUrl = MwsEndpoint.Europe.RegionOrMarketPlaceEndpoint;
					break;
				case AmazonRegion.India:
					rootUrl = MwsEndpoint.India.RegionOrMarketPlaceEndpoint;
					break;
				case AmazonRegion.Japan:
					rootUrl = MwsEndpoint.Japan.RegionOrMarketPlaceEndpoint;
					break;
				case AmazonRegion.NorthAmerica:
					rootUrl = MwsEndpoint.NorthAmerica.RegionOrMarketPlaceEndpoint;
					break;
				case AmazonRegion.Brazil:
					rootUrl = MwsEndpoint.Brazil.RegionOrMarketPlaceEndpoint;
					break;
				default:
					throw new ArgumentException($"{region} is unknown - EasyMWS doesn't know the RootURL");
			}


			var config = new MarketplaceWebServiceConfig
			{
				ServiceURL = rootUrl
			};
			config = config.WithUserAgent("EasyMWS");

			return config;
		}

		#endregion
	}
}
