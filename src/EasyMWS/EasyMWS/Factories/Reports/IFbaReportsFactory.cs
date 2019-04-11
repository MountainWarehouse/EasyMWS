using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// Factory that can generate report requests for amazon MWS client.<para />
	/// At least one MarketplaceId value is required for IEnumerableings Reports. No MarketplaceId value is required for reports that are not IEnumerableings Reports. <para />
	/// When providing no MarketplaceId value for a reports that is not a IEnumerableings Reports, data for all marketplaces the seller is registered in will be shown.<para />
	/// For more info see : https://docs.developer.amazonservices.com/en_UK/reports/Reports_ReportType.html
	/// </summary>
	public interface IFbaReportsFactory
	{
		#region FBA Sales Reports

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AMAZON_FULFILLED_SHIPMENTS_DATA_ <para />
		/// Tab-delimited flat file. Contains detailed order/shipment/item information including price, address, and tracking data.<para />
		/// You can request up to one month of data in a single report. Content updated near real-time in Europe (EU), Japan, and North America (NA).<para/>
		/// In China, content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.<para/>
		/// Note: In Japan, EU, and NA, in most cases, there will be a delay of approximately one to three hours from the time a fulfillment order ships<para/>
		/// and the time the items in the fulfillment order appear in the report. In some rare cases there could be a delay of up to 24 hours.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaAmazonFulfilledShipmentsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_ <para />
		/// Tab-delimited flat file. Returns all orders updated in the specified date range regardless of fulfillment channel or shipment status. <para />
		/// This report is intended for order tracking, not to drive your fulfillment process; it does not include customer identifying information and scheduling is not supported. For all sellers. <para />
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileAllOrdersReportByLastUpdate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_ <para />
		/// Tab-delimited flat file. Returns all orders placed in the specified date range regardless of fulfillment channel or shipment status. <para />
		/// This report is intended for order tracking, not to drive your fulfillment process; it does not include customer identifying information and scheduling is not supported. For all sellers. <para />
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileAllOrdersReportByOrderDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_ <para />
		/// XML file order report that returns all orders updated in the specified date range regardless of fulfillment channel or shipment status. <para />
		/// This report is intended for order tracking, not to drive your fulfillment process; it does not include customer identifying information and scheduling is not supported. For all sellers. <para />
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XMLAllOrdersReportByLastUpdate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_ <para />
		/// XML file order report that returns all orders placed in the specified date range regardless of fulfillment channel or shipment status. <para />
		/// This report is intended for order tracking, not to drive your fulfillment process; it does not include customer identifying information and scheduling is not supported. For all sellers. <para />
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XMLAllOrdersReportByOrderDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_SALES_DATA_ <para />
		/// Tab-delimited flat file. Contains condensed item level data on shipped FBA customer orders including price, quantity, and ship to location. <para />
		/// Content updated near real-time in Europe (EU), Japan, and North America (NA). In China, content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers. <para />
		/// Note: In Japan, EU, and NA, in most cases, there will be a delay of approximately one to three hours from the time a fulfillment order ships and the time the items in the fulfillment order appear in the report. In some rare cases there could be a delay of up to 24 hours.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FBACustomerShipmentSalesReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_PROMOTION_DATA_ <para />
		/// Tab-delimited flat file. Contains promotions applied to FBA customer orders sold through Amazon; e.g. Super Saver Shipping. <para />
		/// Content updated daily. This report is only available to FBA sellers in the North America (NA) region. For Marketplace and Seller Central sellers. <para />
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FBAPromotionsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CUSTOMER_TAXES_DATA_ <para />
		/// Tab-delimited flat file for tax-enabled US sellers. This report contains data through February 28, 2013. All new transaction data can be found in the Sales Tax Report. <para />
		/// For FBA sellers only. For Marketplace and Seller Central sellers. <para />
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FBACustomerTaxes(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		#endregion

		#region FBA Inventory Reports

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AFN_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaAmazonFulfilledInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AFN_INVENTORY_DATA_BY_COUNTRY_ <para />
		/// Tab-delimited flat file. Contains quantity available for local fulfillment by country, helping Multi-Country Inventory sellers in Europe track their FBA inventory. <para />
		/// Content updated in near-real time. This report is only available to FBA sellers in European (EU) marketplaces. <para />
		/// For Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>a
		ReportRequestPropertiesContainer FbaMultiCountryInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_EXCESS_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains IEnumerableings with excess inventory, which helps sellers take action to sell through faster. <para />
		/// Content updated in near real-time. This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Excess Inventory Report.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaManageExcessInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_ <para />
		/// Tab delimited flat file. Contains historical data of shipments that crossed country borders. <para />
		/// For export shipments or for shipments using Amazon's European Fulfillment Network (note that Amazon's European Fulfillment Network is for Seller Central sellers only). <para />
		/// Content updated daily. For Marketplace and Seller Central sellers.<para />
		/// 
		/// Possible deprecated : present in http://s3.amazonaws.com documentation, but missing in https://docs.developer.amazonservices.com documentation.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaCrossBorderInventoryMovementReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains historical daily snapshots of your available inventory in Amazon’s fulfillment centers including quantity, location and disposition. <para />
		/// Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaDailyInventoryHistoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_ <para />
		/// Tab-delimited flat file. Contains inbound shipment problems by product and shipment.<para />
		/// Content updated daily. For Marketplace and Seller Central sellers. <para />
		/// This report is only available to FBA sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaInboundPerformanceReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_ <para />
		/// Tab-delimited flat file. Contains corrections and updates to your inventory in response to issues such as damage, loss, receiving discrepancies, etc. Content updated daily. <para />
		/// For FBA sellers only. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaInventoryAdjustmentsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_ <para />
		/// Tab-delimited flat file. Contains information about inventory age, condition, sales volume, weeks of cover, and price. Content updated daily. For FBA Sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaInventoryHealthReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_ <para />
		/// Tab-delimited flat file. Contains inventory that has completed the receive process at Amazon’s fulfillment centers. <para />
		/// Content updated daily. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaReceivedInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_ <para />
		/// Tab-delimited flat file. Contains history of inventory events (e.g. receipts, shipments, adjustments etc.) by SKU and Fulfillment Center. <para />
		/// Content updated daily. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaInventoryEventDetailReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains historical monthly snapshots of your available inventory in Amazon’s fulfillment centers including average and end-of-month quantity, location and disposition. <para />
		/// Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaMonthlyInventoryHistoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_INVENTORY_AGED_DATA_ <para />
		/// Tab-delimited flat file. Indicates the age of inventory, which helps sellers take action to avoid paying the Long Term Storage Fee. <para />
		/// Content updated daily. This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Inventory Age Report.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaInventoryAgeReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_MYI_ALL_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains current details of all (including archived) inventory including condition, quantity and volume. <para />
		/// Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaManageInventoryArchived(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains current details of active (not archived) inventory including condition, quantity and volume.<para />
		///  Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaManageInventory(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_RESERVED_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Provides data about the number of reserved units in your inventory. <para />
		/// Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaReservedInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_ <para />
		/// Tab delimited flat file. Provides recommendations on products to restock, suggested order quantities, and reorder dates. <para />
		/// For more information, see Restock Inventory Report. Content updated in near real-time. <para />
		/// This report is only available to FBA sellers in the US marketplace.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer RestockInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_STRANDED_INVENTORY_LOADER_DATA_ <para />
		/// Tab-delimited flat file. Contains a IEnumerable of stranded inventory. <para />
		/// Update the IEnumerableing information in this report file and then submit the file using Content updated in near real-time. <para />
		/// This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Bulk Fix Stranded Inventory Report.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaBulkFixStrandedInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_STRANDED_INVENTORY_UI_DATA_ <para />
		/// Tab-delimited flat file. Contains a breakdown of inventory in stranded status, including recommended actions. Content updated in near real-time. <para />
		/// This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Stranded Inventory Report.
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaStrandedInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_FBA_STORAGE_FEE_CHARGES_DATA_ <para />
        /// Tab-delimited flat file. Contains estimated monthly inventory storage fees for each ASIN of a seller's inventory in Amazon fulfillment centers. For FBA sellers only.
        /// </summary>
        /// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
        ReportRequestPropertiesContainer FbaStorageFeesReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        #endregion

        #region FBA Payment Reports

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_ <para />
        /// Tab-delimited flat file. Contains the estimated Amazon Selling and Fulfillment Fees for your FBA inventory with active offers. <para />
        /// The content is updated at least once every 72 hours. <para />
        /// To successfully generate a report, specify the StartDate parameter of a minimum 72 hours prior to NOW and EndDate to NOW.<para />
        /// For FBA sellers in the NA and EU only. For Marketplace and Seller Central sellers.						<para />																					
        /// </summary>
        /// <param name="endDate">To successfully generate a report, specify EndDate to NOW. If endDate is not specified, it is set automatically to NOW.</param>
        /// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
        /// <param name="startDate">To successfully generate a report, specify the StartDate parameter to a minimum of 72 hours prior to NOW</param>
        /// <returns></returns>
        ReportRequestPropertiesContainer FbaFeePreviewReport(DateTime startDate, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_REIMBURSEMENTS_DATA_ <para />
		/// Tab-delimited flat file. Contains itemized details of your inventory reimbursements including the reason for the reimbursement. Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.<para/>																					
		/// </summary>
		/// <param name="endDate">To successfully generate a report, specify EndDate to NOW. If endDate is not specified, it is set automatically to NOW.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="startDate">To successfully generate a report, specify the StartDate parameter to a minimum of 72 hours prior to NOW</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaReimbursementsReport(DateTime startDate, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		#endregion

		#region FBA Customer Concessions Reports

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_ <para />
		/// Tab-delimited flat file. Contains customer returned items received at an Amazon fulfillment center, including Return Reason and Disposition.<para />
		/// Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.<para />
		/// </summary>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaReturnsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_REPLACEMENT_DATA_ <para />
		/// Tab-delimited flat file. Contains replacements that have been issued to customers for completed orders. Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers. Available in the US and China (CN) only.<para/>
		/// </summary>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaReplacementsReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		#endregion

		#region FBA Removals Reports

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_RECOMMENDED_REMOVAL_DATA_ <para />
		/// Tab-delimited flat file. The report identifies sellable items that will be 365 days or older during the next Long-Term Storage cleanup event, if the report is generated within six weeks of the cleanup event date.<para/>
		/// The report includes both sellable and unsellable items. Content updated daily. For FBA sellers. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaRecommendedRemovalReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_REMOVAL_ORDER_DETAIL_DATA_ <para />
		/// Tab-delimited flat file. This report contains all the removal orders, including the items in each removal order, placed during any given time period.<para/>
		/// This can be used to help reconcile the total number of items requested to be removed from an Amazon fulfillment center with the actual number of items removed along with the status of each item in the removal order.<para/>
		/// Content updated in near real-time. For FBA sellers. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaRemovalOrderDetailReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_REMOVAL_SHIPMENT_DETAIL_DATA_ <para />
		/// Tab-delimited flat file. This report provides shipment tracking information for all removal orders and includes the items that have been removed from an Amazon fulfillment center for either a single removal order or for a date range.<para/>
		/// This report will not include canceled returns or disposed items; it is only for shipment information. Content updated in near real-time. For FBA sellers. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional IEnumerable of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FbaRemovalShipmentDetailReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_FBA_UNO_INVENTORY_DATA_ <para />
        /// Tab-delimited flat file. Contains all of your products are enrolled in the Small & Light program and how much inventory you currently have in Small & Light fulfillment centers.<para/>
        /// The report also shows your current prices and whether any of your products are in unsellable status. This report is only available in the Germany, Japan, UK, and US marketplaces.<para/>
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer SmallAndLightInventoryReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        #endregion

    }
}
