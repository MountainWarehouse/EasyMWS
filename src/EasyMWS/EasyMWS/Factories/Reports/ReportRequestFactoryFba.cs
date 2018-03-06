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
	    {
		    return new RequestReportRequest
		    {
			    ReportType = "_GET_AFN_INVENTORY_DATA_",
			    Merchant = _merchant,
			    MWSAuthToken = _mWsAuthToken,
			    MarketplaceIdList = marketplaceIdList == null ? null : new IdList {Id = marketplaceIdList}
		    };
	    }

	    public RequestReportRequest GenerateRequestForReportGetAfnInventoryDataByCountry(List<string> marketplaceIdList = null)
	    {
		    return new RequestReportRequest
		    {
			    ReportType = "_GET_AFN_INVENTORY_DATA_BY_COUNTRY_",
			    Merchant = _merchant,
			    MWSAuthToken = _mWsAuthToken,
			    MarketplaceIdList = marketplaceIdList == null ? null : new IdList { Id = marketplaceIdList }
		    };
		}
    }
}
