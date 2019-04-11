using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	public class PendingOrderReportsFactory : IPendingOrderReportsFactory
	{
		public ReportRequestPropertiesContainer ConvergedFlatFilePendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_CONVERGED_FLAT_FILE_PENDING_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FlatFilePendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_PENDING_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer XMLPendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
        => ReportGeneratorHelper.GenerateReportRequest("_GET_PENDING_ORDERS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);
	}
}
