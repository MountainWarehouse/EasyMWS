using MountainWarehouse.EasyMWS.Model;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public static class ReportsPermittedMarketplacesMapper
    {
		/// <summary>
		/// A mapping between Report type and the collection of marketplaces for which that report can be requested from amazon, 
		/// as defined here : https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html
		/// </summary>
		private static Dictionary<string, IEnumerable<MwsMarketplace>> _reportsMarketplaces = new Dictionary<string, IEnumerable<MwsMarketplace>>
		{
            // Performance Reports
            { "_GET_SELLER_FEEDBACK_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_V1_SELLER_PERFORMANCE_REPORT_",  MwsMarketplaceGroup.AmazonGlobal() },

            // Returns Reports
            { "_GET_CSV_MFN_PRIME_RETURNS_REPORT_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_FLAT_FILE_MFN_SKU_RETURN_ATTRIBUTES_REPORT_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_FLAT_FILE_RETURNS_DATA_BY_RETURN_DATE_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_XML_MFN_PRIME_RETURNS_REPORT_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_XML_MFN_SKU_RETURN_ATTRIBUTES_REPORT_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_XML_RETURNS_DATA_BY_RETURN_DATE_",  MwsMarketplaceGroup.AmazonGlobal() },

            // Pending Order Reports
            { "_GET_CONVERGED_FLAT_FILE_PENDING_ORDERS_DATA_", new List<MwsMarketplace> { MwsMarketplace.Japan, MwsMarketplace.China } },
            { "_GET_FLAT_FILE_PENDING_ORDERS_DATA_", new List<MwsMarketplace> { MwsMarketplace.Japan, MwsMarketplace.China } },
            { "_GET_PENDING_ORDERS_DATA_", new List<MwsMarketplace> { MwsMarketplace.Japan, MwsMarketplace.China } },

            // Order Tracking Reports
            { "_GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_", MwsMarketplaceGroup.AmazonNorthAmericaAndEurope() },
            { "_GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_", MwsMarketplaceGroup.AmazonNorthAmericaAndEurope() },
            { "_GET_FLAT_FILE_ARCHIVED_ORDERS_DATA_BY_ORDER_DATE_", MwsMarketplaceGroup.AmazonNorthAmericaAndEurope() },
            { "_GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_", MwsMarketplaceGroup.AmazonNorthAmericaAndEurope() },
            { "_GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_", MwsMarketplaceGroup.AmazonNorthAmericaAndEurope() },

			// Fba Reports
			{ "_GET_AMAZON_FULFILLED_SHIPMENTS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_SALES_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_PROMOTION_DATA_",  MwsMarketplaceGroup.AmazonNorthAmerica() },
			{ "_GET_FBA_FULFILLMENT_CUSTOMER_TAXES_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_AFN_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_AFN_INVENTORY_DATA_BY_COUNTRY_",  MwsMarketplaceGroup.AmazonEurope() },
			{ "_GET_EXCESS_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonUSAndIndiaAndJapan() },
			{ "_GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_INVENTORY_AGED_DATA_",  MwsMarketplaceGroup.AmazonUSAndIndiaAndJapan() },
			{ "_GET_FBA_MYI_ALL_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_RESERVED_INVENTORY_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_",  new List<MwsMarketplace> {MwsMarketplace.US} },
			{ "_GET_STRANDED_INVENTORY_LOADER_DATA_",  MwsMarketplaceGroup.AmazonUSAndIndiaAndJapan() },
			{ "_GET_STRANDED_INVENTORY_UI_DATA_", MwsMarketplaceGroup.AmazonUSAndIndiaAndJapan() },
            { "_GET_FBA_STORAGE_FEE_CHARGES_DATA_", MwsMarketplaceGroup.AmazonGlobal() },

            { "_GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_",  MwsMarketplaceGroup.AmazonNorthAmericaAndEurope() },
			{ "_GET_FBA_REIMBURSEMENTS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },

			{ "_GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_REPLACEMENT_DATA_", new List<MwsMarketplace> { MwsMarketplace.US, MwsMarketplace.China } },

            { "_GET_FBA_RECOMMENDED_REMOVAL_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_REMOVAL_ORDER_DETAIL_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FBA_FULFILLMENT_REMOVAL_SHIPMENT_DETAIL_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_FBA_UNO_INVENTORY_DATA_",  new List<MwsMarketplace> { MwsMarketplace.Germany, MwsMarketplace.Japan, MwsMarketplace.UK, MwsMarketplace.US } },

			// Inventory Reports
			{ "_GET_MERCHANT_LISTINGS_ALL_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_FLAT_FILE_OPEN_LISTINGS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_LISTINGS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_LISTINGS_INACTIVE_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_LISTINGS_DATA_BACK_COMPAT_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_LISTINGS_DATA_LITE_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_LISTINGS_DATA_LITER_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_CANCELLED_LISTINGS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_CONVERGED_FLAT_FILE_SOLD_LISTINGS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_MERCHANT_LISTINGS_DEFECT_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
            { "_GET_PAN_EU_OFFER_STATUS_",  MwsMarketplaceGroup.AmazonEurope() },
            { "_GET_MFN_PAN_EU_OFFER_STATUS_",  MwsMarketplaceGroup.AmazonEurope() },
            { "_GET_FLAT_FILE_GEO_OPPORTUNITIES_",  new List<MwsMarketplace> { MwsMarketplace.US } },

			// Order Reports
			{ "_GET_FLAT_FILE_ACTIONABLE_ORDER_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_ORDERS_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_FLAT_FILE_ORDER_REPORT_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },
			{ "_GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_",  MwsMarketplaceGroup.AmazonGlobal() },

			// Tax Reports
			{ "_SC_VAT_TAX_REPORT_",  MwsMarketplaceGroup.AmazonEurope() },
			{ "_GET_VAT_TRANSACTION_DATA_",  MwsMarketplaceGroup.AmazonEurope() },
			{ "_GET_GST_MTR_B2B_CUSTOM_",  new List<MwsMarketplace> { MwsMarketplace.India } },
			{ "_GET_GST_MTR_B2C_CUSTOM_",  new List<MwsMarketplace> { MwsMarketplace.India } },
            { "_GET_FLAT_FILE_SALES_TAX_DATA_",  null },

            { "_GET_EASYSHIP_DOCUMENTS_",  new List<MwsMarketplace> { MwsMarketplace.India } },

            { "_RFQD_BULK_DOWNLOAD_",  new List<MwsMarketplace> { MwsMarketplace.US, MwsMarketplace.UK, MwsMarketplace.Germany, MwsMarketplace.India, MwsMarketplace.Japan }  },
        };

		/// <summary>
		/// Gets the list of permitted marketplaces corresponding to the specified ReportType. If no result is found then this returns Null.<para/>
		/// </summary>
		/// <param name="reportType">Report type e.g. _GET_AMAZON_FULFILLED_SHIPMENTS_DATA_</param>
		/// <returns></returns>
		public static IEnumerable<MwsMarketplace> GetMarketplaces(string reportType) => _reportsMarketplaces.ContainsKey(reportType) ? _reportsMarketplaces[reportType] : null;
    }
}
