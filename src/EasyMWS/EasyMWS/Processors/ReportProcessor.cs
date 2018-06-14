using System;
using System.Data.SqlClient;
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
			_logger.Info("Executing polling action for report requests.");

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
				try
				{
					ExecuteMethodCallback(reportEntry);
					reportRequestService.Delete(reportEntry);
				}
				catch (SqlException e)
				{
					_logger.Error(e.Message, e);
				}
				catch (Exception e)
				{
					reportEntry.ReportRequestRetryCount++;
					reportRequestService.Update(reportEntry);
					_logger.Error(
						$"Method callback failed for {reportEntry.RegionAndTypeComputed}. Placing report request entry in retry queue. Current retry count is :{reportEntry.ReportRequestRetryCount}. {e.Message}", e);
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
					ReportRequestRetryCount = 0,
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

			_requestReportProcessor.RequestReportFromAmazon(reportRequestService, reportRequest);
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
			
			var reportInfo = _requestReportProcessor.DownloadGeneratedReportFromAmazon(reportToDownload);
			if (reportInfo.report == null)
			{
				_logger.Warn($"AmazonMWS report download failed for {reportToDownload.RegionAndTypeComputed}");
				return;
			}

			var hasValidHash = MD5ChecksumHelper.IsChecksumCorrect(reportInfo.report, reportInfo.md5Hash);
			if (hasValidHash)
			{
				_logger.Info($"Checksum verification succeeded for report {reportToDownload.RegionAndTypeComputed}");

				using (var streamReader = new StreamReader(reportInfo.report))
				{
					var zippedReport = ZipHelper.CreateArchiveFromContent(streamReader.ReadToEnd());
					reportToDownload.Details = new ReportRequestDetails { ReportContent = zippedReport };
				}

				reportRequestService.Update(reportToDownload);
				reportRequestService.SaveChanges();
			}
			else
			{
				_logger.Warn($"Checksum verification failed for report {reportToDownload.RegionAndTypeComputed}");
				_requestReportProcessor.MoveToRetryQueue(reportRequestService, reportToDownload);
			}
		}

		public void ExecuteMethodCallback(ReportRequestEntry reportRequest)
		{
			_logger.Info(
				$"Attempting to perform method callback for the next downloaded report in queue : {reportRequest.RegionAndTypeComputed}.");

			var callback = new Callback(reportRequest.TypeName, reportRequest.MethodName,
				reportRequest.Data, reportRequest.DataTypeName);

			var unzippedReport = ZipHelper.ExtractArchivedSingleFileToStream(reportRequest.Details?.ReportContent);
			_callbackActivator.CallMethod(callback, unzippedReport);
		}
	}
}
