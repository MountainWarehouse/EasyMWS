namespace MountainWarehouse.EasyMWS.Helpers
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

	    /// <summary>
	    /// The US marketplace. Shares the same amazon MWS endpoint with Canada and Mexico.
	    /// </summary>
	    public static MwsMarketplace US = new MwsMarketplace(
		    "US", "US", "ATVPDKIKX0DER", MwsEndpoint.NorthAmerica);

	    /// <summary>
	    /// The Canada marketplace. Shares the same amazon MWS endpoint with US and Mexico.
	    /// </summary>
	    public static MwsMarketplace Canada = new MwsMarketplace(
		    "Canada", "CA", "A2EUQ1WTGCTBG2", MwsEndpoint.NorthAmerica);

		/// <summary>
		///  The United Kingdom marketplace. Shares the same amazon MWS endpoint with Spain, France, Germany, and Italy marketplaces.
		/// </summary>
		public static MwsMarketplace UK = new MwsMarketplace(
		    "UK", "UK", "A1F83G8C2ARO7P", MwsEndpoint.Europe);
	}
}
