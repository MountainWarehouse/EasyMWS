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

			Assert.AreEqual("NorthAmerica", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws.amazonservices.com", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Brazil_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Brazil;

			Assert.AreEqual("Brazil", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws.amazonservices.com", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Europe_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Europe;

			Assert.AreEqual("Europe", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws-eu.amazonservices.com", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void India_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.India;

			Assert.AreEqual("India", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws.amazonservices.in", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void China_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.China;

			Assert.AreEqual("China", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws.amazonservices.com.cn", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Japan_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Japan;

			Assert.AreEqual("Japan", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws.amazonservices.jp", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void Australia_IsInitializedWithExpectedData()
		{
			var mwsMarketplaceAccessEndpoint = MwsEndpoint.Australia;

			Assert.AreEqual("Australia", mwsMarketplaceAccessEndpoint.Name);
			Assert.AreEqual("https://mws.amazonservices.com.au", mwsMarketplaceAccessEndpoint.RegionOrMarketPlaceEndpoint);
		}
	}
}
