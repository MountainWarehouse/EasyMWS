using System;
using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Model
{
    public sealed class MwsMarketplace
    {
	    /// <summary>
	    /// The name of the current marketplace.
	    /// </summary>
	    public readonly string Name;

	    /// <summary>
	    /// Marketplace ID. This value has to be provided in request report request objects when requesting reports with the MWS client.
	    /// </summary>
	    public readonly string Id;

	    /// <summary>
	    /// The country code of the current marketplace.
	    /// </summary>
	    public readonly string CountryCode;

		/// <summary>
		/// The MWS endpoint that the marketplace uses.
		/// </summary>
	    public readonly MwsEndpoint MwsEndpoint;

	    private MwsMarketplace(string name, string countryCode, string id, MwsEndpoint mwsEndpoint)
		    => (Name, CountryCode, Id, MwsEndpoint) = (name, countryCode, id, mwsEndpoint);

	    public static List<string> operator +(MwsMarketplace mp1, MwsMarketplace mp2)
	    {
		    return new List<string> { mp1.Id, mp2.Id };
	    }

	    public static List<string> operator +(List<string> list, MwsMarketplace mp)
	    {
		    list.Add(mp.Id);
		    return list;
	    }

	    public static List<string> operator +(MwsMarketplace mp, List<string> list)
	    {
		    list.Add(mp.Id);
		    return list;
	    }

	    public static string GetMarketplaceCountryCode(string marketplaceId)
	    {
		    var mwsMarketplaces = new List<MwsMarketplace>{Australia, Brazil, Canada, China, France, Germany, India, Italy, Japan, Mexico, Spain, UK, US};
			var marketplace = mwsMarketplaces.FirstOrDefault(x => x.Id == marketplaceId);

		    if (marketplace == null)
			    throw new ArgumentException($"No marketplace was found for the requested marketplace id {marketplaceId}");

		    return marketplace.CountryCode;
		}

	    public static string GetMarketplaceCountryCodesAsCommaSeparatedString(IEnumerable<string> marketplaceIds)
	    {
		    return marketplaceIds.Select(GetMarketplaceCountryCode).Aggregate((c, n) => $"{c}, {n}");
		}

	    /// <summary>
		/// The Canada marketplace. Shares the same amazon MWS endpoint with US and Mexico.
		/// </summary>
		public static MwsMarketplace Canada = new MwsMarketplace(
			"Canada", "CA", "A2EUQ1WTGCTBG2", MwsEndpoint.NorthAmerica);

		/// <summary>
		/// The US marketplace. Shares the same amazon MWS endpoint with Canada and Mexico.
		/// </summary>
		public static MwsMarketplace US = new MwsMarketplace(
			"US", "US", "ATVPDKIKX0DER", MwsEndpoint.NorthAmerica);

		/// <summary>
		/// The Mexico marketplace. Shares the same amazon MWS endpoint with US and Canada.
		/// </summary>
		public static MwsMarketplace Mexico = new MwsMarketplace(
			"Mexico", "MX", "A1AM78C64UM0Y8", MwsEndpoint.NorthAmerica);

		/// <summary>
		/// The Spain marketplace. Shares the same amazon MWS endpoint with UK, France, Germany, and Italy marketplaces.
		/// </summary>
		public static MwsMarketplace Spain = new MwsMarketplace(
			"Spain", "ES", "A1RKKUPIHCS9HS", MwsEndpoint.Europe);

		/// <summary>
		///  The United Kingdom marketplace. Shares the same amazon MWS endpoint with Spain, France, Germany, and Italy marketplaces.
		/// </summary>
		public static MwsMarketplace UK = new MwsMarketplace(
			"UK", "UK", "A1F83G8C2ARO7P", MwsEndpoint.Europe);

		/// <summary>
		/// The France marketplace. Shares the same amazon MWS endpoint with UK, Spain, Germany, and Italy marketplaces.
		/// </summary>
		public static MwsMarketplace France = new MwsMarketplace(
			"France", "FR", "A13V1IB3VIYZZH", MwsEndpoint.Europe);

		/// <summary>
		/// The Germany marketplace. Shares the same amazon MWS endpoint with UK, France, Spain, and Italy marketplaces.
		/// </summary>
		public static MwsMarketplace Germany = new MwsMarketplace(
			"Germany", "DE", "A1PA6795UKMFR9", MwsEndpoint.Europe);

		/// <summary>
		/// The Italy marketplace. Shares the same amazon MWS endpoint with UK, France, Germany, and Spain marketplaces.
		/// </summary>
		public static MwsMarketplace Italy = new MwsMarketplace(
			"Italy", "IT", "APJ6JRA9NG5V4", MwsEndpoint.Europe);

		/// <summary>
		/// The Brazil marketplace. Does not share it's amazon MWS endpoint with any other marketplace. 
		/// </summary>
		public static MwsMarketplace Brazil = new MwsMarketplace(
			"Brazil", "BR", "A2Q3Y263D00KWC", MwsEndpoint.Brazil);

		/// <summary>
		/// The India marketplace. Does not share it's amazon MWS endpoint with any other marketplace. 
		/// </summary>
		public static MwsMarketplace India = new MwsMarketplace(
			"India", "IN", "A21TJRUUN4KGV", MwsEndpoint.India);

		/// <summary>
		/// The China marketplace. Does not share it's amazon MWS endpoint with any other marketplace. 
		/// </summary>
		public static MwsMarketplace China = new MwsMarketplace(
			"China", "CN", "AAHKV2X7AFYLW", MwsEndpoint.China);

		/// <summary>
		/// The Japan marketplace. Does not share it's amazon MWS endpoint with any other marketplace. 
		/// </summary>
		public static MwsMarketplace Japan = new MwsMarketplace(
			"Japan", "JP", "A1VC38T7YXB528", MwsEndpoint.Japan);

		/// <summary>
		/// The Australia marketplace. Does not share it's amazon MWS endpoint with any other marketplace. 
		/// </summary>
		public static MwsMarketplace Australia = new MwsMarketplace(
			"Australia", "AU", "A39IBJ37TRP1C6", MwsEndpoint.Australia);
	}
}
