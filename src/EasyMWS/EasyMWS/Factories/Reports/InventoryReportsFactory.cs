using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	// When adding support for a new report type, ReportsPermittedMarketplacesMapper map also has to be updated to include the permitted marketplaces for that report.
	public class InventoryReportsFactory : IInventoryReportsFactory
	{
        private ReportOptions PopulateReportOptionsWithCustomValue(ReportOptions reportOptions, bool custom = false, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
        {
            if (custom && requestedMarketplaces != null)
            {
                var acceptedMarketplaceIdsForCustomOption = new List<string>
                {
                    MwsMarketplace.Canada.Id,
                    MwsMarketplace.US.Id,
                    MwsMarketplace.UK.Id,
                    MwsMarketplace.India.Id
                };
                var nonAcceptedMarketplaceIds = requestedMarketplaces.Select(m => m.Id).Except(acceptedMarketplaceIdsForCustomOption).ToList();

                if (nonAcceptedMarketplaceIds.Any())
                {
                    throw new ArgumentException(
                        $"The 'Custom' option is not available for the following marketplace(s) : {MwsMarketplace.GetMarketplaceCountryCodesAsCommaSeparatedString(nonAcceptedMarketplaceIds)}");
                }

                if(reportOptions == null) reportOptions = new ReportOptions();
                reportOptions.AddBooleanOption("custom", custom);
            }

            return reportOptions;
        }

        public ReportRequestPropertiesContainer AllListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false)
		{
            var reportOptions = PopulateReportOptionsWithCustomValue(null, custom, requestedMarketplaces);

            return ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_ALL_DATA_", ContentUpdateFrequency.Unknown,
				requestedMarketplaces: requestedMarketplaces, 
				reportOptions: reportOptions, startDate: startDate, endDate: endDate);
		}

		public ReportRequestPropertiesContainer InventoryReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false)
		{
            var reportOptions = PopulateReportOptionsWithCustomValue(null, custom, requestedMarketplaces);

            return ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_OPEN_LISTINGS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces,
                reportOptions: reportOptions, startDate: startDate, endDate: endDate);
        }

		public ReportRequestPropertiesContainer ActiveListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);


		public ReportRequestPropertiesContainer InactiveListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_INACTIVE_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer OpenListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false)
		{
            var reportOptions = PopulateReportOptionsWithCustomValue(null, custom, requestedMarketplaces);

            return ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_DATA_BACK_COMPAT_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces,
                reportOptions: reportOptions, startDate: startDate, endDate: endDate);
        }

		public ReportRequestPropertiesContainer OpenListingsLiteReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_DATA_LITE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer OpenListingsLiterReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_DATA_LITER_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer CanceledListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null, bool custom = false)
		{
            var reportOptions = PopulateReportOptionsWithCustomValue(null, custom, requestedMarketplaces);

            return ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_CANCELLED_LISTINGS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces,
                reportOptions: reportOptions, startDate: startDate, endDate: endDate);
        }

		public ReportRequestPropertiesContainer SoldListingsReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_CONVERGED_FLAT_FILE_SOLD_LISTINGS_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer ListingQualityAndSuppressedListingReport(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_MERCHANT_LISTINGS_DEFECT_DATA_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer PanEuropeanEligibilityFbaASINs(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_PAN_EU_OFFER_STATUS_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

		public ReportRequestPropertiesContainer PanEuropeanEligibilitySelfFulfilledASINs(DateTime? startDate = null,
			DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_MFN_PAN_EU_OFFER_STATUS_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer GlobalExpansionOpportunitiesReport(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_GEO_OPPORTUNITIES_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);
    }
}
