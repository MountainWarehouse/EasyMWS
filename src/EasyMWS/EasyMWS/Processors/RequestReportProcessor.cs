using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
    internal class RequestReportProcessor : IRequestReportProcessor
    {
	    private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
	    private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
	    private readonly AmazonRegion _region;
	    private readonly string _merchantId;

	    internal RequestReportProcessor(AmazonRegion region, string merchantId, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
	    {
		    _region = region;
		    _merchantId = merchantId;
		    _options = options;
		    _logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
	    }

	    public ReportRequestEntry GetNextFromQueueOfReportsToRequest(IReportRequestCallbackService reportRequestService)
	    {
			return string.IsNullOrEmpty(_merchantId)
				? null
				: reportRequestService.GetAll()
					.FirstOrDefault(rrc => rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId
					                        && rrc.RequestReportId == null
					                        && RetryIntervalHelper.IsRetryPeriodAwaited(rrc.LastRequested, rrc.RequestRetryCount,
						                        _options.ReportRequestRetryInitialDelay, _options.ReportRequestRetryInterval,
						                        _options.ReportRequestRetryType)
					);
	    }



	    public string RequestReportFromAmazon(ReportRequestEntry reportRequestEntry)
	    {
		    var missingInformationExceptionMessage = "Cannot request report from amazon due to missing report request information";

			if (reportRequestEntry?.ReportRequestData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Report request data is null.");
		    if (string.IsNullOrEmpty(reportRequestEntry.ReportType)) throw new ArgumentException($"{missingInformationExceptionMessage}: Report Type is missing.");

			var reportRequestData = reportRequestEntry.GetPropertiesContainer();

			_logger.Info($"Attempting to request the next report in queue from Amazon: {reportRequestEntry.RegionAndTypeComputed}.");

			var reportRequest = new RequestReportRequest
			{
				Merchant = reportRequestEntry.MerchantId,
				ReportType = reportRequestEntry.ReportType
			};

		    if (reportRequestData.MarketplaceIdList != null)
			    reportRequest.MarketplaceIdList = new IdList {Id = reportRequestData.MarketplaceIdList};
			if (reportRequestData.StartDate.HasValue)
			    reportRequest.StartDate = reportRequestData.StartDate.Value;
		    if (reportRequestData.EndDate.HasValue)
			    reportRequest.EndDate = reportRequestData.EndDate.Value;
		    if (!string.IsNullOrEmpty(reportRequestData.ReportOptions))
			    reportRequest.ReportOptions = reportRequestData.ReportOptions;

		    try
		    {
			    var reportResponse = _marketplaceWebServiceClient.RequestReport(reportRequest);
			    var requestId = reportResponse?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = reportResponse?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info($"Request to MWS.RequestReport was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']", new RequestInfo(timestamp, requestId));

			    return reportResponse?.RequestReportResult?.ReportRequestInfo?.ReportRequestId;
		    }
		    catch (Exception e)
		    {
			    if (e is MarketplaceWebServiceException exception && exception.StatusCode == HttpStatusCode.BadRequest)
			    {
				    _logger.Error($"Request to MWS.RequestReport failed! [HttpStatusCode:'{exception.StatusCode}',ErrorType:'{exception.ErrorType}', ErrorCode:'{exception.ErrorCode}']", e);
					return HttpStatusCode.BadRequest.ToString();
			    }

			    _logger.Error($"Request to MWS.RequestReport failed!", e);
				return null;
			}
		}

	    public void MoveToQueueOfReportsToGenerate(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry, string reportRequestId)
	    {
			reportRequestEntry.RequestReportId = reportRequestId;
			reportRequestEntry.RequestRetryCount = 0;
			reportRequestService.Update(reportRequestEntry);
			reportRequestService.SaveChanges();
	    }

	    public IEnumerable<string> GetAllPendingReportFromQueue(IReportRequestCallbackService reportRequestService)
	    {
			    return string.IsNullOrEmpty(_merchantId)
				    ? new List<string>().AsEnumerable()
				    : reportRequestService
						.Where(rrcs => rrcs.AmazonRegion == _region && rrcs.MerchantId == _merchantId
					                   && rrcs.RequestReportId != null
					                   && rrcs.GeneratedReportId == null)
					    .Select(r => r.RequestReportId);
	    }

	    public List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant)
	    {
		    _logger.Info($"Attempting to request report processing statuses for all reports in queue.");

			var request = new GetReportRequestListRequest() {ReportRequestIdList = new IdList(), Merchant = merchant};
		    request.ReportRequestIdList.Id.AddRange(requestIdList);
		    var response = _marketplaceWebServiceClient.GetReportRequestList(request);
			var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
		    var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			_logger.Info($"Request to MWS.GetReportRequestList was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']", new RequestInfo(timestamp, requestId));

			var responseInformation = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>();

		    if (response != null)
		    {
			    foreach (var reportRequestInfo in response.GetReportRequestListResult.ReportRequestInfo)
			    {
				    responseInformation.Add(
					    (reportRequestInfo.ReportRequestId, reportRequestInfo.GeneratedReportId, reportRequestInfo
						    .ReportProcessingStatus));
			    }
		    }

		    return responseInformation;
	    }

	    public void CleanupReportRequests(IReportRequestCallbackService reportRequestService)
	    {
			_logger.Info("Executing cleanup of report requests queue.");
			var expiredReportRequests = reportRequestService.GetAll()
				.Where(rrc => (rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId) && IsRetryCountExceeded(rrc));

			foreach (var expiredReport in expiredReportRequests)
			{
				reportRequestService.Delete(expiredReport);
				_logger.Warn($"Report request {expiredReport.RegionAndTypeComputed} deleted from queue. Reason: The retry count of '{_options.ReportRequestMaxRetryCount}' was exceeded while trying to request the report from Amazon.");
			}

		    var entriesWithExpirationPeriodExceeded = reportRequestService.GetAll()
			    .Where(rrc => (rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId) && IsExpirationPeriodExceeded(rrc));

		    foreach (var expiredReport in entriesWithExpirationPeriodExceeded)
		    {
			    reportRequestService.Delete(expiredReport);
			    _logger.Warn($"Report request {expiredReport.RegionAndTypeComputed} deleted from queue. Reason: Expiration period of '{_options.ReportDownloadRequestEntryExpirationPeriod.Hours} hours' was exceeded.");
		    }

			reportRequestService.SaveChanges();
	    }

	    private bool IsRetryCountExceeded(ReportRequestEntry reportRequestEntry) => 
			(reportRequestEntry.RequestRetryCount > _options.ReportRequestMaxRetryCount);

	    private bool IsExpirationPeriodExceeded(ReportRequestEntry reportRequestEntry) =>
			(DateTime.Compare(reportRequestEntry.DateCreated, DateTime.UtcNow.Subtract(_options.ReportDownloadRequestEntryExpirationPeriod)) < 0);

		public void QueueReportsAccordingToProcessingStatus(IReportRequestCallbackService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses)
	    {
			foreach (var reportGenerationInfo in reportGenerationStatuses)
			{
				var reportGenerationCallback = reportRequestService.FirstOrDefault(rrc =>
					rrc.RequestReportId == reportGenerationInfo.ReportRequestId
					&& rrc.GeneratedReportId == null);
				if (reportGenerationCallback == null) continue;

				if (reportGenerationInfo.ReportProcessingStatus == "_DONE_")
				{
					reportGenerationCallback.GeneratedReportId = reportGenerationInfo.GeneratedReportId;
					reportGenerationCallback.RequestRetryCount = 0;
					reportRequestService.Update(reportGenerationCallback);
					_logger.Info(
						$"Report was successfully generated by Amazon for {reportGenerationCallback.RegionAndTypeComputed}. GeneratedReportId:'{reportGenerationInfo.GeneratedReportId}'. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_DONE_NO_DATA_")
				{
					reportRequestService.Delete(reportGenerationCallback);
					_logger.Warn(
						$"Report was successfully generated by Amazon for {reportGenerationCallback.RegionAndTypeComputed} but it didn't contain any data. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'. Report request is now dequeued.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_SUBMITTED_" ||
				            reportGenerationInfo.ReportProcessingStatus == "_IN_PROGRESS_")
				{
					reportGenerationCallback.GeneratedReportId = null;
					reportGenerationCallback.RequestRetryCount = 0;
					reportRequestService.Update(reportGenerationCallback);
					_logger.Info(
						$"Report generation by Amazon is still in progress for {reportGenerationCallback.RegionAndTypeComputed}. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_CANCELLED_")
				{
					reportGenerationCallback.RequestReportId = null;
					reportGenerationCallback.GeneratedReportId = null;
					reportGenerationCallback.RequestRetryCount++;
					reportRequestService.Update(reportGenerationCallback);
					_logger.Warn(
						$"Report generation was canceled by Amazon for {reportGenerationCallback.RegionAndTypeComputed}. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'. Placing report back in report request queue! Retry count is now '{reportGenerationCallback.RequestRetryCount}'.");
				}
				else
				{
					reportGenerationCallback.RequestReportId = null;
					reportGenerationCallback.GeneratedReportId = null;
					reportGenerationCallback.RequestRetryCount++;
					reportRequestService.Update(reportGenerationCallback);
					_logger.Warn(
						$"Report status returned by amazon is {reportGenerationInfo.ReportProcessingStatus} for {reportGenerationCallback.RegionAndTypeComputed}. This status is not yet handled by EasyMws. Placing report back in report request queue! Retry count is now '{reportGenerationCallback.RequestRetryCount}'");
				}
			}
			reportRequestService.SaveChanges();
	    }


	    public ReportRequestEntry GetNextFromQueueOfReportsToDownload(IReportRequestCallbackService reportRequestService)
	    {
			return string.IsNullOrEmpty(_merchantId)
				? null
				: reportRequestService.FirstOrDefault(
					rrc => rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId
					        && rrc.RequestReportId != null
					        && rrc.GeneratedReportId != null);
	    }

	    public Stream DownloadGeneratedReportFromAmazon(ReportRequestEntry reportRequestEntry)
	    {
		    _logger.Info($"Attempting to download the next report in queue from Amazon: {reportRequestEntry.RegionAndTypeComputed}.");

			var reportResultStream = new MemoryStream();
		    var getReportRequest = new GetReportRequest
		    {
			    ReportId = reportRequestEntry.GeneratedReportId,
			    Report = reportResultStream,
			    Merchant = reportRequestEntry.MerchantId
		    };

		    var response = _marketplaceWebServiceClient.GetReport(getReportRequest);

		    var reportContentStream = getReportRequest.Report;
			var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
		    var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			_logger.Info($"Request to MWS.GetReport was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']", new RequestInfo(timestamp, requestId));
			_logger.Info($"Report download from Amazon has succeeded for {reportRequestEntry.RegionAndTypeComputed}.");

			return reportContentStream;
	    }

	    public void RemoveFromQueue(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry)
	    {
			reportRequestService.Delete(reportRequestEntry);
			reportRequestService.SaveChanges();
	    }

	    public void MoveToRetryQueue(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry)
	    {
			reportRequestEntry.RequestRetryCount++;

			reportRequestService.Update(reportRequestEntry);
			reportRequestService.SaveChanges();

			_logger.Warn(
				$"Moving {reportRequestEntry.RegionAndTypeComputed} to retry queue. Retry count is now '{reportRequestEntry.RequestRetryCount}'.");
	    }
    }
}
