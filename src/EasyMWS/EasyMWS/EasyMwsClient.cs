using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarketplaceWebService;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
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
		private AmazonRegion _amazonRegion;

		public AmazonRegion AmazonRegion => _amazonRegion;

		internal EasyMwsClient(AmazonRegion region, string accessKeyId, string mwsSecretAccessKey, IReportRequestCallbackService reportRequestCallbackService) 
			: this(region, accessKeyId, mwsSecretAccessKey)
		{
			_reportRequestCallbackService = reportRequestCallbackService;
		}

		/// <param name="region">The region of the account</param>
		/// <param name="accessKeyId">Your specific access key</param>
		/// <param name="mwsSecretAccessKey">Your specific secret access key</param>
		public EasyMwsClient(AmazonRegion region, string accessKeyId, string mwsSecretAccessKey)
		{
			_amazonRegion = region;
			_mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(region));
			_reportRequestCallbackService = _reportRequestCallbackService ?? new ReportRequestCallbackService();
			_callbackActivator = new CallbackActivator();
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

		/// <summary>
		/// Dequeues the next ReportRequestCallback from the specified region queue of non-requested reports by setting it's RequestReportId value. <para/>
		/// </summary>
		/// <param name="region">The region of the Queue to dequeue from.</param>
		/// <param name="requestReportId">Value to set on the report to dequeue.</param>
		private void DequeueNonRequestedReport(AmazonRegion region, string requestReportId)
		{
			var frontOfQueue = _reportRequestCallbackService.GetAll()
				.Where(rrc => rrc.AmazonRegion == region && rrc.RequestReportId == null)
				.OrderBy(rrc => rrc.Id)
				.FirstOrDefault();

			frontOfQueue.RequestReportId = requestReportId;

			_reportRequestCallbackService.Update(frontOfQueue);
			_reportRequestCallbackService.SaveChanges();
		}

		/// <summary>
		/// Dequeues the next ReportRequestCallback from the specified region queue of non-generated reports by setting it's GeneratedReportId value. <para/>
		/// </summary>
		/// <param name="region">The region of the Queue to dequeue from.</param>
		/// <param name="generatedReportId">Value to set on the report to dequeue.</param>
		private void DequeueNonGeneratedReport(AmazonRegion region, string generatedReportId)
		{
			var frontOfQueue = _reportRequestCallbackService.GetAll()
				.Where(rrc => rrc.AmazonRegion == region && rrc.GeneratedReportId == null && rrc.RequestReportId != null)
				.OrderBy(rrc => rrc.Id)
				.FirstOrDefault();

			frontOfQueue.GeneratedReportId = generatedReportId;

			_reportRequestCallbackService.Update(frontOfQueue);
			_reportRequestCallbackService.SaveChanges();
		}
	}
}
