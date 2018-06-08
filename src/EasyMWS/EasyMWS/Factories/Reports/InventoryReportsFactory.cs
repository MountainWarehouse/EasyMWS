using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class InventoryReportsFactory : IInventoryReportsFactory
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
		public ReportRequestPropertiesContainer AllListingsReport(MwsMarketplaceGroup requestedMarketplacesGroup = null, bool custom = false)
		{
			if (custom && requestedMarketplacesGroup != null)
			{
				var acceptedMarketplaceIdsForCustomOption = new List<string>
				{
					MwsMarketplace.Canada.Id,
					MwsMarketplace.US.Id,
					MwsMarketplace.UK.Id,
					MwsMarketplace.India.Id
				};
				var nonAcceptedMarketplaceIds = requestedMarketplacesGroup.GetMarketplacesIdList.Except(acceptedMarketplaceIdsForCustomOption).ToList();

				if (nonAcceptedMarketplaceIds.Any())
				{
					throw new ArgumentException(
						$"The 'Custom' option is not available for the following marketplace(s) : {MwsMarketplace.GetMarketplaceCountryCodesAsCommaSeparatedString(nonAcceptedMarketplaceIds)}");
				}
			}

			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("custom", custom);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_ALL_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(), 
				requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList(), 
				reportOptions: reportOptions);
		}


	}
}
