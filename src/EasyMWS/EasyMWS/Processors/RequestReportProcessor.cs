using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	    private readonly IAmazonReportService _amazonReportStorageService;
	    private readonly IReportRequestCallbackService _reportRequestCallbackService;
	    private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
	    private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
	    private readonly AmazonRegion _region;
	    private readonly string _merchantId;

	    internal RequestReportProcessor(AmazonRegion region, string merchantId,
		    IMarketplaceWebServiceClient marketplaceWebServiceClient,
		    IReportRequestCallbackService reportRequestCallbackService, IAmazonReportService amazonReportStorageService,
		    IEasyMwsLogger logger, EasyMwsOptions options) : this(region, merchantId, marketplaceWebServiceClient, logger, options)
	    {
		    _reportRequestCallbackService = reportRequestCallbackService;
		    _amazonReportStorageService = amazonReportStorageService;
	    }

	    internal RequestReportProcessor(AmazonRegion region, string merchantId, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
	    {
		    _region = region;
		    _merchantId = merchantId;
		    _options = options;
		    _logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
		    _reportRequestCallbackService = _reportRequestCallbackService ?? new ReportRequestCallbackService(_options, _logger);
		    _amazonReportStorageService = _amazonReportStorageService ?? new AmazonReportService(_options);
	    }

	    public ReportRequestEntry GetNextFromQueueOfReportsToRequest() =>
		    string.IsNullOrEmpty(_merchantId) ? null : _reportRequestCallbackService.GetAll()
				    .FirstOrDefault(rrc => rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId
					    && rrc.RequestReportId == null
					    && RetryIntervalHelper.IsRetryPeriodAwaited(rrc.LastRequested, rrc.RequestRetryCount,
						    _options.ReportRequestRetryInitialDelay, _options.ReportRequestRetryInterval, _options.ReportRequestRetryType)
				    );
		
	    public string RequestReportFromAmazon(ReportRequestEntry reportRequestEntry)
	    {
		    var missingInformationExceptionMessage = "Cannot submit queued feed to amazon due to missing feed submission information";

			if (reportRequestEntry?.ReportRequestData == null) throw new ArgumentNullException(missingInformationExceptionMessage);

			var reportRequestData = reportRequestEntry.GetPropertiesContainer();
		    if (reportRequestData?.ReportType == null) throw new ArgumentException(missingInformationExceptionMessage);

			_logger.Info($"Attempting to request the next report in queue from Amazon: {reportRequestEntry.RegionAndTypeComputed}.");

			var reportRequest = new RequestReportRequest
			{
				Merchant = reportRequestEntry.MerchantId,
				ReportType = reportRequestData.ReportType
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
				_logger.Error($"Request to MWS.RequestReport failed!", e);
			    return null;
			}
		}

	    public void MoveToQueueOfReportsToGenerate(ReportRequestEntry reportRequestEntry, string reportRequestId)
	    {
		    reportRequestEntry.RequestReportId = reportRequestId;
		    reportRequestEntry.RequestRetryCount = 0;
			_reportRequestCallbackService.Update(reportRequestEntry);
	    }

	    public IEnumerable<string> GetAllPendingReportFromQueue() =>
		    string.IsNullOrEmpty(_merchantId) ? new List<string>().AsEnumerable() : _reportRequestCallbackService
			.Where(rrcs => rrcs.AmazonRegion == _region && rrcs.MerchantId == _merchantId
			            && rrcs.RequestReportId != null
			            && rrcs.GeneratedReportId == null)
			.Select(r => r.RequestReportId);

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

	    public void MoveReportsToGeneratedQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportsStatusInformation)
	    {
		    var doneReportsInfo = reportsStatusInformation.Where(t => t.ReportProcessingStatus == "_DONE_" && t.GeneratedReportId != null);

		    foreach (var reportInfo in doneReportsInfo)
		    {
			    var reportRequestCallback = _reportRequestCallbackService.FirstOrDefault(rrc => rrc.RequestReportId == reportInfo.ReportRequestId);
			    reportRequestCallback.GeneratedReportId = reportInfo.GeneratedReportId;
				_reportRequestCallbackService.Update(reportRequestCallback);
		    }
		}

	    public void MoveReportsBackToRequestQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportsStatusInformation)
	    {
		    var doneReportsInfo = reportsStatusInformation.Where(t => t.ReportProcessingStatus == "_CANCELLED_");

		    foreach (var reportInfo in doneReportsInfo)
		    {
			    var reportRequestCallback = _reportRequestCallbackService.FirstOrDefault(rrc => rrc.RequestReportId == reportInfo.ReportRequestId);
			    reportRequestCallback.RequestReportId = null;
			    reportRequestCallback.RequestRetryCount++;

			    _reportRequestCallbackService.Update(reportRequestCallback);
		    }
	    }

	    public void CleanupReportRequests()
	    {
			_logger.Info("Executing cleanup of report requests queue.");
		    var expiredReportRequests = _reportRequestCallbackService.GetAll()
			    .Where(rrc => rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId
					&& rrc.RequestRetryCount > _options.ReportRequestMaxRetryCount)
				.Select(r => new {r.Id, r.RegionAndTypeComputed});

		    if (expiredReportRequests.Any())
		    {
			    _logger.Warn("The following report requests have exceeded their retry limit and will now be deleted :");
			    foreach (var expiredReport in expiredReportRequests)
			    {
				    _reportRequestCallbackService.Delete(expiredReport.Id);
				    _logger.Warn($"Report request {expiredReport.RegionAndTypeComputed} deleted from queue.");
				}
		    }
		}

		public void QueueReportsAccordingToProcessingStatus(
		    List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses)
	    {
		    foreach (var reportGenerationInfo in reportGenerationStatuses)
		    {
			    var reportGenerationCallback = _reportRequestCallbackService.FirstOrDefault(rrc =>
					    rrc.RequestReportId == reportGenerationInfo.ReportRequestId
						&& rrc.GeneratedReportId == null);
				if(reportGenerationCallback == null) continue;

				if (reportGenerationInfo.ReportProcessingStatus == "_DONE_")
			    {
				    reportGenerationCallback.GeneratedReportId = reportGenerationInfo.GeneratedReportId;
				    reportGenerationCallback.RequestRetryCount = 0;
					_reportRequestCallbackService.Update(reportGenerationCallback);
				    _logger.Info($"Report was successfully generated by Amazon for {reportGenerationCallback.RegionAndTypeComputed}. GeneratedReportId:'{reportGenerationInfo.GeneratedReportId}'. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_DONE_NO_DATA_")
			    {
				    _reportRequestCallbackService.Delete(reportGenerationCallback.Id);
					_logger.Warn($"Report was successfully generated by Amazon for {reportGenerationCallback.RegionAndTypeComputed} but it didn't contain any data. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'. Report request is now dequeued.");
				}
			    else if (reportGenerationInfo.ReportProcessingStatus == "_SUBMITTED_" ||
			             reportGenerationInfo.ReportProcessingStatus == "_IN_PROGRESS_")
			    {
				    reportGenerationCallback.GeneratedReportId = null;
				    reportGenerationCallback.RequestRetryCount = 0;
				    _reportRequestCallbackService.Update(reportGenerationCallback);
				    _logger.Info(
					    $"Report generation by Amazon is still in progress for {reportGenerationCallback.RegionAndTypeComputed}. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'.");
			    }
			    else if (reportGenerationInfo.ReportProcessingStatus == "_CANCELLED_")
			    {
				    reportGenerationCallback.RequestReportId = null;
				    reportGenerationCallback.GeneratedReportId = null;
				    reportGenerationCallback.RequestRetryCount++;
				    _reportRequestCallbackService.Update(reportGenerationCallback);
				    _logger.Warn(
					    $"Report generation was canceled by Amazon for {reportGenerationCallback.RegionAndTypeComputed}. ProcessingStatus:'{reportGenerationInfo.ReportProcessingStatus}'. Placing report back in report request queue! Retry count is now '{reportGenerationCallback.RequestRetryCount}'.");
			    }
			    else
			    {
				    reportGenerationCallback.RequestReportId = null;
				    reportGenerationCallback.GeneratedReportId = null;
				    reportGenerationCallback.RequestRetryCount++;
				    _reportRequestCallbackService.Update(reportGenerationCallback);
				    _logger.Warn(
					    $"Report status returned by amazon is {reportGenerationInfo.ReportProcessingStatus} for {reportGenerationCallback.RegionAndTypeComputed}. This status is not yet handled by EasyMws. Placing report back in report request queue! Retry count is now '{reportGenerationCallback.RequestRetryCount}'");
			    }
		    }
	    }


	    public ReportRequestEntry GetNextFromQueueOfReportsToDownload() =>
		    string.IsNullOrEmpty(_merchantId) ? null : _reportRequestCallbackService.FirstOrDefault(
			    rrc => rrc.AmazonRegion == _region && rrc.MerchantId == _merchantId
			           && rrc.RequestReportId != null
			           && rrc.GeneratedReportId != null);

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

			if (_options.KeepAmazonReportsInLocalDbAfterCallbackIsPerformed)
		    {
			    _logger.Info($"Backup report storage in local easyMws database is enabled. To disable it, update the KeepAmazonReportsInLocalDbAfterCallbackIsPerformed option.");

				var requestPropertyContainer = reportRequestEntry.GetPropertiesContainer();
			    var reportType = requestPropertyContainer?.ReportType ?? "unknown";

			    _logger.Info($"Proceeding to save backup report content in EasyMws internal database for {reportRequestEntry.RegionAndTypeComputed}");
				StoreAmazonReportToInternalStorage(reportContentStream, reportType, requestId, timestamp);
		    }

			return reportContentStream;
	    }

	    public void RemoveFromQueue(int reportRequestCallbackId)
	    {
		    _reportRequestCallbackService.Delete(reportRequestCallbackId);
	    }

	    public void MoveToRetryQueue(ReportRequestEntry reportRequestEntry)
	    {
			reportRequestEntry.RequestRetryCount++;

		    _reportRequestCallbackService.Update(reportRequestEntry);

		    _logger.Warn($"Moving {reportRequestEntry.RegionAndTypeComputed} to retry queue. Retry count is now '{reportRequestEntry.RequestRetryCount}'.");
		}

	    private void StoreAmazonReportToInternalStorage(Stream reportContent, string reportType, string requestId, string timestamp)
	    {
			var sb = new StringBuilder();
			var sr = new StreamReader(reportContent);
		    reportContent.Position = 0;
		    sb.Append(sr.ReadToEnd());
		    reportContent.Position = 0;

		    _amazonReportStorageService.Create(new AmazonReport
		    {
			    Content = sb.ToString(),
				DateCreated = DateTime.UtcNow,
				ReportType = reportType,
				DownloadRequestId = requestId,
				DownloadTimestamp = timestamp
			});
			_amazonReportStorageService.SaveChanges();
		}
    }
}
