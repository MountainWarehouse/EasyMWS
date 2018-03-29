using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Model
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

		internal static MwsEndpoint NorthAmerica =
			new MwsEndpoint("https://mws.amazonservices.com", AmazonRegion.NorthAmerica);

		internal static MwsEndpoint Brazil =
			new MwsEndpoint("https://mws.amazonservices.com", AmazonRegion.Brazil);

		internal static MwsEndpoint Europe =
			new MwsEndpoint("https://mws-eu.amazonservices.com", AmazonRegion.Europe);

		internal static MwsEndpoint India =
			new MwsEndpoint("https://mws.amazonservices.in", AmazonRegion.India);

		internal static MwsEndpoint China =
			new MwsEndpoint("https://mws.amazonservices.com.cn", AmazonRegion.China);

		internal static MwsEndpoint Japan =
			new MwsEndpoint("https://mws.amazonservices.jp", AmazonRegion.Japan);

		internal static MwsEndpoint Australia =
			new MwsEndpoint("https://mws.amazonservices.com.au", AmazonRegion.Australia);
	}
}
