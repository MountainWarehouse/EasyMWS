namespace MountainWarehouse.EasyMWS.Helpers
{
	public sealed class MwsEndpoint
	{
		private MwsEndpoint(string regionOrMarketPlace, string name) =>
			(RegionOrMarketPlaceEndpoint, Name) = (regionOrMarketPlace, name);

		/// <summary>
		/// The MWS endpoint for a marketplace or a group of marketplaces.
		/// </summary>
		public readonly string RegionOrMarketPlaceEndpoint;

		/// <summary>
		/// The name of the MWS endpoint.
		/// </summary>
		public readonly string Name;

		public static MwsEndpoint NorthAmerica =
			new MwsEndpoint("https://mws.amazonservices.com", "NorthAmerica");

		public static MwsEndpoint Brazil =
			new MwsEndpoint("https://mws.amazonservices.com", "Brazil");

		public static MwsEndpoint Europe =
			new MwsEndpoint("https://mws-eu.amazonservices.com", "Europe");

		public static MwsEndpoint India =
			new MwsEndpoint("https://mws.amazonservices.in", "India");

		public static MwsEndpoint China =
			new MwsEndpoint("https://mws.amazonservices.com.cn", "China");

		public static MwsEndpoint Japan =
			new MwsEndpoint("https://mws.amazonservices.jp", "Japan");

		public static MwsEndpoint Australia =
			new MwsEndpoint("https://mws.amazonservices.com.au", "Australia");
	}
}
