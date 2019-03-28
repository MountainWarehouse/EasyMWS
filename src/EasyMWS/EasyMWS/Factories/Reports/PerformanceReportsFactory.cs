using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	/// </summary>
	public class PerformanceReportsFactory : IPerformanceReportsFactory
	{
		public ReportRequestPropertiesContainer FlatFileFeedbackReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer XMLCustomerMetricsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}
	}
}
