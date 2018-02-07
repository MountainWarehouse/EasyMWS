using System;
using MarketplaceWebService;

namespace MountainWarehouse.EasyMWS
{
	/// <summary>Client for Amazon Marketplace Web Services</summary>
    public class EasyMwsClient
    {
	    private IMarketplaceWebServiceClient _mwsClient;

		/// <param name="region">The region of the account</param>
		/// <param name="accessKeyId">Your specific access key</param>
		/// <param name="mwsSecretAccessKey">Your specific secret access key</param>
		public EasyMwsClient(AmazonRegion region, string accessKeyId, string mwsSecretAccessKey)
	    {
		    _mwsClient = new MarketplaceWebServiceClient(accessKeyId, mwsSecretAccessKey, CreateConfig(region));
		}

		#region Helpers for creating the MarketplaceWebServiceClient

		private MarketplaceWebServiceConfig CreateConfig(AmazonRegion region)
	    {
		    string rootUrl;
		    switch (region)
		    {
				case AmazonRegion.Australia: rootUrl = "https://mws.amazonservices.com.au"; break;
				case AmazonRegion.China: rootUrl = "https://mws.amazonservices.com.cn"; break;
			    case AmazonRegion.Europe: rootUrl = "https://mws-eu.amazonservices.com"; break;
			    case AmazonRegion.India: rootUrl = "https://mws.amazonservices.in"; break;
			    case AmazonRegion.Japan: rootUrl = "https://mws.amazonservices.jp"; break;
			    case AmazonRegion.NorthAmerica: rootUrl = "https://mws.amazonservices.com"; break;
				default:
					throw new ArgumentException($"{region} is unknown - EasyMWS doesn't know the RootURL");
			}

		    var config = new MarketplaceWebServiceConfig
		    {
			    ServiceURL = rootUrl
		    };
		    config = config.WithUserAgent("EasyMWS");

		    return config;
	    }

		#endregion

	}
}
