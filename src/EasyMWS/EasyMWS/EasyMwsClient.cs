using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
		private EasyMwsOptions _options;

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
		/// <param name="options">Configuration options for EasyMwsClient</param>
		public EasyMwsClient(AmazonRegion region, string merchantId, string accessKeyId, string mwsSecretAccessKey, EasyMwsOptions options = null)
		{
			_options = options ?? EasyMwsOptions.Defaults;
			_merchantId = merchantId;
			_amazonRegion = region;
			_mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(region));
			_reportRequestCallbackService = _reportRequestCallbackService ?? new ReportRequestCallbackService();
			_callbackActivator = new CallbackActivator();
			_requestReportProcessor = new RequestReportProcessor(_mwsClient, _reportRequestCallbackService, _options);
		}

		public void Poll()
		{
			RequestNextReportInQueueFromAmazon();

			RequestReportStatusesFromAmazon();

			var generatedReportRequestCallback = DownloadNextGeneratedRequestReportInQueueFromAmazon();

			PerformCallback(generatedReportRequestCallback.reportRequestCallback, generatedReportRequestCallback.stream);
		}

		private void RequestNextReportInQueueFromAmazon()
		{
			var reportRequestCallbackReportQueued = _requestReportProcessor.GetNonRequestedReportFromQueue(_amazonRegion);

			if (reportRequestCallbackReportQueued == null)
				return;

			var reportRequestId = _requestReportProcessor.RequestSingleQueuedReport(reportRequestCallbackReportQueued, _merchantId);

			if (!string.IsNullOrEmpty(reportRequestId))
			{
				_requestReportProcessor.MoveToNonGeneratedReportsQueue(reportRequestCallbackReportQueued, reportRequestId);
			}
			// todo: what if we don't get ID back from Amazon?
		}

		private (ReportRequestCallback reportRequestCallback, Stream stream) DownloadNextGeneratedRequestReportInQueueFromAmazon()
		{
			var generatedReportRequest = _requestReportProcessor.GetReadyForDownloadReports(_amazonRegion);

			if (generatedReportRequest == null)
				return (null, null);
			
			var stream = _requestReportProcessor.DownloadGeneratedReport(generatedReportRequest, _merchantId);
			
			return (generatedReportRequest, stream);
		}

		private void PerformCallback(ReportRequestCallback reportRequestCallback, Stream stream)
		{
			if (reportRequestCallback == null || stream == null) return;

			var callback = new Callback(reportRequestCallback.TypeName, reportRequestCallback.MethodName,
				reportRequestCallback.Data, reportRequestCallback.DataTypeName);

			_callbackActivator.CallMethod(callback, stream);

			DequeueReport(reportRequestCallback);
		}

		private void RequestReportStatusesFromAmazon()
		{
			var reportRequestCallbacksPendingReports = _requestReportProcessor.GetAllPendingReport(_amazonRegion).ToList();

			if (!reportRequestCallbacksPendingReports.Any())
				return;

			var reportRequestIds = reportRequestCallbacksPendingReports.Select(x => x.RequestReportId);

			var reportRequestStatuses = _requestReportProcessor.GetReportRequestListResponse(reportRequestIds, _merchantId);

			_requestReportProcessor.MoveReportsToGeneratedQueue(reportRequestStatuses);
			_requestReportProcessor.MoveReportsBackToRequestQueue(reportRequestStatuses);

		}

		private void DequeueReport(ReportRequestCallback reportRequestCallback)
		{
			_requestReportProcessor.DequeueReportRequestCallback(reportRequestCallback);
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
