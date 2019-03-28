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
	    private IFbaReportsFactory _fbaReportsFactory;
	    private AmazonRegion _region = AmazonRegion.Europe;

	    [Test]
	    public void
			FbaAmazonFulfilledInventoryReport_ReturnsTypeReportRequestPropertiesContainer()
	    {
		    _fbaReportsFactory = new FbaReportsFactory();

		    var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport();

		    Assert.AreEqual(typeof(ReportRequestPropertiesContainer), reportRequest.GetType());
	    }

		[Test]
		public void
			FbaAmazonFulfilledInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaAmazonFulfilledInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaAmazonFulfilledInventoryReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaAmazonFulfilledInventoryReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaAmazonFulfilledInventoryReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaAmazonFulfilledInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaAmazonFulfilledInventoryReport();

			Assert.AreEqual("_GET_AFN_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaMultiCountryInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMultiCountryInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaMultiCountryInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMultiCountryInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaMultiCountryInventoryReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMultiCountryInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaMultiCountryInventoryReport_WithAmericanMarketplaceProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaMultiCountryInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaMultiCountryInventoryReport_WithNonEuMarketplaceProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaMultiCountryInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void FbaMultiCountryInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMultiCountryInventoryReport();

			Assert.AreEqual("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaManageExcessInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageExcessInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaManageExcessInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageExcessInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaManageExcessInventoryReport_WithUSMarketplaceProvidedReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageExcessInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageExcessInventoryReport_WithIndiaMarketplaceProvidedReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageExcessInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageExcessInventoryReport_WithJapanMarketplaceProvidedReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageExcessInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageExcessInventoryReport_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.France);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaManageExcessInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaManageExcessInventoryReport_WithNorthAmericanNonUSMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaManageExcessInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaManageExcessInventoryReport_WithInternationalMarketplace_NotIndiaOrJapan_Provided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaManageExcessInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void FbaManageExcessInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageExcessInventoryReport();

			Assert.AreEqual("_GET_EXCESS_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaCrossBorderInventoryMovementReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaCrossBorderInventoryMovementReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaCrossBorderInventoryMovementReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaCrossBorderInventoryMovementReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaCrossBorderInventoryMovementReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaCrossBorderInventoryMovementReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaCrossBorderInventoryMovementReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaCrossBorderInventoryMovementReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaCrossBorderInventoryMovementReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaCrossBorderInventoryMovementReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}
		
		[Test]
		public void
			FbaCrossBorderInventoryMovementReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaCrossBorderInventoryMovementReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaDailyInventoryHistoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaDailyInventoryHistoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaDailyInventoryHistoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaDailyInventoryHistoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaDailyInventoryHistoryReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaDailyInventoryHistoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaDailyInventoryHistoryReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaDailyInventoryHistoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaDailyInventoryHistoryReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaDailyInventoryHistoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaDailyInventoryHistoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaDailyInventoryHistoryReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaInboundPerformanceReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInboundPerformanceReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInboundPerformanceReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInboundPerformanceReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInboundPerformanceReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInboundPerformanceReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInboundPerformanceReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInboundPerformanceReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInboundPerformanceReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInboundPerformanceReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaInboundPerformanceReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInboundPerformanceReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaInventoryAdjustmentsReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAdjustmentsReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryAdjustmentsReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAdjustmentsReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryAdjustmentsReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryAdjustmentsReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryAdjustmentsReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryAdjustmentsReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryAdjustmentsReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryAdjustmentsReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaInventoryAdjustmentsReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAdjustmentsReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaInventoryHealthReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryHealthReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryHealthReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryHealthReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryHealthReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryHealthReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryHealthReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryHealthReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryHealthReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryHealthReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaInventoryHealthReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryHealthReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaReceivedInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReceivedInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaReceivedInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReceivedInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaReceivedInventoryReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaReceivedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaReceivedInventoryReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaReceivedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaReceivedInventoryReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaReceivedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaReceivedInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReceivedInventoryReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaInventoryEventDetailReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryEventDetailReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryEventDetailReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryEventDetailReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryEventDetailReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryEventDetailReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryEventDetailReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryEventDetailReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryEventDetailReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaInventoryEventDetailReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}


		[Test]
		public void FbaInventoryEventDetailReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryEventDetailReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaMonthlyInventoryHistoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMonthlyInventoryHistoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaMonthlyInventoryHistoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMonthlyInventoryHistoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaMonthlyInventoryHistoryReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaMonthlyInventoryHistoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaMonthlyInventoryHistoryReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaMonthlyInventoryHistoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaMonthlyInventoryHistoryReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaMonthlyInventoryHistoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaMonthlyInventoryHistoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaMonthlyInventoryHistoryReport();

			Assert.AreEqual("_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaInventoryAgeReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAgeReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaInventoryAgeReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAgeReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaInventoryAgeReport_WithUSMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAgeReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaInventoryAgeReport_WithIndiaMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAgeReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaInventoryAgeReport_WithJapanMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAgeReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaInventoryAgeReport_WithNorthAmericanMarketplacesNonUSProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);
			marketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaInventoryAgeReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void FbaInventoryAgeReport_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaInventoryAgeReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaInventoryAgeReport_WithInternationalMarketplacesProvidedNotIndiaOrJapan_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaInventoryAgeReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void FbaInventoryAgeReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaInventoryAgeReport();

			Assert.AreEqual("_GET_FBA_INVENTORY_AGED_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.Daily, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaManageInventoryArchived_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventoryArchived();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaManageInventoryArchived_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventoryArchived(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaManageInventoryArchived_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventoryArchived(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageInventoryArchived_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventoryArchived(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageInventoryArchived_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventoryArchived(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageInventoryArchived_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventoryArchived();

			Assert.AreEqual("_GET_FBA_MYI_ALL_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaManageInventory_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventory();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaManageInventory_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventory(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaManageInventory_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaManageInventory(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaManageInventory_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaManageInventory(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaManageInventory_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaManageInventory(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaManageInventory_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaManageInventory();

			Assert.AreEqual("_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaReservedInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReservedInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaReservedInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReservedInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaReservedInventoryReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReservedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaReservedInventoryReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReservedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaReservedInventoryReport_WithNonUsOrEUMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReservedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaReservedInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaReservedInventoryReport();

			Assert.AreEqual("_GET_RESERVED_INVENTORY_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.RestockInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.RestockInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}


		[Test]
		public void
			GenerateRequestForReportGetRestockInventoryRecommendationsReport_WithUSMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.RestockInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}


		[Test]
		public void
			GenerateSuggestedFbaReplenReport_NotValidMarketPlaceRequested_ShouldThrowArgumentException()
		{
			var marketPlaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() => _fbaReportsFactory.RestockInventoryReport(requestedMarketplaces: marketPlaceGroup.GetMarketplaces), "Should have thrown exception for unsupported marketplace");
		}

		[Test]
		public void
			GenerateSuggestedFbaReplenReport_WithNoMarketplaceProvided_ShouldReturnMarketPlaceIdNotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.RestockInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateRequestForReportGetRestockInventoryRecommendationsReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.RestockInventoryReport();

			Assert.AreEqual("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaBulkFixStrandedInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaBulkFixStrandedInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaBulkFixStrandedInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaBulkFixStrandedInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaBulkFixStrandedInventoryReport_WithUSMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaBulkFixStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaBulkFixStrandedInventoryReport_WithIndiaMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaBulkFixStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaBulkFixStrandedInventoryReport_WithJapanMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest =
				_fbaReportsFactory.FbaBulkFixStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaBulkFixStrandedInventoryReport_WithNorthAmericanMarketplacesNonUSProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);
			marketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaBulkFixStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaBulkFixStrandedInventoryReport_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaBulkFixStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaBulkFixStrandedInventoryReport_WithInternationalMarketplacesProvidedNotIndiaOrJapan_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaBulkFixStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void FbaBulkFixStrandedInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaBulkFixStrandedInventoryReport();

			Assert.AreEqual("_GET_STRANDED_INVENTORY_LOADER_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

		[Test]
		public void
			FbaStrandedInventoryReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaStrandedInventoryReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaStrandedInventoryReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaStrandedInventoryReport(null);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void FbaStrandedInventoryReport_WithUSMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaStrandedInventoryReport_WithIndiaMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaStrandedInventoryReport_WithJapanMarketplaceProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void
			FbaStrandedInventoryReport_WithNorthAmericanMarketplacesNonUSProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);
			marketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaStrandedInventoryReport_WithEuropeanMarketplacesProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void
			FbaStrandedInventoryReport_WithInternationalMarketplacesProvidedNotIndiaOrJapan_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);
			_fbaReportsFactory = new FbaReportsFactory();

			Assert.Throws<ArgumentException>(() =>
				_fbaReportsFactory.FbaStrandedInventoryReport(requestedMarketplaces: marketplaceGroup.GetMarketplaces));
		}

		[Test]
		public void FbaStrandedInventoryReport_ReturnsReportRequest_WithCorrectType()
		{
			_fbaReportsFactory = new FbaReportsFactory();

			var reportRequest = _fbaReportsFactory.FbaStrandedInventoryReport();

			Assert.AreEqual("_GET_STRANDED_INVENTORY_UI_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}
	}
}
