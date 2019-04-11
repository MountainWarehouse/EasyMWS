using MountainWarehouse.EasyMWS.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public interface ISettlementReportsFactory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_ <para />
		/// Tab-delimited flat file settlement report that is automatically scheduled by Amazon; it cannot be requested through RequestReport. For all sellers.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileSettlementReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null);

		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_V2_SETTLEMENT_REPORT_DATA_XML_ <para />
		/// XML file settlement report that is automatically scheduled by Amazon; it cannot be requested through RequestReport. For Seller Central sellers only.<para/>
		/// </summary>
		/// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
		/// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer XmlSettlementReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null);

        /// <summary>
        /// Generate a request object for a MWS report of type : _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_V2_ <para />
        /// Tab-delimited flat file alternate version of the Flat File Settlement Report that is automatically scheduled by Amazon; it cannot be requested through RequestReport.<para/>
        /// Price columns are condensed into three general purpose columns: amounttype, amountdescription, and amount. For Seller Central sellers only.<para/>
        /// </summary>
        /// <param name="startDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
        /// <param name="endDate">Optional argument that can help specify an interval of time for which the report is generated.</param>
        /// <param name="requestedMarketplaces">(NA, EU only) Optional group of marketplaces used when submitting a report request. For more info see MwsMarketplaceGroup class summary.</param>
        /// <returns></returns>
        ReportRequestPropertiesContainer FlatFileV2SettlementReport(DateTime? startDate = null, DateTime? endDate = null,
			IEnumerable<MwsMarketplace> requestedMarketplaces = null);

	}
}
