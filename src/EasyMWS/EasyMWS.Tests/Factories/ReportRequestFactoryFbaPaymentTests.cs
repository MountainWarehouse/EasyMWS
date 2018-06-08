using System;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Factories
{
    public class ReportRequestFactoryFbaPaymentTests
    {
	    private IReportRequestFactoryFba _reportRequestFactoryFBA;
	    private AmazonRegion _region = AmazonRegion.Europe;

		[Test]
		public void
			FbaFeePreviewReport_ReturnsTypeReportRequestPropertiesContainer()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow);

			Assert.AreEqual(typeof(ReportRequestPropertiesContainer), reportRequest.GetType());
		}

	    [Test]
	    public void FbaFeePreviewReport_ReturnsRequestWithStartDateSetAsExpected()
	    {
		    var testStartDate = DateTime.UtcNow.AddDays(-2);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(testStartDate);

		    Assert.NotNull(reportRequest);
		    Assert.IsNotNull(reportRequest.StartDate);
			Assert.AreEqual(testStartDate, reportRequest.StartDate);
		}

		[Test]
		public void
			FbaFeePreviewReport_WithNoMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

		[Test]
		public void
			FbaFeePreviewReport_WithNullMarketplaceProvided_ReturnsRequestWithMarketplaceIdList_NotSet()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow);

			Assert.NotNull(reportRequest);
			Assert.IsNull(reportRequest.MarketplaceIdList);
		}

	    [Test]
	    public void
		    FbaFeePreviewReport_WithNoEndDateProvided_ReturnsRequestWithEndDateSetToUtcNow()
	    {
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow);

		    Assert.NotNull(reportRequest);
		    Assert.IsTrue(reportRequest.EndDate - DateTime.UtcNow < TimeSpan.FromSeconds(1));
	    }

		[Test]
		public void FbaFeePreviewReport_WithEuropeanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France)
				.AddMarketplace(MwsMarketplace.Italy)
				.AddMarketplace(MwsMarketplace.Spain);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow,requestedMarketplacesGroup: marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

	    [Test]
	    public void FbaFeePreviewReport_WithNorthAmericanMarketplacesProvided_ReturnsRequest()
	    {
		    var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
			    .AddMarketplace(MwsMarketplace.Canada)
			    .AddMarketplace(MwsMarketplace.Mexico);
		    _reportRequestFactoryFBA = new ReportRequestFactoryFba();

		    var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow, requestedMarketplacesGroup: marketplaceGroup);

		    Assert.NotNull(reportRequest);
	    }

		[Test]
		public void FbaFeePreviewReport_WithAmericanMarketplacesProvided_ReturnsRequest()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US)
				.AddMarketplace(MwsMarketplace.Canada)
				.AddMarketplace(MwsMarketplace.Mexico);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow, requestedMarketplacesGroup: marketplaceGroup);

			Assert.NotNull(reportRequest);
		}

		[Test]
		public void FbaFeePreviewReport_WithNonUsOrEUMarketplaceProvided_ThrowsArgumentException()
		{
			var marketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			Assert.Throws<ArgumentException>(() => _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow, requestedMarketplacesGroup: marketplaceGroup));
		}

		[Test]
		public void FbaFeePreviewReport_ReturnsReportRequest_WithCorrectType()
		{
			_reportRequestFactoryFBA = new ReportRequestFactoryFba();

			var reportRequest = _reportRequestFactoryFBA.FbaFeePreviewReport(DateTime.UtcNow);

			Assert.AreEqual("_GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_", reportRequest.ReportType);
			Assert.AreEqual(ContentUpdateFrequency.AtLeast72Hours, reportRequest.UpdateFrequency);
		}
	}
}
