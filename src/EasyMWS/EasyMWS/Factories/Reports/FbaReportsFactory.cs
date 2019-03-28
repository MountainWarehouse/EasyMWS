using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	/// </summary>
	public class FbaReportsFactory : IFbaReportsFactory
	{
		#region FBA Inventory Reports

		public ReportRequestPropertiesContainer FbaAmazonFulfilledShipmentsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AMAZON_FULFILLED_SHIPMENTS_DATA_", ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FlatFileAllOrdersReportByLastUpdate(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FlatFileAllOrdersReportByOrderDate(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer XMLAllOrdersReportByLastUpdate(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer XMLAllOrdersReportByOrderDate(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FBACustomerShipmentSalesReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_SALES_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FBAPromotionsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_PROMOTION_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FBACustomerTaxes(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_TAXES_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaAmazonFulfilledInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AFN_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaMultiCountryInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_",
				ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaManageExcessInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_EXCESS_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaCrossBorderInventoryMovementReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaDailyInventoryHistoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaInboundPerformanceReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaInventoryAdjustmentsReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaInventoryHealthReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaReceivedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaInventoryEventDetailReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaMonthlyInventoryHistoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaInventoryAgeReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_INVENTORY_AGED_DATA_", ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaManageInventoryArchived(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_MYI_ALL_INVENTORY_DATA_",
				ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaManageInventory(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_",
				ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaReservedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_RESERVED_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer RestockInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_",
				ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaBulkFixStrandedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_STRANDED_INVENTORY_LOADER_DATA_",
				ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaStrandedInventoryReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_STRANDED_INVENTORY_UI_DATA_",
				ContentUpdateFrequency.NearRealTime,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		#endregion

		#region FBA Payment Reports

		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		public ReportRequestPropertiesContainer FbaFeePreviewReport(DateTime startDate,
			DateTime? endDate, IEnumerable<string> requestedMarketplaces = null)
		{
			var permittedMarketplacesIds = new List<MwsMarketplace> { MwsMarketplace.US, MwsMarketplace.Canada, MwsMarketplace.Mexico };
			permittedMarketplacesIds.AddRange(MwsMarketplaceGroup.AmazonEurope());
			return ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_",
			   ContentUpdateFrequency.AtLeast72Hours,
			   requestedMarketplaces: requestedMarketplaces,
			   startDate: startDate, endDate: endDate ?? DateTime.UtcNow);
		}


		public ReportRequestPropertiesContainer FbaReimbursementsReport(DateTime startDate, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_REIMBURSEMENTS_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		#endregion

		#region FBA Customer Concessions Reports

		public ReportRequestPropertiesContainer FbaReturnsReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplaces = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_",
				ContentUpdateFrequency.Daily,
				requestedMarketplaces: requestedMarketplaces,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaReplacementsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_REPLACEMENT_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaRecommendedRemovalReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_RECOMMENDED_REMOVAL_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaRemovalOrderDetailReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_REMOVAL_ORDER_DETAIL_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer FbaRemovalShipmentDetailReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
			=> ReportGeneratorHelper.GenerateReportRequest("_GET_FBA_FULFILLMENT_REMOVAL_SHIPMENT_DETAIL_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup,
				startDate: startDate, endDate: endDate);

		#endregion
	}
}
