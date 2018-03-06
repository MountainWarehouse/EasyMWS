using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceWebService.Model;
using MountainWarehouse.EasyMWS.Helpers;

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

	    public ReportRequestWrapper GenerateRequestForReportGetAfnInventoryData(MwsMarketplaceGroup requestedMarketplaces = null)
		    => GenerateReportRequest("_GET_AFN_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonGlobal(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

	    public ReportRequestWrapper GenerateRequestForReportGetAfnInventoryDataByCountry(MwsMarketplaceGroup requestedMarketplaces = null)
		    => GenerateReportRequest("_GET_AFN_INVENTORY_DATA_BY_COUNTRY_", ContentUpdateFrequency.NearRealTime,
				permittedMarketplaces: MwsMarketplaceGroup.AmazonEurope(),
				requestedMarketplaces: requestedMarketplaces?.GetMarketplacesIdList.ToList());

	    public ReportRequestWrapper GenerateRequestForReportGetExcessInventoryData(MwsMarketplaceGroup requestedMarketplacesGroup = null)
		    => GenerateReportRequest("_GET_EXCESS_INVENTORY_DATA_", ContentUpdateFrequency.NearRealTime,
			    permittedMarketplaces: MwsMarketplace.US + MwsMarketplace.India + MwsMarketplace.Japan,
			    requestedMarketplaces: requestedMarketplacesGroup?.GetMarketplacesIdList.ToList());

		private ReportRequestWrapper GenerateReportRequest(string reportType, ContentUpdateFrequency reportUpdateFrequency, List<string> permittedMarketplaces, List<string> requestedMarketplaces = null)
	    {
		    ValidateMarketplaceCompatibility(reportType, permittedMarketplaces, requestedMarketplaces);
			var reportRequest = new RequestReportRequest
		    {
			    ReportType = reportType,
			    Merchant = _merchant,
			    MWSAuthToken = _mWsAuthToken,
			    MarketplaceIdList = requestedMarketplaces == null ? null : new IdList {Id = requestedMarketplaces}
		    };
		    return new ReportRequestWrapper(reportRequest, reportUpdateFrequency);
	    }

	    private void ValidateMarketplaceCompatibility(string reportType, List<string> permittedMarketplaces, List<string> requestedMarketplaces = null)
	    {
		    if (requestedMarketplaces == null) return;

		    foreach (var requestedMarketplace in requestedMarketplaces)
		    {
			    if (!permittedMarketplaces.Contains(requestedMarketplace))
			    {
				    throw new ArgumentException(
					    $@"The report request for type:'{reportType}', is only available to the following marketplaces:'{permittedMarketplaces.Aggregate((c, n) => $"{c}, {n}")}'.
The requested marketplace:'{requestedMarketplace}' is not supported by Amazon MWS for the specified report type.");
			    }
		    }
	    }
	}
}
