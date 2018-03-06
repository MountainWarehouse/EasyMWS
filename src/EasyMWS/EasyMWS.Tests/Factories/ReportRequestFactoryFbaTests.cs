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
	    public void GenerateRequestForReportGetAfnInventoryData_WithNonNullMerchant_HasMerchantSetCorectly()
	    {
		    var testMerchant = "testMerchant3465";
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba(testMerchant);

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData();

		    Assert.AreEqual(testMerchant, reportRequest.Merchant);
	    }

	    [Test]
	    public void GenerateRequestForReportGetAfnInventoryData_WithNonmWsAuthToken_HasMwsAuthTokenSetCorectly()
	    {
		    var testmWsAuthToken = "mWsAuthToken3456";
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba(mWsAuthToken: testmWsAuthToken);

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData();

		    Assert.AreEqual(testmWsAuthToken, reportRequest.MWSAuthToken);
	    }

	    [Test]
	    public void GenerateRequestForReportGetAfnInventoryData_ReturnsReportRequest_WithCorrectType()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData();

		    Assert.AreEqual("_GET_AFN_INVENTORY_DATA_", reportRequest.ReportType);
	    }

		[Test]
	    public void
			GenerateRequestForReportGetAfnInventoryDataByCountry_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry(null);

		    Assert.NotNull(reportRequest);
		    Assert.IsNull(reportRequest.MarketplaceIdList);
	    }

	    [Test]
	    public void GenerateRequestForReportGetAfnInventoryDataByCountry_WithNonNullMerchant_HasMerchantSetCorectly()
	    {
		    var testMerchant = "testMerchant3465";
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba(testMerchant);

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry();

		    Assert.AreEqual(testMerchant, reportRequest.Merchant);
	    }

	    [Test]
	    public void GenerateRequestForReportGetAfnInventoryDataByCountry_WithNonmWsAuthToken_HasMwsAuthTokenSetCorectly()
	    {
		    var testmWsAuthToken = "mWsAuthToken3456";
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba(mWsAuthToken: testmWsAuthToken);

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry();

		    Assert.AreEqual(testmWsAuthToken, reportRequest.MWSAuthToken);
	    }

	    [Test]
	    public void GenerateRequestForReportGetAfnInventoryDataByCountry_ReturnsReportRequest_WithCorrectType()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry();

		    Assert.AreEqual("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_", reportRequest.ReportType);
	    }
	}
}
