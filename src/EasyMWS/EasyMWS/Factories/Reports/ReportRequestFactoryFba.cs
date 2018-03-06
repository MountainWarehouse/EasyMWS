using System.Collections.Generic;
using MarketplaceWebService.Model;

namespace MountainWarehouse.EasyMWS.Factories.Reports
{
    public class ReportRequestFactoryFba : IReportRequestFactoryFba
    {
	    private readonly string _merchant;
	    private readonly string _mWsAuthToken;

		/// <summary>
		/// Creates an instance of the Report Request Factory for amazon FBA reports.
		/// </summary>
		/// <param name="merchant">Optional parameter. MerchantId / SellerId</param>
		/// <param name="mWsAuthToken">MWS request authentication token</param>
		public ReportRequestFactoryFba(string merchant = null, string mWsAuthToken = null)
		    => (_merchant, _mWsAuthToken)
			    = (merchant, mWsAuthToken);

	    public RequestReportRequest GenerateRequestForReportGetAfnInventoryData(List<string> marketplaceIdList = null)
		    => GenerateReportRequest("_GET_AFN_INVENTORY_DATA_", marketplaceIdList);

	    public RequestReportRequest GenerateRequestForReportGetAfnInventoryDataByCountry(List<string> marketplaceIdList = null)
		    => GenerateReportRequest("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_", marketplaceIdList);

	    private RequestReportRequest GenerateReportRequest(string reportType, List<string> requestedMarketplaces = null)
	    {
		    var reportRequest = new RequestReportRequest
		    {
			    ReportType = reportType,
			    Merchant = _merchant,
			    MWSAuthToken = _mWsAuthToken,
			    MarketplaceIdList = requestedMarketplaces == null ? null : new IdList {Id = requestedMarketplaces}
		    };
		    return reportRequest;
	    }
    }
}
