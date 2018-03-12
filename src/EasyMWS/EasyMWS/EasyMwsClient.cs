using System;
using System.IO;
using System.Threading.Tasks;
using MarketplaceWebService;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.ReportProcessors;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS
{
	/// <summary>Client for Amazon Marketplace Web Services</summary>
	public class EasyMwsClient
	{
		private IMarketplaceWebServiceClient _mwsClient;
		private IReportRequestCallbackService _reportRequestCallbackService;
		private CallbackActivator _callbackActivator;
		private string _merchantId;
		private AmazonRegion _amazonRegion;
		private IRequestReportProcessor _requestReportProcessor;

		public AmazonRegion AmazonRegion => _amazonRegion;

		internal EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey, IReportRequestCallbackService reportRequestCallbackService, IMarketplaceWebServiceClient marketplaceWebServiceClient, IRequestReportProcessor requestReportProcessor) 
			: this(region, merchantId, accessKeyId, mwsSecretAccessKey)
		{
			_reportRequestCallbackService = reportRequestCallbackService;
			_requestReportProcessor = requestReportProcessor;
			_mwsClient = marketplaceWebServiceClient;
		}

		/// <param name="region">The region of the account</param>
		/// <param name="merchantId"></param>
		/// <param name="accessKeyId">Your specific access key</param>
		/// <param name="mwsSecretAccessKey">Your specific secret access key</param>
		public EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey)
		{
			_merchantId = merchantId;
			_amazonRegion = region;
			_mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(region));
			_reportRequestCallbackService = _reportRequestCallbackService ?? new ReportRequestCallbackService();
			_callbackActivator = new CallbackActivator();
			_requestReportProcessor = new RequestReportProcessor(_mwsClient, _reportRequestCallbackService);
		}

		public void Poll()
		{
			var reportRequestCallbackReportQueued = _requestReportProcessor.GetNonRequestedReportFromQueue(_amazonRegion);

			var reportRequestId = _requestReportProcessor.RequestSingleQueuedReport(reportRequestCallbackReportQueued, _merchantId);
			if (!string.IsNullOrEmpty(reportRequestId))
			{
				_requestReportProcessor.MoveToNonGeneratedReportsQueue(reportRequestCallbackReportQueued, reportRequestId);
			}
			// todo: what if we don't get ID back from Amazon?
			
			var reportRequestCallbacksPendingReports = _requestReportProcessor.GetAllPendingReport(_amazonRegion);

			foreach (var reportRequestCallbacksPendingReport in reportRequestCallbacksPendingReports)
			{
				var reportRequestPropertiesContainer = JsonConvert.DeserializeObject<ReportRequestPropertiesContainer>(reportRequestCallbacksPendingReport.ReportRequestData);

				/* Get statuses:
					_DONE_: Call update, update GeneratedReportId
					_CANCELLED_: Call update, update ReportRequestId to NULL
					_OTHER_: Log/Exception/nothing?
				*/
			}

			/* Loop on ReportRequestId NOT NULL AND GeneratedRequestId NOT NULL:
			 * Download
			 */

			/*
			 * Callback
			 */
		}

		#region Helpers for creating the MarketplaceWebServiceClient

		private MarketplaceWebServiceConfig CreateConfig(AmazonRegion region)
		{
			string rootUrl;
			switch (region)
			{
				case AmazonRegion.Australia:
					rootUrl = "https://mws.amazonservices.com.au";
					break;
				case AmazonRegion.China:
					rootUrl = "https://mws.amazonservices.com.cn";
					break;
				case AmazonRegion.Europe:
					rootUrl = "https://mws-eu.amazonservices.com";
					break;
				case AmazonRegion.India:
					rootUrl = "https://mws.amazonservices.in";
					break;
				case AmazonRegion.Japan:
					rootUrl = "https://mws.amazonservices.jp";
					break;
				case AmazonRegion.NorthAmerica:
					rootUrl = "https://mws.amazonservices.com";
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

		public void QueueReport(ReportRequestPropertiesContainer reportRequestContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			_reportRequestCallbackService.Create(GetSerializedReportRequestCallback(reportRequestContainer, callbackMethod, callbackData));
			_reportRequestCallbackService.SaveChanges();
		}

		public async Task QueueReportAsync(ReportRequestPropertiesContainer reportRequestContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			await _reportRequestCallbackService.CreateAsync(GetSerializedReportRequestCallback(reportRequestContainer, callbackMethod, callbackData));
			await _reportRequestCallbackService.SaveChangesAsync();
		}

		private ReportRequestCallback GetSerializedReportRequestCallback(
			ReportRequestPropertiesContainer reportRequestContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			if(reportRequestContainer == null || callbackMethod == null) throw new ArgumentNullException();
			var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);

			return new ReportRequestCallback(serializedCallback)
			{
				AmazonRegion = _amazonRegion,
				LastRequested = DateTime.MinValue,
				ContentUpdateFrequency = reportRequestContainer.UpdateFrequency,
				ReportRequestData = JsonConvert.SerializeObject(reportRequestContainer)
			};
		}
	}
}
