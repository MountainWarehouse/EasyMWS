using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// Note that the date range that you specify when requesting an order report indicates when the orders became eligible for fulfillment (no longer in a "pending" state), not when the orders were created.
	/// </summary>
	public interface IOrderReportsFactory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ACTIONABLE_ORDER_DATA_ <para />
		/// Tab-delimited flat file report that contains only orders that are not confirmed as shipped. Can be requested or scheduled. For Marketplace and Seller Central sellers.<para/>
		/// This report accepts the following ReportOptions values: ShowSalesChannel<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="showSalesChannel">A Boolean value that indicates whether an additional column is added to the report that shows the sales channel. Default: false.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer UnshippedOrdersReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null, bool showSalesChannel = false);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_ORDERS_DATA_ <para />
		/// Scheduled XML order report. For Seller Central sellers only.<para/>
		/// You can only schedule one _GET_ORDERS_DATA_ or _GET_FLAT_FILE_ORDERS_DATA_ (Requested or Scheduled Flat File Order Report) report at a time. If you have one of these reports scheduled and you schedule a new report, the existing report will be canceled.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer ScheduledXMLOrderReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ORDERS_DATA_ <para />
		/// Tab-delimited flat file order report that can be requested or scheduled. The report shows orders from the previous 60 days. For Marketplace and Seller Central sellers.<para/>
		/// Seller Central sellers can only schedule one _GET_ORDERS_DATA_ (Scheduled XML Order Report) or _GET_FLAT_FILE_ORDERS_DATA_ report at a time. If you have one of these reports scheduled and you schedule a new report, the existing report will be canceled.<para/>
		/// Marketplace sellers can only schedule one _GET_FLAT_FILE_ORDERS_DATA_ or _GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_ (Flat File Order Report) report at a time. If you have one of these reports scheduled and you schedule a new report, the existing report will be canceled.<para/>
		/// Note: The format of this report will differ slightly depending on whether it is scheduled or requested.<para/>
		/// This report accepts the following ReportOptions values: ShowSalesChannel <para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="showSalesChannel">A Boolean value that indicates whether an additional column is added to the report that shows the sales channel. Default: false.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer RequestedOrScheduledFlatFileOrderReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null, bool showSalesChannel = false);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_ <para />
		/// Tab-delimited flat file order report that can be requested or scheduled. For Marketplace sellers only.<para/>
		/// You can only schedule one _GET_FLAT_FILE_ORDERS_DATA_ or _GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_ report at a time. If you have one of these reports scheduled and you schedule a new report, the existing report will be canceled.<para/>
		/// Note: The format of this report will differ slightly depending on whether it is scheduled or requested. For example, the format for the dates will differ, and the ship-method column is only returned when the report is requested.<para/>
		/// This report accepts the following ReportOptions values: ShowSalesChannel <para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="showSalesChannel">A Boolean value that indicates whether an additional column is added to the report that shows the sales channel. Default: false.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileOrderReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<string> requestedMarketplacesGroup = null, bool showSalesChannel = false);
	}
}
