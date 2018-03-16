using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class MwsEndpointTests
    {
		[Test]
		public void NorthAmerica_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.NorthAmerica;

			Assert.AreEqual(AmazonRegion.NorthAmerica, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws.amazonservices.com", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Brazil_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Brazil;

			Assert.AreEqual(AmazonRegion.Brazil, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws.amazonservices.com", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Europe_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Europe;

			Assert.AreEqual(AmazonRegion.Europe, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws-eu.amazonservices.com", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void India_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.India;

			Assert.AreEqual(AmazonRegion.India, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws.amazonservices.in", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void China_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.China;

			Assert.AreEqual(AmazonRegion.China, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws.amazonservices.com.cn", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Japan_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Japan;

			Assert.AreEqual(AmazonRegion.Japan, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws.amazonservices.jp", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Australia_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Australia;

			Assert.AreEqual(AmazonRegion.Australia, mwsMarketplaceAccessEndpoint.Region);
			Assert.AreEqual("https://mws.amazonservices.com.au", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}
	}
}
