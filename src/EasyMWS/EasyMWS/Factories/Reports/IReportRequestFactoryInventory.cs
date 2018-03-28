using System;
using MountainWarehouse.EasyMWS.Helpers;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface IReportRequestFactoryInventory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_MERCHANT_LISTINGS_ALL_DATA_ <para />
		/// Tab-delimited flat file detailed all listings report. For Marketplace and Seller Central sellers. <para />
		/// This report accepts the following ReportOptions values: Custom<para/>
		/// </summary>
		/// <param name="requestedMarketplacesGroup">Optional group of marketplaces used when submitting a report request. For more info see class summary.</param>
		/// <param name="custom">A Boolean value that indicates whether a custom report is returned. For more information, see Custom Inventory Reports. <para/>
		/// Default: false. This functionality is available only in the Canada, US, UK, and India marketplaces.</param>
		/// <returns></returns>
		[Obsolete("Some of the parameters for this report may be missing. Report request not verified yet.")]
	    ReportRequestPropertiesContainer AllListingsReport(
			MwsMarketplaceGroup requestedMarketplacesGroup = null, bool custom = false);
	}
}
