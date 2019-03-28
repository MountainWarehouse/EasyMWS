using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	/// </summary>
	public class OrderReportsFactory : IOrderReportsFactory
	{
		public ReportRequestPropertiesContainer UnshippedOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null, bool showSalesChannel = false)
		{
			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("showSalesChannel", showSalesChannel);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ACTIONABLE_ORDER_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}


		public ReportRequestPropertiesContainer ScheduledXMLOrderReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer RequestedOrScheduledFlatFileOrderReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null, bool showSalesChannel = false)
		{
			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("showSalesChannel", showSalesChannel);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}

		public ReportRequestPropertiesContainer FlatFileOrderReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplacesGroup = null, bool showSalesChannel = false)
		{
			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("showSalesChannel", showSalesChannel);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}
	}
}
