using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class OrderReportsFactory : IOrderReportsFactory
	{
		public ReportRequestPropertiesContainer UnshippedOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
			MwsMarketplaceGroup requestedMarketplacesGroup = null, bool showSalesChannel = false)
		{
			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("showSalesChannel", showSalesChannel);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ACTIONABLE_ORDER_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList(),
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}


		public ReportRequestPropertiesContainer ScheduledXMLOrderReport(DateTime? startDate = null, DateTime? endDate = null,
			MwsMarketplaceGroup requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList(),
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer RequestedOrScheduledFlatFileOrderReport(DateTime? startDate = null,
			DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null, bool showSalesChannel = false)
		{
			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("showSalesChannel", showSalesChannel);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList(),
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}

		public ReportRequestPropertiesContainer FlatFileOrderReport(DateTime? startDate = null, DateTime? endDate = null,
			MwsMarketplaceGroup requestedMarketplacesGroup = null, bool showSalesChannel = false)
		{
			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("showSalesChannel", showSalesChannel);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList(),
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}
	}
}
