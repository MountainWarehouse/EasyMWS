using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class ReportRequestFactoryInventory : IReportRequestFactoryInventory
	{
		public ReportRequestPropertiesContainer AllListingsReport(DateTime? startDate = null, DateTime? endDate = null, 
			MwsMarketplaceGroup requestedMarketplacesGroup = null, bool custom = false)
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
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}


	}
}
