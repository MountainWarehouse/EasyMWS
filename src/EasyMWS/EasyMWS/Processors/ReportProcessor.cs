using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using MountainWarehouse.EasyMWS.CallbackLogic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
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
		private readonly IEasyMwsLogger _logger;

		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient,
			IRequestReportProcessor requestReportProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
			: this(region, merchantId, options, mwsClient, logger)
		{
			_requestReportProcessor = requestReportProcessor;
			_callbackActivator = callbackActivator;
		}

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();
			_requestReportProcessor = _requestReportProcessor ?? new RequestReportProcessor(_region, _merchantId, mwsClient, _logger, _options);
		}


		public void PollReports(IReportRequestCallbackService reportRequestService)
		{
			_logger.Info("EasyMwsClient: Executing polling action for report requests.");

			_requestReportProcessor.CleanupReportRequests(reportRequestService);

			RequestNextReportInQueueFromAmazon(reportRequestService);

			RequestReportStatusesFromAmazon(reportRequestService);

			DownloadNextReportInQueueFromAmazon(reportRequestService);

			PerformCallbackForPreviouslyDownloadedReports(reportRequestService);
		}

		private void PerformCallbackForPreviouslyDownloadedReports(IReportRequestCallbackService reportRequestService)
		{
			var previouslyDownloadedReports = reportRequestService.GetAll()
				.Where(rre => rre.AmazonRegion == _region && rre.MerchantId == _merchantId && rre.Details != null);

			foreach (var reportEntry in previouslyDownloadedReports)
			{
				var callbackSucceeded = true;
				try
				{
					ExecuteMethodCallback(reportEntry);
				}
				catch (Exception e)
				{
					callbackSucceeded = false;
					reportEntry.RequestRetryCount++;
					reportRequestService.Update(reportEntry);
					_logger.Error($"Method callback failed for {reportEntry.RegionAndTypeComputed}. Placing report request entry to retry queue. Current retry count is :{reportEntry.RequestRetryCount}. {e.Message}", e);
				}
				finally
				{
					if (callbackSucceeded)
					{
						reportRequestService.Delete(reportEntry);
					}
				}
			}

			reportRequestService.SaveChanges();
		}

		public void QueueReport(IReportRequestCallbackService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			try
			{
				if (callbackMethod == null)
				{
					throw new ArgumentNullException(nameof(callbackMethod),"The callback method cannot be null, as it has to be invoked once the report has been downloaded, in order to provide access to the report content.");
				}

				if (propertiesContainer == null) throw new ArgumentNullException();

				var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

				var reportRequest = new ReportRequestEntry(serializedPropertiesContainer)
				{
					AmazonRegion = _region,
					MerchantId = _merchantId,
					LastRequested = DateTime.MinValue,
					DateCreated = DateTime.UtcNow,
					ContentUpdateFrequency = propertiesContainer.UpdateFrequency,
					RequestReportId = null,
					GeneratedReportId = null,
					RequestRetryCount = 0,
					ReportType = propertiesContainer.ReportType
				};

				var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);
				reportRequest.Data = serializedCallback.Data;
				reportRequest.TypeName = serializedCallback.TypeName;
				reportRequest.MethodName = serializedCallback.MethodName;
				reportRequest.DataTypeName = serializedCallback.DataTypeName;

				reportRequestService.Create(reportRequest);
				reportRequestService.SaveChanges();

				_logger.Info($"The following report was queued for download from Amazon {reportRequest.RegionAndTypeComputed}.");
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void PurgeQueue(IReportRequestCallbackService reportRequestService)
		{
			var entriesToDelete = reportRequestService.GetAll().Where(rre => rre.AmazonRegion == _region && rre.MerchantId == _merchantId);
			reportRequestService.DeleteRange(entriesToDelete);
			reportRequestService.SaveChanges();
		}

		public void RequestNextReportInQueueFromAmazon(IReportRequestCallbackService reportRequestService)
		{
			var reportRequest = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(reportRequestService);

			if (reportRequest == null) return;

			var reportRequestId = _requestReportProcessor.RequestReportFromAmazon(reportRequest);

			reportRequest.LastRequested = DateTime.UtcNow;
			reportRequestService.Update(reportRequest);
			reportRequestService.SaveChanges();

			if (string.IsNullOrEmpty(reportRequestId))
			{
				_requestReportProcessor.MoveToRetryQueue(reportRequestService, reportRequest);
				_logger.Warn($"AmazonMWS request failed for {reportRequest.RegionAndTypeComputed}. Reason: ReportRequestId not generated by Amazon. Placing report request in retry queue. Retry count : {reportRequest.RequestRetryCount}");
			}
			else if (reportRequestId == HttpStatusCode.BadRequest.ToString())
			{
				_requestReportProcessor.RemoveFromQueue(reportRequestService, reportRequest);
				_logger.Warn($"AmazonMWS request failed for {reportRequest.RegionAndTypeComputed}. The report request was removed from queue.");
			}
			else
			{
				_requestReportProcessor.MoveToQueueOfReportsToGenerate(reportRequestService, reportRequest, reportRequestId);
				_logger.Info(
					$"AmazonMWS request succeeded for {reportRequest.RegionAndTypeComputed}. ReportRequestId:'{reportRequestId}'");
			}
		}

		public void RequestReportStatusesFromAmazon(IReportRequestCallbackService reportRequestService)
		{
			var pendingReportsRequestIds = _requestReportProcessor.GetAllPendingReportFromQueue(reportRequestService).ToList();

			if (!pendingReportsRequestIds.Any()) return;

			var reportRequestStatuses =
				_requestReportProcessor.GetReportProcessingStatusesFromAmazon(pendingReportsRequestIds, _merchantId);

			if (reportRequestStatuses != null)
			{
				_requestReportProcessor.QueueReportsAccordingToProcessingStatus(reportRequestService, reportRequestStatuses);
			}
		}

		public void DownloadNextReportInQueueFromAmazon(IReportRequestCallbackService reportRequestService)
		{
			var reportToDownload = _requestReportProcessor.GetNextFromQueueOfReportsToDownload(reportRequestService);
			if (reportToDownload == null) return;
			
			var stream = _requestReportProcessor.DownloadGeneratedReportFromAmazon(reportToDownload);
			if(stream == null) return;

			reportToDownload.Details = new ReportRequestDetails { ReportContent = StreamHelper.GetBytesFromStream(stream) };
			reportRequestService.Update(reportToDownload);
			reportRequestService.SaveChanges();
		}

		public void ExecuteMethodCallback(ReportRequestEntry reportRequest, Stream stream)
		{
			_logger.Info(
				$"Attempting to perform method callback for the next downloaded report in queue : {reportRequest.RegionAndTypeComputed}.");

			var callback = new Callback(reportRequest.TypeName, reportRequest.MethodName,
				reportRequest.Data, reportRequest.DataTypeName);

			_callbackActivator.CallMethod(callback, stream);
		}

		public void ExecuteMethodCallback(ReportRequestEntry reportRequest)
		{
			_logger.Info(
				$"Attempting to perform method callback for the next downloaded report in queue : {reportRequest.RegionAndTypeComputed}.");

			var callback = new Callback(reportRequest.TypeName, reportRequest.MethodName,
				reportRequest.Data, reportRequest.DataTypeName);

			_callbackActivator.CallMethod(callback, StreamHelper.GetStreamFromBytes(reportRequest.Details?.ReportContent));
		}
	}
}
