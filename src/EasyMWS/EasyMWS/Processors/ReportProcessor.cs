using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using MountainWarehouse.EasyMWS.CallbackLogic;
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

		public void PollReports(IReportRequestEntryService reportRequestService)
		{
			_logger.Info("Executing polling action for report requests.");

			_requestReportProcessor.CleanupReportRequests(reportRequestService);

			RequestNextReportInQueueFromAmazon(reportRequestService);

			RequestReportStatusesFromAmazon(reportRequestService);

			DownloadNextReportInQueueFromAmazon(reportRequestService);

			PerformCallbackForPreviouslyDownloadedReports(reportRequestService);
		}

		public void DownloadNextReportInQueueFromAmazon(IReportRequestEntryService reportRequestService)
		{
			var reportToDownload = reportRequestService.GetNextFromQueueOfReportsToDownload(_options, _merchantId, _region);
			if (reportToDownload == null) return;

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(reportRequestService, reportToDownload);
		}

		private void PerformCallbackForPreviouslyDownloadedReports(IReportRequestEntryService reportRequestService)
		{
			var reportsReadyForCallback = reportRequestService.GetAllFromQueueOfReportsReadyForCallback(_options, _merchantId, _region);

			foreach (var reportEntry in reportsReadyForCallback)
			{
				try
				{
					_logger.Info($"Attempting to perform method callback for the next downloaded report in queue : {reportEntry.RegionAndTypeComputed}.");
					var callback = new Callback(reportEntry.TypeName, reportEntry.MethodName, reportEntry.Data, reportEntry.DataTypeName);
					var unzippedReport = ZipHelper.ExtractArchivedSingleFileToStream(reportEntry.Details?.ReportContent);
					_callbackActivator.CallMethod(callback, unzippedReport);
					reportRequestService.Delete(reportEntry);
				}
				catch(SqlException e)
				{
					_logger.Error($"Method callback failed for {reportEntry.RegionAndTypeComputed} due to an internal error '{e.Message}'. The callback will be retried at the next poll request.", e);
				}
				catch (Exception e)
				{
					_logger.Error($"Method callback failed for {reportEntry.RegionAndTypeComputed}. Current retry count is :{reportEntry.InvokeCallbackRetryCount}. {e.Message}", e);
					reportEntry.InvokeCallbackRetryCount++;
					reportRequestService.Update(reportEntry);
				}
			}

			reportRequestService.SaveChanges();
		}

		public void QueueReport(IReportRequestEntryService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
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
					LastAmazonRequestDate = DateTime.MinValue,
					DateCreated = DateTime.UtcNow,
					ContentUpdateFrequency = propertiesContainer.UpdateFrequency,
					RequestReportId = null,
					GeneratedReportId = null,
					ReportRequestRetryCount = 0,
					ReportDownloadRetryCount = 0,
					ReportProcessRetryCount = 0,
					InvokeCallbackRetryCount = 0,
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

		public void PurgeQueue(IReportRequestEntryService reportRequestService)
		{
			var entriesToDelete = reportRequestService.GetAll().Where(rre => rre.AmazonRegion == _region && rre.MerchantId == _merchantId);
			reportRequestService.DeleteRange(entriesToDelete);
			reportRequestService.SaveChanges();
		}

		public void RequestNextReportInQueueFromAmazon(IReportRequestEntryService reportRequestService)
		{
			var reportRequest = reportRequestService.GetNextFromQueueOfReportsToRequest(_options, _merchantId, _region);

			if (reportRequest == null) return;

			_requestReportProcessor.RequestReportFromAmazon(reportRequestService, reportRequest);
		}

		public void RequestReportStatusesFromAmazon(IReportRequestEntryService reportRequestService)
		{
			var pendingReportsRequestIds = reportRequestService.GetAllPendingReportFromQueue(_merchantId, _region).ToList();

			if (!pendingReportsRequestIds.Any()) return;

			var reportRequestStatuses =
				_requestReportProcessor.GetReportProcessingStatusesFromAmazon(pendingReportsRequestIds, _merchantId);

			if (reportRequestStatuses != null)
			{
				_requestReportProcessor.QueueReportsAccordingToProcessingStatus(reportRequestService, reportRequestStatuses);
			}
		}
	}
}
