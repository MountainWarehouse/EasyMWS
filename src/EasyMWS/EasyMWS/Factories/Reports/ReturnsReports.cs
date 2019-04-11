using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class ReturnsReports : IReturnsReports
    {
        public ReportRequestPropertiesContainer CSVPrimeReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
            => ReportGeneratorHelper.GenerateReportRequest("_GET_CSV_MFN_PRIME_RETURNS_REPORT_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer FlatFileReturnAttributesReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
        => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_MFN_SKU_RETURN_ATTRIBUTES_REPORT_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer FlatFileReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
    => ReportGeneratorHelper.GenerateReportRequest("_GET_FLAT_FILE_RETURNS_DATA_BY_RETURN_DATE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer XMLPrimeReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
    => ReportGeneratorHelper.GenerateReportRequest("_GET_XML_MFN_PRIME_RETURNS_REPORT_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer XMLReturnAttributesReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
    => ReportGeneratorHelper.GenerateReportRequest("_GET_XML_MFN_SKU_RETURN_ATTRIBUTES_REPORT_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);

        public ReportRequestPropertiesContainer XMLReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null)
    => ReportGeneratorHelper.GenerateReportRequest("_GET_XML_RETURNS_DATA_BY_RETURN_DATE_", ContentUpdateFrequency.Unknown,
                requestedMarketplaces: requestedMarketplaces, startDate: startDate, endDate: endDate);
    }
}
