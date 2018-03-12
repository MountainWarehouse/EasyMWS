using System.Linq;
using MarketplaceWebService;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
    internal class RequestReportProcessor
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

	    internal ReportRequestCallback GetFrontOfNonRequestedReportsQueue(AmazonRegion region)
	    {
		    return _reportRequestCallbackService.FirstOrDefault(x => x.AmazonRegion == region && x.RequestReportId == null);
		}

	    internal string RequestSingleQueuedReport(ReportRequestCallback reportRequestCallback)
	    {
		    var reportRequestData = JsonConvert.DeserializeObject<ReportRequestPropertiesContainer>(reportRequestCallback.ReportRequestData);
		    var reportRequest = reportRequestData.ToMwsClientReportRequest();

		    var reportResponse = _marketplaceWebServiceClient.RequestReport(reportRequest);

		   return reportResponse?.RequestReportResult?.ReportRequestInfo?.ReportRequestId;
		}

	}
}
