using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	public class PerformanceReportsFactory : IPerformanceReportsFactory
	{
		public ReportRequestPropertiesContainer FlatFileFeedbackReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
         => ReportGeneratorHelper.GenerateReportRequest("_GET_SELLER_FEEDBACK_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer XMLCustomerMetricsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
         => ReportGeneratorHelper.GenerateReportRequest("_GET_V1_SELLER_PERFORMANCE_REPORT_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);
    }
}
