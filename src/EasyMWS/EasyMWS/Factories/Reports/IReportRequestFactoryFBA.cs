using System;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// Factory that can generate report requests for amazon MWS client.<para />
	/// At least one MarketplaceId value is required for Listings Reports. No MarketplaceId value is required for reports that are not Listings Reports. <para />
	/// When providing no MarketplaceId value for a reports that is not a Listings Reports, data for all marketplaces the seller is registered in will be shown.
	/// </summary>
	public interface IReportRequestFactoryFba
	{
		#region FBA Inventory Reports

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AFN_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer GenerateRequestForReportGetAfnInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AFN_INVENTORY_DATA_BY_COUNTRY_ <para />
		/// Tab-delimited flat file. Contains quantity available for local fulfillment by country, helping Multi-Country Inventory sellers in Europe track their FBA inventory. <para />
		/// Content updated in near-real time. This report is only available to FBA sellers in European (EU) marketplaces. <para />
		/// For Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>a
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetAfnInventoryDataByCountry(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_EXCESS_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains listings with excess inventory, which helps sellers take action to sell through faster. <para />
		/// Content updated in near real-time. This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Excess Inventory Report.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetExcessInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_ <para />
		/// Tab delimited flat file. Contains historical data of shipments that crossed country borders. <para />
		/// For export shipments or for shipments using Amazon's European Fulfillment Network (note that Amazon's European Fulfillment Network is for Seller Central sellers only). <para />
		/// Content updated daily. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFulfillmentCrossBorderInventoryMovementData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains historical daily snapshots of your available inventory in Amazon’s fulfillment centers including quantity, location and disposition. <para />
		/// Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentCurrentInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_ <para />
		/// Tab-delimited flat file. Contains inbound shipment problems by product and shipment.<para />
		/// Content updated daily. For Marketplace and Seller Central sellers. <para />
		/// This report is only available to FBA sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInboundNoncomplianceData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_ <para />
		/// Tab-delimited flat file. Contains corrections and updates to your inventory in response to issues such as damage, loss, receiving discrepancies, etc. Content updated daily. <para />
		/// For FBA sellers only. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryAdjustmentsData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_ <para />
		/// Tab-delimited flat file. Contains information about inventory age, condition, sales volume, weeks of cover, and price. Content updated daily. For FBA Sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryHealthData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_ <para />
		/// Tab-delimited flat file. Contains inventory that has completed the receive process at Amazon’s fulfillment centers. <para />
		/// Content updated daily. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventoryReceiptsData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_ <para />
		/// Tab-delimited flat file. Contains history of inventory events (e.g. receipts, shipments, adjustments etc.) by SKU and Fulfillment Center. <para />
		/// Content updated daily. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentInventorySummaryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains historical monthly snapshots of your available inventory in Amazon’s fulfillment centers including average and end-of-month quantity, location and disposition. <para />
		/// Content updated daily. For FBA sellers only. For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaFulfillmentMonthlyInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_INVENTORY_AGED_DATA_ <para />
		/// Tab-delimited flat file. Indicates the age of inventory, which helps sellers take action to avoid paying the Long Term Storage Fee. <para />
		/// Content updated daily. This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Inventory Age Report.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaInventoryAgedData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_MYI_ALL_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains current details of all (including archived) inventory including condition, quantity and volume. <para />
		/// Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaMyiAllInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Contains current details of active (not archived) inventory including condition, quantity and volume.<para />
		///  Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer GenerateRequestForReportGetFbaMyiUnsuppressedInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_RESERVED_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Provides data about the number of reserved units in your inventory. <para />
		/// Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetReservedInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_RESTOCK_INVENTORY_RECOMMENDATIONS_REPORT_ <para />
		/// Tab delimited flat file. Provides recommendations on products to restock, suggested order quantities, and reorder dates. <para />
		/// For more information, see Restock Inventory Report. Content updated in near real-time. <para />
		/// This report is only available to FBA sellers in the US marketplace.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetRestockInventoryRecommendationsReport(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_STRANDED_INVENTORY_LOADER_DATA_ <para />
		/// Tab-delimited flat file. Contains a list of stranded inventory. <para />
		/// Update the listing information in this report file and then submit the file using Content updated in near real-time. <para />
		/// This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Bulk Fix Stranded Inventory Report.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetStrandedInventoryLoaderData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_STRANDED_INVENTORY_UI_DATA_ <para />
		/// Tab-delimited flat file. Contains a breakdown of inventory in stranded status, including recommended actions. Content updated in near real-time. <para />
		/// This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
		/// For more information, see Stranded Inventory Report.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
		ReportRequestPropertiesContainer GenerateRequestForReportGetStrandedInventoryUiData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		#endregion

		#region FBA Payment Reports

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_ <para />
		/// Tab-delimited flat file. Contains the estimated Amazon Selling and Fulfillment Fees for your FBA inventory with active offers. <para />
		/// The content is updated at least once every 72 hours. <para />
		/// To successfully generate a report, specify the StartDate parameter of a minimum 72 hours prior to NOW and EndDate to NOW.<para />
		/// For FBA sellers in the NA and EU only. For Marketplace and Seller Central sellers.						<para />																					
		/// </summary>
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer GenerateRequestForReportFbaFeePreviewReport(DateTime startDate, DateTime? endDate = null, MwsMarketplaceGroup requestedMarketplacesGroup = null);

		#endregion

	
	}
}
