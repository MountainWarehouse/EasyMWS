using MountainWarehouse.EasyMWS.Model;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// Factory that can generate report requests for amazon MWS client.<para />
	/// At least one MarketplaceId value is required for Listings Reports. No MarketplaceId value is required for reports that are not Listings Reports. <para />
	/// When providing no MarketplaceId value for a reports that is not a Listings Reports, data for all marketplaces the seller is registered in will be shown.<para />
	/// For more info see : https://docs.developer.amazonservices.com/en_UK/reports/Reports_ReportType.html
	/// </summary>
	public interface IInventoryReportsFactory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_ALL_DATA_ <para />
		/// Tab-delimited flat file detailed all listings report. For Marketplace and Seller Central sellers. <para />
		/// This report accepts the following ReportOptions values: Custom<para/>
		/// </summary>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="custom">A Boolean value that indicates whether a custom report is returned. For more information, see Custom Inventory Reports. <para/>
		/// Default: false. This functionality is available only in the Canada, US, UK, and India marketplaces.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer AllListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_FLAT_FILE_OPEN_LISTINGS_DATA_ <para />
		/// Tab-delimited flat file open listings report that contains a summary of the seller's product listings with the price and quantity for each SKU. For Marketplace and Seller Central sellers.<para/>
		/// This report accepts the following ReportOptions values: Custom<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="custom">A Boolean value that indicates whether a custom report is returned. For more information, see Custom Inventory Reports.<para/>
		/// Default: false. This functionality is available only in the Canada, US, UK, and India marketplaces.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer InventoryReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_DATA_ <para />
		/// Tab-delimited flat file detailed active listings report. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer ActiveListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_INACTIVE_DATA_ <para />
		/// Tab-delimited flat file detailed active listings report. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer InactiveListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_DATA_BACK_COMPAT_ <para />
		/// Tab-delimited flat file open listings report.<para/>
		/// This report accepts the following ReportOptions values: Custom<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="custom">A Boolean value that indicates whether a custom report is returned. For more information, see Custom Inventory Reports.<para/>
		/// Default: false. This functionality is available only in the Canada, US, UK, and India marketplaces.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer OpenListingsReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_DATA_LITE_ <para />
		/// Tab-delimited flat file active listings report that contains only the SKU, ASIN, Price, and Quantity fields for items that have a quantity greater than zero. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer OpenListingsLiteReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_DATA_LITER_ <para />
		/// Tab-delimited flat file active listings report that contains only the SKU and Quantity fields for items that have a quantity greater than zero. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer OpenListingsLiterReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_CANCELLED_LISTINGS_DATA_ <para />
		/// Tab-delimited flat file canceled listings report. For Marketplace and Seller Central sellers.<para/>
		/// This report accepts the following ReportOptions values: Custom<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <param name="custom">A Boolean value that indicates whether a custom report is returned. For more information, see Custom Inventory Reports.<para/>
		/// Default: false. This functionality is available only in the Canada, US, UK, and India marketplaces.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer CanceledListingsReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_CONVERGED_FLAT_FILE_SOLD_LISTINGS_DATA_ <para />
		/// Tab-delimited flat file sold listings report that contains items sold on Amazon's retail website. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer SoldListingsReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_DEFECT_DATA_ <para />
		/// Tab-delimited flat file listing quality and suppressed listing report that contains your listing information that is incomplete or incorrect. For Marketplace and Seller Central sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer ListingQualityAndSuppressedListingReport(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_PAN_EU_OFFER_STATUS_ <para />
		/// Tab-delimited flat file report that contains enrollment status and eligibility information for the Pan-European FBA program for each of the seller's Amazon-fulfilled listings.<para/>
		/// This report is only available to FBA sellers in the Spain, UK, France, Germany, and Italy marketplaces. For more information, see Pan-European Eligibility in the Seller Central Help.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer PanEuropeanEligibilityFbaASINs(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MFN_PAN_EU_OFFER_STATUS_ <para />
		/// Tab-delimited flat file report that contains eligibility information for the Pan-European FBA Program for each of the seller's self-fulfilled listings.<para/>
		/// Self-fulfilled listings are not allowed in the Pan-European FBA program, and this report can help sellers determine whether to convert any of their self-fulfilled listings to Amazon-fulfilled listings in order to enroll them in the program.<para/>
		///  This report is only available in the Spain, UK, France, Germany, and Italy marketplaces. For more information, see Pan-European Eligibility in the Seller Central Help.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer PanEuropeanEligibilitySelfFulfilledASINs(DateTime? startDate = null, DateTime? endDate = null,
		    IEnumerable<MwsMarketplace> requestedMarketplaces = null);
	}
}
