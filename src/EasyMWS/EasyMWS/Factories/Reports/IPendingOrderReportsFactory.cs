using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// These pending order reports are only available in Japan and China. These reports can be both scheduled and requested where noted.
	/// </summary>
	public interface IPendingOrderReportsFactory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_PENDING_ORDERS_DATA_ <para />
		/// Tab-delimited flat file report that can be requested or scheduled that shows all pending orders. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFilePendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_PENDING_ORDERS_DATA_ <para />
		/// XML report that can be requested or scheduled that shows all pending orders. Can only be scheduled using Amazon MWS.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XMLPendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_CONVERGED_FLAT_FILE_PENDING_ORDERS_DATA_ <para />
		/// Flat file report that can be requested or scheduled that shows all pending orders. For Marketplace sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer ConvergedFlatFilePendingOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null);
	}
}
