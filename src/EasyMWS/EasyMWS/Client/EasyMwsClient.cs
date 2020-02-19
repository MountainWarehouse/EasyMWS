using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;

namespace MountainWarehouse.EasyMWS.Client
{
    public class EasyMwsClient : IEasyMwsClient
	{
		private readonly EasyMwsOptions _options;
		private readonly AmazonRegion _amazonRegion;
		private readonly string _merchantId;
		private readonly IReportQueueingProcessor _reportProcessor;
		private readonly IFeedQueueingProcessor _feedProcessor;
		private readonly IEasyMwsLogger _easyMwsLogger;

        public AmazonRegion AmazonRegion => _amazonRegion;
        public string MerchantId => _merchantId;
        public EasyMwsOptions Options => _options;
        public event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;
        public event EventHandler<FeedUploadedEventArgs> FeedUploaded;
		public event EventHandler<ReportRequestFailedEventArgs> ReportRequestFailed;

		/// <summary>
		/// Constructor to be used for UnitTesting/Mocking (in the absence of a dedicated DependencyInjection framework)
		/// </summary>
		internal EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey, string mWSAuthToken,
            IReportQueueingProcessor reportProcessor,
			IFeedQueueingProcessor feedProcessor, IEasyMwsLogger easyMwsLogger,
			EasyMwsOptions options)
			: this(region, merchantId, accessKeyId, mwsSecretAccessKey, mWSAuthToken, easyMwsLogger, options)
		{
			_reportProcessor = reportProcessor;
			_feedProcessor = feedProcessor;

            _reportProcessor.ReportDownloadedInternal -= OnReportDownloaded;
            _reportProcessor.ReportDownloadedInternal += OnReportDownloaded;
			_reportProcessor.ReportRequestFailedInternal -= OnReportRequestFailed;
			_reportProcessor.ReportRequestFailedInternal += OnReportRequestFailed;
			_feedProcessor.FeedUploadedInternal -= OnFeedUploaded;
            _feedProcessor.FeedUploadedInternal += OnFeedUploaded;
        }

        /// <param name="region">The region of the account. Required parameter. A finer grained region or country can be specified on a PropertiesContainer by specifying its marketplaceIdList constructor argument.</param>
        /// <param name="merchantId">Seller ID / Merchant ID. Required parameter.</param>
        /// <param name="accessKeyId">Amazon account access key. Required parameter. This key can either belong to a seller or to a developer account authorized by a seller. If the key belongs to a developer account authorized by the seller then also make sure to specify the MWSAuthToken argument.</param>
        /// <param name="mwsSecretAccessKey">Amazon account secret access key. Required parameter. This key can either belong to a seller or to a developer account authorized by a seller. If the key belongs to a developer account authorized by the seller then also make sure to specify the MWSAuthToken argument.</param>
        /// <param name="mwsAuthToken">MWS Authorization Token. Optional parameter. If the provided access keys belong to a developer account authorized by a seller, this argument is the MWS Authorization Token provided by the seller to the authorized developer.</param>
        /// <param name="easyMwsLogger">An optional IEasyMwsLogger instance that can provide access to logs. It is strongly recommended to use a logger implementation already existing in the EasyMws package.</param>
        /// <param name="options">Configuration options for EasyMwsClient</param>
        public EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey, string mwsAuthToken = null,
            IEasyMwsLogger easyMwsLogger = null, EasyMwsOptions options = null)
		{
			if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(accessKeyId) ||
			    string.IsNullOrEmpty(mwsSecretAccessKey))
				throw new ArgumentNullException(
					"One or more required parameters provided to initialize the EasyMwsClient were null or empty");

			_amazonRegion = region;
			_merchantId = merchantId;
			_options = options ?? new EasyMwsOptions();

			_easyMwsLogger = easyMwsLogger ?? new EasyMwsLogger(isEnabled: false);
			var mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(_amazonRegion));
			_reportProcessor = _reportProcessor ?? new ReportProcessor(_amazonRegion, _merchantId, mwsAuthToken, _options, mwsClient, _easyMwsLogger);
			_feedProcessor = _feedProcessor ?? new FeedProcessor(_amazonRegion, _merchantId, mwsAuthToken, _options, mwsClient, _easyMwsLogger);

            _reportProcessor.ReportDownloadedInternal -= OnReportDownloaded;
            _reportProcessor.ReportDownloadedInternal += OnReportDownloaded;
            _feedProcessor.FeedUploadedInternal -= OnFeedUploaded;
            _feedProcessor.FeedUploadedInternal += OnFeedUploaded;
        }

		public void Poll()
		{
            Parallel.Invoke(
				() =>
				{
					using (var reportRequestService = new ReportRequestEntryService(_options, _easyMwsLogger))
					{
                        _reportProcessor.PollReports(reportRequestService);
					}
				},
				() =>
				{
					using (var feedSubmissionService = new FeedSubmissionEntryService(_options, _easyMwsLogger))
					{
						_feedProcessor.PollFeeds(feedSubmissionService);
					}
				});
		}

        public void QueueReport(ReportRequestPropertiesContainer reportRequestContainer, string targetEventId = null, Dictionary<string, object> targetEventArgs = null)
        {
            using (var reportRequestService = new ReportRequestEntryService(_options, _easyMwsLogger))
            {
                _reportProcessor.QueueReport(reportRequestService, reportRequestContainer, targetEventId, targetEventArgs);
            }
        }

        public void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer, string targetEventId = null, Dictionary<string, object> targetEventArgs = null)
		{
			using (var feedSubmissionService = new FeedSubmissionEntryService(_options, _easyMwsLogger))
			{
				_feedProcessor.QueueFeed(feedSubmissionService, feedSubmissionContainer, targetEventId, targetEventArgs);
			}
		}

		public void PurgeReportRequestEntriesQueue()
		{
			using (var reportRequestService = new ReportRequestEntryService(_options, _easyMwsLogger))
			{
				_reportProcessor.PurgeQueue(reportRequestService);
			}
		}

		public void PurgeFeedSubmissionEntriesQueue()
		{
			using (var feedSubmissionService = new FeedSubmissionEntryService(_options, _easyMwsLogger))
			{
				_feedProcessor.PurgeQueue(feedSubmissionService);
			}
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

        private void OnReportDownloaded(object sender, ReportDownloadedEventArgs e) => ReportDownloaded?.Invoke(null, e);
        private void OnFeedUploaded(object sender, FeedUploadedEventArgs e) => FeedUploaded?.Invoke(null, e);
		private void OnReportRequestFailed(object sender, ReportRequestFailedEventArgs e) => ReportRequestFailed?.Invoke(null, e);

		#endregion

	}
}
