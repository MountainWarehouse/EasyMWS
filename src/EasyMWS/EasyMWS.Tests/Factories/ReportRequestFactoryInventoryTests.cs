using System;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories
{
    public class ReportRequestFactoryInventoryTests
    {
	    private IInventoryReportsFactory _inventoryReportsFactory;
	    private AmazonRegion _region = AmazonRegion.Europe;

	    [SetUp]
	    public void Setup()
	    {
		    _inventoryReportsFactory = new InventoryReportsFactory();
	    }

	    [Test]
	    public void AllListingsReport_ReturnsType_ReportRequestPropertiesContainer()
	    {
		    _inventoryReportsFactory = new InventoryReportsFactory();

		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport();

		    Assert.AreEqual(typeof(ReportRequestPropertiesContainer), propertiesContainer.GetType());
		}

		[Test]
	    public void AllListingsReport_WithNoMarketplacesProvided_ReturnsContainerWithReportOptionsPropertyContains_CustomArgumentSerializedWithValueFalse()
	    {
		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport();

			Assert.AreEqual("custom=false;", propertiesContainer.ReportOptions);
	    }

	    [Test]
	    public void
		    AllListingsReport_WithValidMarketplacesAndCustomFalseProvided_ReturnsContainerWithReportOptionsPropertyContains_CustomArgumentSerializedWithValueFalse()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

			var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList,custom: false);

		    Assert.AreEqual("custom=false;", propertiesContainer.ReportOptions);
		}

	    [Test]
	    public void
		    AllListingsReport_WithValidMarketplacesAndCustomTrueProvided_ReturnsContainerWithReportOptionsPropertyContains_CustomArgumentSerializedWithValueTrue()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true);

		    Assert.AreEqual("custom=true;", propertiesContainer.ReportOptions);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithNorthAmericanMarketplacesProvided_ReturnsRequest()
	    {
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);

			var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList);

			Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
	    {
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);

			var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithNonUsOrEuMarketplacesProvided_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

			var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithCanadianMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithUSMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithUKMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);

		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithIndianMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);

		    var propertiesContainer = _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithNorthAmericanMarketplaceNonUSOrCAAndCustomTrue_ThrowsArgumentException()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Mexico);

		    Assert.Throws<ArgumentException>(() => _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true));
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithEUMarketplaceNonUKAndCustomTrue_ThrowsArgumentException()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.France);

		    Assert.Throws<ArgumentException>(() => _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true));
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithInternationalMarketplaceNonIndiaAndCustomTrue_ThrowsArgumentException()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

		    Assert.Throws<ArgumentException>(() => _inventoryReportsFactory.AllListingsReport(requestedMarketplacesGroup: marketplaceGroup.GetMarketplacesIdList, custom: true));
	    }
	}
}
