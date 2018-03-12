using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
    internal class RequestReportProcessor
    {
	    private readonly IReportRequestCallbackService _reportRequestCallbackService;

		public RequestReportProcessor(IReportRequestCallbackService reportRequestCallbackService = null)
	    {
			_reportRequestCallbackService = reportRequestCallbackService ?? new ReportRequestCallbackService();
	    }

	    internal ReportRequestCallback GetFrontOfNonRequestedReportsQueue(AmazonRegion region)
	    {
		    return _reportRequestCallbackService.FirstOrDefault(x => x.AmazonRegion == region && x.RequestReportId == null);
		}
    }
}
