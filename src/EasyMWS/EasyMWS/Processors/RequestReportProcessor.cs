using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
    internal class RequestReportProcessor : IRequestReportProcessor
    {
	    private readonly IReportRequestCallbackService _reportRequestCallbackService;
	    private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
	    private readonly EasyMwsOptions _options;

		internal RequestReportProcessor(IMarketplaceWebServiceClient marketplaceWebServiceClient,
		    IReportRequestCallbackService reportRequestCallbackService, EasyMwsOptions options) : this(marketplaceWebServiceClient, options)
	    {
		    _reportRequestCallbackService = reportRequestCallbackService;
		}

		internal RequestReportProcessor(IMarketplaceWebServiceClient marketplaceWebServiceClient, EasyMwsOptions options)
	    {
		    _marketplaceWebServiceClient = marketplaceWebServiceClient;
		    _reportRequestCallbackService = _reportRequestCallbackService ?? new ReportRequestCallbackService();
		    _options = options;
	    }

	    public ReportRequestCallback GetNonRequestedReportFromQueue(AmazonRegion region, string merchantId) =>
		    string.IsNullOrEmpty(merchantId) ? null : _reportRequestCallbackService.GetAll()
				    .FirstOrDefault(rrc => rrc.AmazonRegion == region && rrc.MerchantId == merchantId
					    && rrc.RequestReportId == null
					    && RetryIntervalHelper.IsRetryPeriodAwaited(rrc.LastRequested, rrc.RequestRetryCount,
						    _options.ReportRequestRetryInitialDelay, _options.ReportRequestRetryInterval, _options.ReportRequestRetryType)
				    );
		
	    public string RequestSingleQueuedReport(ReportRequestCallback reportRequestCallback, string merchantId)
	    {
		    if (reportRequestCallback == null || string.IsNullOrEmpty(merchantId))
			    throw new ArgumentNullException("Cannot submit queued feed to amazon due to missing feed submission information or empty merchant ID");

			var reportRequestData = JsonConvert.DeserializeObject<ReportRequestPropertiesContainer>(reportRequestCallback.ReportRequestData);
			var reportRequest = new RequestReportRequest
			{
				Merchant = reportRequestCallback.MerchantId,
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
			    return reportResponse?.RequestReportResult?.ReportRequestInfo?.ReportRequestId;
			}
		    catch (Exception)
		    {
				return null;
		    }
		}

	    public void MoveToNonGeneratedReportsQueue(ReportRequestCallback reportRequestCallback, string reportRequestId)
	    {
		    reportRequestCallback.RequestReportId = reportRequestId;
		    reportRequestCallback.RequestRetryCount = 0;
			_reportRequestCallbackService.Update(reportRequestCallback);
	    }

	    public IEnumerable<ReportRequestCallback> GetAllPendingReportFromQueue(AmazonRegion region, string merchantId) =>
		    string.IsNullOrEmpty(merchantId) ? new List<ReportRequestCallback>().AsEnumerable() : _reportRequestCallbackService.Where(
			    rrcs => rrcs.AmazonRegion == region && rrcs.MerchantId == merchantId
			            && rrcs.RequestReportId != null
			            && rrcs.GeneratedReportId == null);

		public List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportRequestListResponse(IEnumerable<string> requestIdList, string merchant)
	    {
		    var request = new GetReportRequestListRequest() {ReportRequestIdList = new IdList(), Merchant = merchant};
		    request.ReportRequestIdList.Id.AddRange(requestIdList);
		    var response = _marketplaceWebServiceClient.GetReportRequestList(request);

		    var responseInformation = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>();

		    foreach (var reportRequestInfo in response.GetReportRequestListResult.ReportRequestInfo)
		    {
			    responseInformation.Add((reportRequestInfo.ReportRequestId, reportRequestInfo.GeneratedReportId, reportRequestInfo.ReportProcessingStatus));
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

	    public void MoveReportsToQueuesAccordingToProcessingStatus(
		    List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses)
	    {
		    foreach (var reportGenerationInfo in reportGenerationStatuses)
		    {
			    var reportGenerationCallback = _reportRequestCallbackService.FirstOrDefault(rrc =>
					    rrc.RequestReportId == reportGenerationInfo.ReportRequestId
						&& rrc.GeneratedReportId == null);
				if(reportGenerationCallback == null) continue;

			    if (reportGenerationInfo.ReportProcessingStatus == "_DONE_" ||
			        reportGenerationInfo.ReportProcessingStatus == "_DONE_NO_DATA_")
			    {
				    reportGenerationCallback.GeneratedReportId = reportGenerationInfo.GeneratedReportId;
				    reportGenerationCallback.RequestRetryCount = 0;
					_reportRequestCallbackService.Update(reportGenerationCallback);
				}
			    else if (reportGenerationInfo.ReportProcessingStatus == "_SUBMITTED_" ||
			             reportGenerationInfo.ReportProcessingStatus == "_IN_PROGRESS_")
			    {
					reportGenerationCallback.GeneratedReportId = null;
				    reportGenerationCallback.RequestRetryCount = 0;
					_reportRequestCallbackService.Update(reportGenerationCallback);
				}
			    else if (reportGenerationInfo.ReportProcessingStatus == "_CANCELLED_")
			    {
				    reportGenerationCallback.RequestReportId = null;
				    reportGenerationCallback.GeneratedReportId = null;
					reportGenerationCallback.RequestRetryCount++;
				    _reportRequestCallbackService.Update(reportGenerationCallback);
				}
			    else
			    {
				    reportGenerationCallback.RequestReportId = null;
				    reportGenerationCallback.GeneratedReportId = null;
				    reportGenerationCallback.RequestRetryCount++;
				    _reportRequestCallbackService.Update(reportGenerationCallback);
				}
		    }
	    }


	    public ReportRequestCallback GetReadyForDownloadReportsFromQueue(AmazonRegion region, string merchantId) =>
		    string.IsNullOrEmpty(merchantId) ? null : _reportRequestCallbackService.FirstOrDefault(
			    rrc => rrc.AmazonRegion == region && rrc.MerchantId == merchantId
			           && rrc.RequestReportId != null
			           && rrc.GeneratedReportId != null);

		public Stream DownloadGeneratedReportFromAmazon(ReportRequestCallback reportRequestCallback, string merchantId)
		{
			var reportResultStream = new MemoryStream();
			var getReportRequest = new GetReportRequest
			{
				ReportId = reportRequestCallback.GeneratedReportId,
				Report = reportResultStream,
				Merchant = merchantId
			};

			_marketplaceWebServiceClient.GetReport(getReportRequest);

			return getReportRequest.Report;
	    }

	    public void DequeueReportRequestCallback(ReportRequestCallback reportRequestCallback)
	    {
		    _reportRequestCallbackService.Delete(reportRequestCallback);
	    }

	    public void MoveToRetryQueue(ReportRequestCallback reportRequestCallback)
	    {
			reportRequestCallback.RequestRetryCount++;

		    _reportRequestCallbackService.Update(reportRequestCallback);
		}
    }
}
