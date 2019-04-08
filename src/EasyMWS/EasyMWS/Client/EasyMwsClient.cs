using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
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

		internal EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey,
			IReportQueueingProcessor reportProcessor,
			IFeedQueueingProcessor feedProcessor, IEasyMwsLogger easyMwsLogger,
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
		public EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey,
			IEasyMwsLogger easyMwsLogger = null, EasyMwsOptions options = null)
		{
			if (string.IsNullOrEmpty(merchantId) || string.IsNullOrEmpty(accessKeyId) ||
			    string.IsNullOrEmpty(mwsSecretAccessKey))
				throw new ArgumentNullException(
					"One or more required parameters provided to initialize the EasyMwsClient were null or empty");

			_amazonRegion = region;
			_merchantId = merchantId;
			_options = options ?? EasyMwsOptions.Defaults();

			_easyMwsLogger = easyMwsLogger ?? new EasyMwsLogger(isEnabled: false);
			var mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(_amazonRegion));
			_reportProcessor = _reportProcessor ?? new ReportProcessor(_amazonRegion, _merchantId, _options, mwsClient, _easyMwsLogger);
			_feedProcessor = _feedProcessor ?? new FeedProcessor(_amazonRegion, _merchantId, _options, mwsClient, _easyMwsLogger);

		}

		public AmazonRegion AmazonRegion => _amazonRegion;

		public string MerchantId => _merchantId;

		public EasyMwsOptions Options => _options;

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


		public void QueueReport(ReportRequestPropertiesContainer reportRequestContainer,
			Action<Stream, object> callbackMethod, object callbackData)
		{
			using (var reportRequestService = new ReportRequestEntryService(_options, _easyMwsLogger))
			{
				_reportProcessor.QueueReport(reportRequestService, reportRequestContainer, callbackMethod, callbackData);
			}
		}

		public void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer,
			Action<Stream, object> callbackMethod, object callbackData)
		{
			using (var feedSubmissionService = new FeedSubmissionEntryService(_options, _easyMwsLogger))
			{
				_feedProcessor.QueueFeed(feedSubmissionService, feedSubmissionContainer, callbackMethod, callbackData);
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

		#endregion

	}
}
