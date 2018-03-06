namespace MountainWarehouse.EasyMWS.Helpers
{
    public class MwsMarketplace
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
	    public readonly string MwsEndpoint;

	    private MwsMarketplace(string name, string countryCode, string id, string mwsEndpoint)
		    => (Name, CountryCode, Id, MwsEndpoint) = (name, countryCode, id, mwsEndpoint);

	    /// <summary>
	    /// The US marketplace. Shares the same amazon MWS endpoint with Canada and Mexico.
	    /// </summary>
	    public static MwsMarketplace US = new MwsMarketplace(
		    "US", "US", "ATVPDKIKX0DER", "https://mws.amazonservices.com");
	}
}
