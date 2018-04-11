using System;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories
{
	public class ReportRequestFactoryFbaReplenTests
	{
		private IReportRequestFactoryFba _reportRequestFactoryFba;
		private AmazonRegion _region;

		[Test]
		public void SetUp()
		{
			_region = AmazonRegion.Europe;
		}
		[Test]
		public void
			GenerateSuggestedFbaReplenReport_ValidMarketPlaceRequested_ShouldReturnTypeReportRequestPropertiesContainer()
		{
			var marketPlaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			_reportRequestFactoryFba = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFba.GenerateSuggestedFbaReplenReport(marketPlaceGroup);

			Assert.AreEqual(typeof(ReportRequestPropertiesContainer), reportRequest.GetType());
		}

		[Test]
		public void
			GenerateSuggestedFbaReplenReport_NotValidMarketPlaceRequested_ShouldThrowArgumentException()
		{
			var marketPlaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);
			_reportRequestFactoryFba = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(()=> _reportRequestFactoryFba.GenerateSuggestedFbaReplenReport(marketPlaceGroup),"Should have thrown exception for unsupported marketplace");			
		}

		[Test]
		public void
			GenerateSuggestedFbaReplenReport_WithNoMarketplaceProvided_ShouldReturnMarketPlaceIdNotSet()
		{
			_reportRequestFactoryFba = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFba.GenerateSuggestedFbaReplenReport();

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void GenerateSuggestedFbaReplenReport_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFba = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFba.GenerateSuggestedFbaReplenReport();

			Assert.AreEqual("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, reportRequest.UpdateFrequency);
		}

	}
}