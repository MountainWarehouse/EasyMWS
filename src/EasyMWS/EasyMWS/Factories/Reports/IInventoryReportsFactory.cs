using MountainWarehouse.EasyMWS.Model;

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
		/// <param name="requestedMarketplacesGroup">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <param name="custom">A Boolean value that indicates whether a custom report is returned. For more information, see Custom Inventory Reports. <para/>
		/// Default: false. This functionality is available only in the Canada, US, UK, and India marketplaces.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer AllListingsReport(
			MwsMarketplaceGroup requestedMarketplacesGroup = null, bool custom = false);
	}
}
