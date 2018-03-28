using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public class ReportRequestFactoryInventory : IReportRequestFactoryInventory
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

			return GenerateReportRequest("_GET_MERCHANT_LISTINGS_ALL_DATA_", ContentUpdateFrequency.Unknown,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(), 
				requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList(), 
				reportOptions: reportOptions);
		}

		private ReportRequestPropertiesContainer GenerateReportRequest(string reportType, ContentUpdateFrequency reportUpdateFrequency,
			List<string> permittedMarketplaces, List<string> requestedMarketplaces = null, DateTime? startDate = null, DateTime? endDate = null, ReportOptions reportOptions = null)
		{
			ValidateMarketplaceCompatibility(reportType, permittedMarketplaces, requestedMarketplaces);
			return new ReportRequestPropertiesContainer(reportType, reportUpdateFrequency, requestedMarketplaces, startDate, endDate, reportOptions?.GetOptionsString());
		}

		private void ValidateMarketplaceCompatibility(string reportType, List<string> permittedMarketplaces,
			List<string> requestedMarketplaces = null)
		{
			if (requestedMarketplaces == null) return;

			foreach (var requestedMarketplace in requestedMarketplaces)
			{
				if (!permittedMarketplaces.Contains(requestedMarketplace))
				{
					throw new ArgumentException(
						$@"The report request for type:'{reportType}', is only available to the following marketplaces:'{
								MwsMarketplace.GetMarketplaceCountryCodesAsCommaSeparatedString(permittedMarketplaces)
							}'.
The requested marketplace:'{MwsMarketplace.GetMarketplaceCountryCode(requestedMarketplace)}' is not supported by Amazon MWS for the specified report type.");
				}
			}
		}
	}
}
