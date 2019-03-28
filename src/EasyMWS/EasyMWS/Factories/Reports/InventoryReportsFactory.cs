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
	public class InventoryReportsFactory : IInventoryReportsFactory
	{
		public ReportRequestPropertiesContainer AllListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null, bool custom = false)
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
				var nonAcceptedMarketplaceIds = requestedMarketplacesGroup.Except(acceptedMarketplaceIdsForCustomOption).ToList();

				if (nonAcceptedMarketplaceIds.Any())
				{
					throw new ArgumentException(
						$"The 'Custom' option is not available for the following marketplace(s) : {MwsMarketplace.GetMarketplaceCountryCodesAsCommaSeparatedString(nonAcceptedMarketplaceIds)}");
				}
			}

			var reportOptions = new ReportOptions();
			reportOptions.AddBooleanOption("custom", custom);

			return ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_ALL_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplacesGroup, 
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}

		public ReportRequestPropertiesContainer InventoryReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null, bool custom = false)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer ActiveListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer InactiveListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer OpenListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null, bool custom = false)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer OpenListingsLiteReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer OpenListingsLiterReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer CanceledListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null, bool custom = false)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer SoldListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer ListingQualityAndSuppressedListingReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer PanEuropeanEligibilityFbaASINs(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}

		public ReportRequestPropertiesContainer PanEuropeanEligibilitySelfFulfilledASINs(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<string> requestedMarketplacesGroup = null)
		{
			throw new NotImplementedException();
		}
	}
}
