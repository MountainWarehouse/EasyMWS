namespace MountainWarehouse.EasyMWS.Helpers
{
	public sealed class MwsEndpoint
	{
		private MwsEndpoint(string regionOrMarketPlace, AmazonRegion region) =>
			(RegionOrMarketPlaceEndpoint, Region) = (regionOrMarketPlace, region);

		/// <summary>
		/// The MWS endpoint for a marketplace or a group of marketplaces.
		/// </summary>
		public readonly string RegionOrMarketPlaceEndpoint;

		/// <summary>
		/// The name of the MWS endpoint.
		/// </summary>
		public readonly AmazonRegion Region;

		public static MwsEndpoint NorthAmerica =
			new MwsEndpoint("https://mws.amazonservices.com", AmazonRegion.NorthAmerica);

		public static MwsEndpoint Brazil =
			new MwsEndpoint("https://mws.amazonservices.com", AmazonRegion.Brazil);

		public static MwsEndpoint Europe =
			new MwsEndpoint("https://mws-eu.amazonservices.com", AmazonRegion.Europe);

		public static MwsEndpoint India =
			new MwsEndpoint("https://mws.amazonservices.in", AmazonRegion.India);

		public static MwsEndpoint China =
			new MwsEndpoint("https://mws.amazonservices.com.cn", AmazonRegion.China);

		public static MwsEndpoint Japan =
			new MwsEndpoint("https://mws.amazonservices.jp", AmazonRegion.Japan);

		public static MwsEndpoint Australia =
			new MwsEndpoint("https://mws.amazonservices.com.au", AmazonRegion.Australia);
	}
}
