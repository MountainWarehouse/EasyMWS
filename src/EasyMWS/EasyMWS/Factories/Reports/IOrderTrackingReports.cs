using System;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// These order tracking reports are available in North America (NA) and Europe (EU), and can be used by all Amazon sellers. These reports return all orders, regardless of fulfillment channel or shipment status.<para/> 
	/// These reports are intended for order tracking, not to drive your fulfillment process, as the reports do not include customer-identifying information and scheduling is not supported.<para/> 
	/// Also note that for self-fulfilled orders, item price is not shown for orders in a "pending" state.<para/>
	/// </summary>
	public interface IOrderTrackingReports
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_ <para />
		/// Tab-delimited flat file report that shows all orders updated in the specified period. Cannot be scheduled. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileOrdersByLastUpdateReport(DateTime? startDate = null, DateTime? endDate = null,
		    MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_ <para />
		/// Tab-delimited flat file report that shows all orders that were placed in the specified period. Cannot be scheduled. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileOrdersByOrderDateReport(DateTime? startDate = null, DateTime? endDate = null,
		    MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ARCHIVED_ORDERS_DATA_BY_ORDER_DATE_ <para />
		/// Tab-delimited flat file report that shows all archived orders that were placed in the specified period. Cannot be scheduled. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileArchivedOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
		    MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_ <para />
		/// XML report that shows all orders updated in the specified period. Cannot be scheduled. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XMLOrdersByLastUpdateReport(DateTime? startDate = null, DateTime? endDate = null,
		    MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_ <para />
		/// XML report that shows all orders that were placed in the specified period. Cannot be scheduled. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XMLOrdersByOrderDateReport(DateTime? startDate = null, DateTime? endDate = null,
		    MwsMarketplaceGroup requestedMarketplacesGroup = null);
	}
}
