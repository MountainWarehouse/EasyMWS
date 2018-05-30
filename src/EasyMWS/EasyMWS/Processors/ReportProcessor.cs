using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MountainWarehouse.EasyMWS.CallbackLogic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class ReportProcessor : IReportQueueingProcessor
	{
		private readonly IRequestReportProcessor _requestReportProcessor;
		private readonly ICallbackActivator _callbackActivator;
		private readonly IAmazonReportService _amazonReportService;
		private readonly IEasyMwsLogger _logger;

		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient,
			IRequestReportProcessor requestReportProcessor, ICallbackActivator callbackActivator,
			IAmazonReportService amazonReportService, IEasyMwsLogger logger)
			: this(region, merchantId, options, mwsClient, logger)
		{
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
			_amazonReportService = _amazonReportService ?? new AmazonReportService(_options);
			_requestReportProcessor = _requestReportProcessor ?? new RequestReportProcessor(_region, _merchantId, mwsClient, _amazonReportService, _logger, _options);
		}


		public void PollReports(IReportRequestCallbackService reportRequestService)
		{
			_logger.Info("EasyMwsClient: Executing polling action for report requests.");
			try
			{
				_requestReportProcessor.CleanupReportRequests(reportRequestService);

				RequestNextReportInQueueFromAmazon(reportRequestService);

				RequestReportStatusesFromAmazon(reportRequestService);

				var reportInfo = DownloadNextReportInQueueFromAmazon(reportRequestService);

				if (reportInfo.stream != null)
				{
					PerformCallback(reportRequestService, reportInfo.reportRequestCallback, reportInfo.stream);
				}
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		private void PerformCallback(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequest, Stream stream)
		{
			try
			{
				ExecuteMethodCallback(reportRequest, stream);
				_requestReportProcessor.RemoveFromQueue(reportRequestService, reportRequest.Id);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void QueueReport(IReportRequestCallbackService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			try
			{
				if (propertiesContainer == null) throw new ArgumentNullException();

				var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

				var reportRequest = new ReportRequestEntry(serializedPropertiesContainer)
				{
					AmazonRegion = _region,
					MerchantId = _merchantId,
					LastRequested = DateTime.MinValue,
					ContentUpdateFrequency = propertiesContainer.UpdateFrequency,
					RequestReportId = null,
					GeneratedReportId = null,
					RequestRetryCount = 0,
					ReportType = propertiesContainer.ReportType
				};

				if (callbackMethod != null)
				{
					var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);
					reportRequest.Data = serializedCallback.Data;
					reportRequest.TypeName = serializedCallback.TypeName;
					reportRequest.MethodName = serializedCallback.MethodName;
					reportRequest.DataTypeName = serializedCallback.DataTypeName;
				}

				reportRequestService.Create(reportRequest);
				reportRequestService.SaveChanges();

				_logger.Info($"EasyMwsClient: The following report was queued for download from Amazon {reportRequest.RegionAndTypeComputed}.");
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void QueueReport(IReportRequestCallbackService reportRequestService, ReportRequestPropertiesContainer propertiesContainer)
		{
			QueueReport(reportRequestService, propertiesContainer, null, null);
		}

		public void RequestNextReportInQueueFromAmazon(IReportRequestCallbackService reportRequestService)
		{
			var reportRequest = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(reportRequestService);

			if (reportRequest == null) return;

			try
			{
				var reportRequestId = _requestReportProcessor.RequestReportFromAmazon(reportRequest);

				reportRequest.LastRequested = DateTime.UtcNow;
				reportRequestService.Update(reportRequest);
				reportRequestService.SaveChanges();

				if (string.IsNullOrEmpty(reportRequestId))
				{
					_requestReportProcessor.MoveToRetryQueue(reportRequestService, reportRequest);
					_logger.Warn($"AmazonMWS request failed for {reportRequest.RegionAndTypeComputed}");
				}
				else
				{
					_requestReportProcessor.MoveToQueueOfReportsToGenerate(reportRequestService, reportRequest, reportRequestId);
					_logger.Info(
						$"AmazonMWS request succeeded for {reportRequest.RegionAndTypeComputed}. ReportRequestId:'{reportRequestId}'");
				}
			}
			catch (Exception e)
			{
				_requestReportProcessor.MoveToRetryQueue(reportRequestService, reportRequest);
				_logger.Error(e.Message, e);
			}
		}

		public void RequestReportStatusesFromAmazon(IReportRequestCallbackService reportRequestService)
		{
			try
			{
				var pendingReportsRequestIds = _requestReportProcessor.GetAllPendingReportFromQueue(reportRequestService).ToList();

				if (!pendingReportsRequestIds.Any()) return;

				var reportRequestStatuses =
					_requestReportProcessor.GetReportProcessingStatusesFromAmazon(pendingReportsRequestIds, _merchantId);

				_requestReportProcessor.QueueReportsAccordingToProcessingStatus(reportRequestService, reportRequestStatuses);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public (ReportRequestEntry reportRequestCallback, Stream stream) DownloadNextReportInQueueFromAmazon(IReportRequestCallbackService reportRequestService)
		{
			var reportToDownload = _requestReportProcessor.GetNextFromQueueOfReportsToDownload(reportRequestService);
			if (reportToDownload == null) return (null, null);

			try
			{
				var stream = _requestReportProcessor.DownloadGeneratedReportFromAmazon(reportToDownload);
				return (reportToDownload, stream);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
				return (null, null);
			}
		}

		public void ExecuteMethodCallback(ReportRequestEntry reportRequest, Stream stream)
		{
			_logger.Info(
				$"Attempting to perform method callback for the next downloaded report in queue : {reportRequest.RegionAndTypeComputed}.");

			var callback = new Callback(reportRequest.TypeName, reportRequest.MethodName,
				reportRequest.Data, reportRequest.DataTypeName);

			_callbackActivator.CallMethod(callback, stream);
		}
	}
}
