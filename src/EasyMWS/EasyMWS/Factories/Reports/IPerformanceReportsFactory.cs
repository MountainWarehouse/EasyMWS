using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface IPerformanceReportsFactory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_SELLER_FEEDBACK_DATA_<para />
		/// Tab-delimited flat file that returns negative and neutral feedback (one to three stars) from buyers who rated your seller performance. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileFeedbackReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_V1_SELLER_PERFORMANCE_REPORT_<para />
		/// XML file that contains the individual performance metrics data from the Seller Central dashboard. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XMLCustomerMetricsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);
	}
}
