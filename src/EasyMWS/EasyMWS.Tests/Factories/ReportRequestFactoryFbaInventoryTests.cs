using System;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories
{
    public class ReportRequestFactoryFbaInventoryTests
	{
	    private IReportRequestFactoryFba _reportRequestFactoryFBA;
	    private AmazonRegion _region = AmazonRegion.Europe;

	    [Test]
	    public void
			GenerateRequestForReportGetAfnInventoryData_ReturnsTypeReportRequestPropertiesContainer()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData();

		    Assert.AreEqual(typeof(ReportRequestPropertiesContainer), reportRequest.GetType());
	    }

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

		[Test]
		public void GenerateRequestForReportGetAfnInventoryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetAfnInventoryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetAfnInventoryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetAfnInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryData();

			Assert.AreEqual("_GET_AFN_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetAfnInventoryDataByCountry_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
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
		public void GenerateRequestForReportGetAfnInventoryDataByCountry_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetAfnInventoryDataByCountry_WithAmericanMarketplaceProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetAfnInventoryDataByCountry_WithNonEuMarketplaceProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry(marketplaceGroup));
		}

		[Test]
		public void GenerateRequestForReportGetAfnInventoryDataByCountry_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetAfnInventoryDataByCountry();

			Assert.AreEqual("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetExcessInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetExcessInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetExcessInventoryData_WithUSMarketplaceProvidedReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetExcessInventoryData_WithIndiaMarketplaceProvidedReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetExcessInventoryData_WithJapanMarketplaceProvidedReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetExcessInventoryData_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.France);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetExcessInventoryData_WithNorthAmericanNonUSMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetExcessInventoryData_WithInternationalMarketplace_NotIndiaOrJapan_Provided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData(marketplaceGroup));
		}

		[Test]
		public void GenerateRequestForReportGetExcessInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetExcessInventoryData();

			Assert.AreEqual("_GET_EXCESS_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}
		
		[Test]
		public void
			GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryHealthData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryHealthData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryHealthData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryHealthData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryHealthData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryHealthData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentInventoryHealthData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryHealthData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventorySummaryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventorySummaryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventorySummaryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventorySummaryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventorySummaryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentInventorySummaryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}


		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentInventorySummaryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentInventorySummaryData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaInventoryAgedData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaInventoryAgedData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetFbaInventoryAgedData_WithUSMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaInventoryAgedData_WithIndiaMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaInventoryAgedData_WithJapanMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaInventoryAgedData_WithNorthAmericanMarketplacesNonUSProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);
			marketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(marketplaceGroup));
		}

		[Test]
		public void GenerateRequestForReportGetFbaInventoryAgedData_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaInventoryAgedData_WithInternationalMarketplacesProvidedNotIndiaOrJapan_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData(marketplaceGroup));
		}

		[Test]
		public void GenerateRequestForReportGetFbaInventoryAgedData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaInventoryAgedData();

			Assert.AreEqual("_GET_FBA_INVENTORY_AGED_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiAllInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiAllInventoryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiAllInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiAllInventoryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetFbaMyiAllInventoryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiAllInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaMyiAllInventoryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiAllInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaMyiAllInventoryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiAllInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaMyiAllInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiAllInventoryData();

			Assert.AreEqual("_GET_FBA_MYI_ALL_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData();

			Assert.AreEqual("_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetReservedInventoryData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetReservedInventoryData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetReservedInventoryData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetReservedInventoryData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetReservedInventoryData_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetReservedInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetReservedInventoryData_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetReservedInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetReservedInventoryData_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetReservedInventoryData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetReservedInventoryData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetReservedInventoryData();

			Assert.AreEqual("_GET_RESERVED_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetRestockInventoryRecommendationsReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetRestockInventoryRecommendationsReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetRestockInventoryRecommendationsReport(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetRestockInventoryRecommendationsReport(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetRestockInventoryRecommendationsReport(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetRestockInventoryRecommendationsReport_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetRestockInventoryRecommendationsReport();

			Assert.AreEqual("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryLoaderData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryLoaderData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryLoaderData_WithUSMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryLoaderData_WithIndiaMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryLoaderData_WithJapanMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest =
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryLoaderData_WithNorthAmericanMarketplacesNonUSProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);
			marketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryLoaderData_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryLoaderData_WithInternationalMarketplacesProvidedNotIndiaOrJapan_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData(marketplaceGroup));
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryLoaderData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryLoaderData();

			Assert.AreEqual("_GET_STRANDED_INVENTORY_LOADER_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryUiData_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryUiData_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryUiData_WithUSMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryUiData_WithIndiaMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryUiData_WithJapanMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryUiData_WithNorthAmericanMarketplacesNonUSProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);
			marketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryUiData_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(marketplaceGroup));
		}

		[Test]
		public void
			GenerateRequestForReportGetStrandedInventoryUiData_WithInternationalMarketplacesProvidedNotIndiaOrJapan_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() =>
				_reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData(marketplaceGroup));
		}

		[Test]
		public void GenerateRequestForReportGetStrandedInventoryUiData_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.GenerateRequestForReportGetStrandedInventoryUiData();

			Assert.AreEqual("_GET_STRANDED_INVENTORY_UI_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}
	}
}
