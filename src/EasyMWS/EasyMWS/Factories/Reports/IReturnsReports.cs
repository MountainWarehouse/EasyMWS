using MountainWarehouse.EasyMWS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface IReturnsReports
    {
        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_XML_RETURNS_DATA_BY_RETURN_DATE_<para />
        /// XML report that can be requested or scheduled. Contains detailed returns information, including return request date, RMA ID, label details, ASIN, and return reason code. You can request up to 60 days of data in a single report.
        /// </summary>
        /// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
        /// <returns></returns>
        ReportRequestPropertiesContainer XMLReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_FLAT_FILE_RETURNS_DATA_BY_RETURN_DATE_<para />
        /// Tab-delimited flat file report that can be requested or scheduled. Contains detailed returns information, including return request date, RMA ID, label details, ASIN, and return reason code. You can request up to 60 days of data in a single report.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer FlatFileReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_XML_MFN_PRIME_RETURNS_REPORT_<para />
        /// XML report that can be requested or scheduled. Contains detailed Seller Fulfilled Prime returns information, including return request date, RMA ID, label details, ASIN, and return reason code. You can request up to 60 days of data in a single report.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer XMLPrimeReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_CSV_MFN_PRIME_RETURNS_REPORT_<para />
        /// XML report that can be requested or scheduled. Contains detailed Seller Fulfilled Prime returns information, including return request date, RMA ID, label details, ASIN, and return reason code. You can request up to 60 days of data in a single report.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer CSVPrimeReturnsReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_XML_MFN_SKU_RETURN_ATTRIBUTES_REPORT_<para />
        /// XML report that can be requested or scheduled. Contains detailed return attribute information by SKU, including prepaid label eligibility and returnless refund eligibility. You can request up to 60 days of data in a single report.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer XMLReturnAttributesReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_FLAT_FILE_MFN_SKU_RETURN_ATTRIBUTES_REPORT_<para />
        /// XML report that can be requested or scheduled. Contains detailed Seller Fulfilled Prime returns information, including return request date, RMA ID, label details, ASIN, and return reason code. You can request up to 60 days of data in a single report.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="requestedMarketplaces"></param>
        /// <returns></returns>
        ReportRequestPropertiesContainer FlatFileReturnAttributesReportByReturnDate(DateTime? startDate = null, DateTime? endDate = null, IEnumerable<MwsMarketplace> requestedMarketplaces = null);
    }
}
