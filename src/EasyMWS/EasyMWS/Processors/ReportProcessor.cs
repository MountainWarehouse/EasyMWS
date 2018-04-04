using System;
using System.IO;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class ReportProcessor : IQueueingProcessor<ReportRequestPropertiesContainer>
	{
		private readonly IReportRequestCallbackService _reportService;
		private readonly IRequestReportProcessor _requestReportProcessor;
		private readonly ICallbackActivator _callbackActivator;
		private readonly IAmazonReportService _amazonReportService;
		private readonly IEasyMwsLogger _logger;

		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IReportRequestCallbackService reportService, IMarketplaceWebServiceClient mwsClient,
			IRequestReportProcessor requestReportProcessor, ICallbackActivator callbackActivator,
			IAmazonReportService amazonReportService, IEasyMwsLogger logger)
			: this(region, merchantId, options, mwsClient, logger)
		{
			_reportService = reportService;
			_requestReportProcessor = requestReportProcessor;
			_callbackActivator = callbackActivator;
			_amazonReportService = amazonReportService;
		}

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();
			_amazonReportService = _amazonReportService ?? new AmazonReportService();
			_reportService = _reportService ?? new ReportRequestCallbackService();
			_requestReportProcessor = _requestReportProcessor ?? new RequestReportProcessor(mwsClient, _reportService, _amazonReportService, logger, options);
		}


		public void Poll()
		{
			_logger.Info("EasyMwsClient: Executing polling action for report requests.");
			try
			{
				_requestReportProcessor.CleanupReportRequests();

				RequestNextReportInQueueFromAmazon();
				_reportService.SaveChanges();

				RequestReportStatusesFromAmazon();
				_reportService.SaveChanges();

				var generatedReportRequestCallback = DownloadNextReportInQueueFromAmazon();
				_reportService.SaveChanges();

				ExecuteCallback(generatedReportRequestCallback.reportRequestCallback, generatedReportRequestCallback.stream);
				_reportService.SaveChanges();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void Queue(ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			try
			{
				_reportService.Create(GetSerializedReportRequestCallback(propertiesContainer, callbackMethod, callbackData));
				_reportService.SaveChanges();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void RequestNextReportInQueueFromAmazon()
		{
			var reportRequest = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(_region, _merchantId);

			if (reportRequest == null) return;
			var reportFriendlyId = reportRequest.GetRegionAndTypeString();
			_logger.Info($"Attempting to request the next report in queue from Amazon: {reportFriendlyId}.");

			try
			{
				var reportRequestId = _requestReportProcessor.RequestReportFromAmazon(reportRequest);

				reportRequest.LastRequested = DateTime.UtcNow;
				_reportService.Update(reportRequest);

				if (string.IsNullOrEmpty(reportRequestId))
				{
					_requestReportProcessor.MoveToRetryQueue(reportRequest);
					_logger.Warn($"AmazonMWS request failed for {reportFriendlyId}");
				}
				else
				{
					_requestReportProcessor.GetNextFromQueueOfReportsToGenerate(reportRequest, reportRequestId);
					_logger.Info($"AmazonMWS request succeeded for {reportFriendlyId}. ReportRequestId:'{reportRequestId}'");
				}
			}
			catch (Exception e)
			{
				_requestReportProcessor.MoveToRetryQueue(reportRequest);
				_logger.Error(e.Message, e);
			}
		}

		public void RequestReportStatusesFromAmazon()
		{
			_logger.Info($"Attempting to request report processing status for all reports in queue.");
			try
			{
				var reportRequestCallbacksPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue(_region, _merchantId).ToList();

				if (!reportRequestCallbacksPendingReports.Any()) return;

				var reportRequestIds = reportRequestCallbacksPendingReports.Select(x => x.RequestReportId);

				var reportRequestStatuses = _requestReportProcessor.GetReportProcessingStatusesFromAmazon(reportRequestIds, _merchantId);
				_logger.Info($"AmazonMWS request for report processing statuses succeeded.");

				_requestReportProcessor.QueueReportsAccordingToProcessingStatus(reportRequestStatuses);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public (ReportRequestCallback reportRequestCallback, Stream stream) DownloadNextReportInQueueFromAmazon()
		{
			var generatedReportRequest = _requestReportProcessor.GetNextFromQueueOfReportsToDownload(_region, _merchantId);
			if (generatedReportRequest == null) return (null, null);
			var reportFriendlyId = generatedReportRequest.GetRegionAndTypeString();
			_logger.Info($"Attempting to download the next report in queue from Amazon: {reportFriendlyId}.");

			try
			{
				var stream = _requestReportProcessor.DownloadGeneratedReportFromAmazon(generatedReportRequest);
				_logger.Info($"Report download from Amazon has succeeded for {reportFriendlyId}.");

				return (generatedReportRequest, stream);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
				return (null, null);
			}
		}

		public void ExecuteCallback(ReportRequestCallback reportRequestCallback, Stream stream)
		{
			var reportFriendlyId = reportRequestCallback.GetRegionAndTypeString();
			_logger.Info($"Attempting to perform method callback for the next downloaded report in queue : {reportFriendlyId}.");
			try
			{
				if (reportRequestCallback == null || stream == null) return;

				var callback = new Callback(reportRequestCallback.TypeName, reportRequestCallback.MethodName,
					reportRequestCallback.Data, reportRequestCallback.DataTypeName);

				_callbackActivator.CallMethod(callback, stream);

				_requestReportProcessor.RemoveFromQueue(reportRequestCallback);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		private ReportRequestCallback GetSerializedReportRequestCallback(
			ReportRequestPropertiesContainer reportRequestContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			if (reportRequestContainer == null || callbackMethod == null) throw new ArgumentNullException();
			var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(reportRequestContainer);

			return new ReportRequestCallback(serializedCallback, serializedPropertiesContainer)
			{
				AmazonRegion = _region,
				MerchantId = _merchantId,
				LastRequested = DateTime.MinValue,
				ContentUpdateFrequency = reportRequestContainer.UpdateFrequency,
				RequestReportId = null,
				GeneratedReportId = null,
				RequestRetryCount = 0
			};
		}
	}
}
