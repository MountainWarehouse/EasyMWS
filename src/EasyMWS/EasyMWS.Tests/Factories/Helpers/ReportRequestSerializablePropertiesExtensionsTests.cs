﻿using System;
using MountainWarehouse.EasyMWS;
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

		    var reportRequest = propertiesContainer.ToMwsClientReportRequest("");

			Assert.IsNull(reportRequest);
	    }

	    [Test]
	    public void ToMwsClientReportRequest_WithArgumentContainingRequiredProperties_ReturnsRequestWithTheSamePropertiesSet()
	    {
		    var testMerchant = "testMerchant123";
		    var testReportType = "_Test_Report_Type_";
		    var propertiesContainer = new ReportRequestPropertiesContainer(testReportType, ContentUpdateFrequency.Unknown);

		    var reportRequest = propertiesContainer.ToMwsClientReportRequest(testMerchant);

		    Assert.AreEqual(testMerchant, reportRequest.Merchant);
		    Assert.AreEqual(testReportType, reportRequest.ReportType);
		    Assert.IsNull(reportRequest.MarketplaceIdList);
		    Assert.AreEqual(DateTime.MinValue, reportRequest.EndDate);
		    Assert.AreEqual(DateTime.MinValue, reportRequest.StartDate);
			Assert.IsNull(reportRequest.ReportOptions);
		}
    }
}