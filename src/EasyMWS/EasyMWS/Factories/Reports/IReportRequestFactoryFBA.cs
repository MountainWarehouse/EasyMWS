using System.Collections.Generic;
using MarketplaceWebService.Model;
using MountainWarehouse.EasyMWS.Helpers;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	/// <summary>
	/// Factory that can generate report requests for amazon MWS client.<para />
	/// At least one MarketplaceId value is required for Listings Reports. No MarketplaceId value is required for reports that are not Listings Reports. <para />
	/// When providing no MarketplaceId value for a reports that is not a Listings Reports, data for all marketplaces the seller is registered in will be shown.
	/// </summary>
	public interface IReportRequestFactoryFba
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AFN_INVENTORY_DATA_ <para />
		/// Tab-delimited flat file. Content updated in near real-time. For FBA sellers only. <para />
		/// For Marketplace and Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">Optional group of marketplaces used when submitting a report request.</param>
		/// <returns></returns>
		ReportRequestWrapper GenerateRequestForReportGetAfnInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_AFN_INVENTORY_DATA_BY_COUNTRY_ <para />
		/// Tab-delimited flat file. Contains quantity available for local fulfillment by country, helping Multi-Country Inventory sellers in Europe track their FBA inventory. <para />
		/// Content updated in near-real time. This report is only available to FBA sellers in European (EU) marketplaces. <para />
		/// For Seller Central sellers.
		/// </summary>
		/// <param name="requestedMarketplacesGroup">Optional group of marketplaces used when submitting a report request.</param>
		/// <returns></returns>
		ReportRequestWrapper GenerateRequestForReportGetAfnInventoryDataByCountry(MwsMarketplaceGroup requestedMarketplacesGroup = null);

	    /// <summary>
	    /// Generate a request object for a MWS report of type : _GET_EXCESS_INVENTORY_DATA_ <para />
	    /// Tab-delimited flat file. Contains listings with excess inventory, which helps sellers take action to sell through faster. <para />
	    /// Content updated in near real-time. This report is only available to FBA sellers in the US, India, and Japan marketplaces. <para />
	    /// For more information, see Excess Inventory Report.
	    /// </summary>
	    /// <param name="requestedMarketplacesGroup">Optional list of marketplaces used when submitting a report request. For more info see class summary.</param>
	    /// <returns></returns>
		ReportRequestWrapper GenerateRequestForReportGetExcessInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null);
	}
}
