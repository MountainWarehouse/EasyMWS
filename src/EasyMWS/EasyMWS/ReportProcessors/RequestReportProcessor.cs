using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketplaceWebService;
using MarketplaceWebService.Model;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
    internal class RequestReportProcessor : IRequestReportProcessor
    {
	    private readonly IReportRequestCallbackService _reportRequestCallbackService;
	    private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;

	    internal RequestReportProcessor(IMarketplaceWebServiceClient marketplaceWebServiceClient,
		    IReportRequestCallbackService reportRequestCallbackService) : this(marketplaceWebServiceClient)
	    {
		    _reportRequestCallbackService = reportRequestCallbackService;
		}

		public RequestReportProcessor(IMarketplaceWebServiceClient marketplaceWebServiceClient)
	    {
		    _marketplaceWebServiceClient = marketplaceWebServiceClient;
		    _reportRequestCallbackService = _reportRequestCallbackService ?? new ReportRequestCallbackService();
		}

	    public ReportRequestCallback GetNonRequestedReportFromQueue(AmazonRegion region)
	    {
		    return _reportRequestCallbackService.FirstOrDefault(x => x.AmazonRegion == region && x.RequestReportId == null);
		}

	    public string RequestSingleQueuedReport(ReportRequestCallback reportRequestCallback, string merchantId)
	    {
		    var reportRequestData = JsonConvert.DeserializeObject<ReportRequestPropertiesContainer>(reportRequestCallback.ReportRequestData);
		    var reportRequest = reportRequestData.ToMwsClientReportRequest(merchantId);

		    var reportResponse = _marketplaceWebServiceClient.RequestReport(reportRequest);

		   return reportResponse?.RequestReportResult?.ReportRequestInfo?.ReportRequestId;
		}

	    public void MoveToNonGeneratedReportsQueue(ReportRequestCallback reportRequestCallback, string reportRequestId)
	    {
		    reportRequestCallback.RequestReportId = reportRequestId;
			_reportRequestCallbackService.Update(reportRequestCallback);

			_reportRequestCallbackService.SaveChanges();
	    }

	    public async Task MoveToNonGeneratedReportsQueueAsync(ReportRequestCallback reportRequestCallback, string reportRequestId)
	    {
		    reportRequestCallback.RequestReportId = reportRequestId;
		    _reportRequestCallbackService.Update(reportRequestCallback);

		    await _reportRequestCallbackService.SaveChangesAsync();
	    }

	    public IEnumerable<ReportRequestCallback> GetAllPendingReport(AmazonRegion region)
	    {
		    return _reportRequestCallbackService.Where(rrcs => rrcs.AmazonRegion == region && rrcs.RequestReportId != null && rrcs.GeneratedReportId == null);
	    }

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

		    _reportRequestCallbackService.SaveChanges();

		}

	    public void MoveReportsBackToRequestQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportsStatusInformation)
	    {
		    var doneReportsInfo = reportsStatusInformation.Where(t => t.ReportProcessingStatus == "_CANCELLED_");

		    foreach (var reportInfo in doneReportsInfo)
		    {
			    var reportRequestCallback = _reportRequestCallbackService.FirstOrDefault(rrc => rrc.RequestReportId == reportInfo.ReportRequestId);
			    reportRequestCallback.RequestReportId = null;
			    _reportRequestCallbackService.Update(reportRequestCallback);
		    }

		    _reportRequestCallbackService.SaveChanges();

	    }

	    public ReportRequestCallback GetReadyForDownloadReports(AmazonRegion region)
	    {
		    return _reportRequestCallbackService.FirstOrDefault(rrc => rrc.AmazonRegion == region && rrc.RequestReportId != null && rrc.GeneratedReportId != null);
	    }

	    public Stream DownloadGeneratedReport(ReportRequestCallback reportRequestCallback, string merchantId)
	    {
		    var reportResultStream = new MemoryStream();
		    var reportContent = new StringBuilder();
		    var getReportRequest = new GetReportRequest
		    {
			    ReportId = reportRequestCallback.GeneratedReportId,
			    Report = reportResultStream,
			    Merchant = merchantId
		    };

		    _marketplaceWebServiceClient.GetReport(getReportRequest);

		    var streamReader = new StreamReader(getReportRequest.Report);
		    while (!streamReader.EndOfStream && streamReader.Peek() != -1)
		    {
			    reportContent.Append((char) streamReader.Read());
		    }

		    return getReportRequest.Report;
	    }

	    public void DequeueReportRequestCallback(ReportRequestCallback reportRequestCallback)
	    {
		    _reportRequestCallbackService.Delete(reportRequestCallback);
			_reportRequestCallbackService.SaveChanges();
	    }
	}
}
