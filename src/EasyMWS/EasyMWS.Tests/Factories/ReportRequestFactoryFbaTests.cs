using MountainWarehouse.EasyMWS.Factories.Reports;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories
{
    public class ReportRequestFactoryFbaTests
    {
	    private ReportRequestFactoryFba _reportRequestFactoryFBA;

	    [Test]
	    public void
		    GenerateRequestForReportGetAfnInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData();

		    Assert.NotNull(reportRequest);
		    Assert.IsNull(reportRequest.MarketplaceIdList);
	    }

	    [Test]
	    public void
		    GenerateRequestForReportGetAfnInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData(null);

		    Assert.NotNull(reportRequest);
		    Assert.IsNull(reportRequest.MarketplaceIdList);
	    }
	}
}
