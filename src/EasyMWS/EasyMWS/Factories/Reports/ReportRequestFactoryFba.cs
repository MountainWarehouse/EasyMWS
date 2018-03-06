using System.Collections.Generic;
using MarketplaceWebService.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class ReportRequestFactoryFba : IReportRequestFactoryFba
    {
	    public RequestReportRequest GenerateRequestForReportGetAfnInventoryData(List<string> marketplaceIdList = null)
	    {
		    return new RequestReportRequest();
	    }

    }
}
