using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
	public interface ISettlementReportsFactory
    {
		/// <summary>
		/// Generate a request object for a MWS report of type : _GET_V2_SETTLEMENT_REPORT_DATA_XML_ or _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_ or _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_V2_ ,<br/>
		/// depending on the value provided for the 'reportId' parameter.
		/// <para/>
		/// _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_<br/>
		/// Tab-delimited flat file settlement report that is automatically scheduled by Amazon; it cannot be requested through RequestReport. For all sellers.
		/// <para/>
		/// _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_V2_<br/>
		/// Tab-delimited flat file alternate version of the Flat File Settlement Report that is automatically scheduled by Amazon; it cannot be requested through RequestReport.<br/>
		/// Price columns are condensed into three general purpose columns: amounttype, amountdescription, and amount. For Seller Central sellers only.
		/// <para/>
		/// _GET_V2_SETTLEMENT_REPORT_DATA_XML_<br/>
		/// XML file settlement report that is automatically scheduled by Amazon; it cannot be requested through RequestReport. For Seller Central sellers only.
		/// <para/>
		/// </summary>
		/// <param name="reportId">The ReportId of the settlement report to download.<br/>This id can be obtained by using IEasyMwsClient.ListSettlementReports or amazon scratchpad : https://mws.amazonservices.co.uk/scratchpad/index.html</param>
		/// <returns></returns>
		ReportRequestPropertiesContainer FlatFileSettlementReport(string reportId);
	}
}
