using System;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class ReportRequestSerializablePropertiesExtensionsTests
    {
	    [Test]
	    public void ToMwsClientReportRequest_WithNullArgument_ReturnsNull()
	    {
		    ReportRequestPropertiesContainer propertiesContainer = null;

		    var reportRequest = propertiesContainer.ToMwsClientReportRequest();

			Assert.IsNull(reportRequest);
	    }

	    [Test]
	    public void ToMwsClientReportRequest_WithArgumentContainingRequiredProperties_ReturnsRequestWithTheSamePropertiesSet()
	    {
		    var testMwsAuthToken = "testMwsAuthToken1234";
		    var testMerchant = "testMerchant123";
		    var testReportType = "_Test_Report_Type_";
		    var propertiesContainer = new ReportRequestPropertiesContainer(testReportType, testMerchant, testMwsAuthToken, ContentUpdateFrequency.Unknown);

		    var reportRequest = propertiesContainer.ToMwsClientReportRequest();

		    Assert.AreEqual(testMwsAuthToken,reportRequest.MWSAuthToken);
		    Assert.AreEqual(testMerchant, reportRequest.Merchant);
		    Assert.AreEqual(testReportType, reportRequest.ReportType);
		    Assert.IsNull(reportRequest.MarketplaceIdList);
		    Assert.AreEqual(DateTime.MinValue, reportRequest.EndDate);
		    Assert.AreEqual(DateTime.MinValue, reportRequest.StartDate);
			Assert.IsNull(reportRequest.ReportOptions);
		}
    }
}
