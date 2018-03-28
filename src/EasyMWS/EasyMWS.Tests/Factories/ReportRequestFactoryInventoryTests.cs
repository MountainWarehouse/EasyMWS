using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories
{
    public class ReportRequestFactoryInventoryTests
    {
	    private IReportRequestFactoryInventory _reportRequestFactoryInventory;
	    private AmazonRegion _region = AmazonRegion.Europe;

	    [SetUp]
	    public void Setup()
	    {
		    _reportRequestFactoryInventory = new ReportRequestFactoryInventory();
	    }

	    [Test]
	    public void AllListingsReport_ReturnsType_ReportRequestPropertiesContainer()
	    {
		    _reportRequestFactoryInventory = new ReportRequestFactoryInventory();

		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport();

		    Assert.AreEqual(typeof(ReportRequestPropertiesContainer), propertiesContainer.GetType());
		}

		[Test]
	    public void AllListingsReport_WithNoMarketplacesProvided_ReturnsContainerWithReportOptionsPropertyContains_CustomArgumentSerializedWithValueFalse()
	    {
		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport();

			Assert.AreEqual("custom=false;", propertiesContainer.ReportOptions);
	    }

	    [Test]
	    public void
		    AllListingsReport_WithValidMarketplacesAndCustomFalseProvided_ReturnsContainerWithReportOptionsPropertyContains_CustomArgumentSerializedWithValueFalse()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

			var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, false);

		    Assert.AreEqual("custom=false;", propertiesContainer.ReportOptions);
		}

	    [Test]
	    public void
		    AllListingsReport_WithValidMarketplacesAndCustomTrueProvided_ReturnsContainerWithReportOptionsPropertyContains_CustomArgumentSerializedWithValueTrue()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true);

		    Assert.AreEqual("custom=true;", propertiesContainer.ReportOptions);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithNorthAmericanMarketplacesProvided_ReturnsRequest()
	    {
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);

			var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup);

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

			var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithNonUsOrEuMarketplacesProvided_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

			var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithCanadianMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithUSMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithUKMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);

		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithIndianMarketplaceAndCustomTrue_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);

		    var propertiesContainer = _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true);

		    Assert.NotNull(propertiesContainer);
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithNorthAmericanMarketplaceNonUSOrCAAndCustomTrue_ThrowsArgumentException()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Mexico);

		    Assert.Throws<ArgumentException>(() => _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true));
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithEUMarketplaceNonUKAndCustomTrue_ThrowsArgumentException()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.France);

		    Assert.Throws<ArgumentException>(() => _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true));
	    }

	    [Test]
	    public void GenerateRequestForReportFbaFeePreviewReport_WithInternationalMarketplaceNonIndiaAndCustomTrue_ThrowsArgumentException()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

		    Assert.Throws<ArgumentException>(() => _reportRequestFactoryInventory.AllListingsReport(marketplaceGroup, true));
	    }
	}
}
