using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class MwsMarketplaceTests
    {
		[Test]
		public void Canada_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Canada;

			Assert.AreEqual("Canada", marketplace.Name);
			Assert.AreEqual("CA", marketplace.CountryCode);
			Assert.AreEqual("A2EUQ1WTGCTBG2", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.NorthAmerica, marketplace.MwsEndpoint);
		}

		[Test]
		public void US_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.US;

			Assert.AreEqual("US", marketplace.Name);
			Assert.AreEqual("US", marketplace.CountryCode);
			Assert.AreEqual("ATVPDKIKX0DER", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.NorthAmerica, marketplace.MwsEndpoint);
		}

		[Test]
		public void Mexico_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Mexico;

			Assert.AreEqual("Mexico", marketplace.Name);
			Assert.AreEqual("MX", marketplace.CountryCode);
			Assert.AreEqual("A1AM78C64UM0Y8", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.NorthAmerica, marketplace.MwsEndpoint);
		}

		[Test]
		public void Spain_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Spain;

			Assert.AreEqual("Spain", marketplace.Name);
			Assert.AreEqual("ES", marketplace.CountryCode);
			Assert.AreEqual("A1RKKUPIHCS9HS", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Europe, marketplace.MwsEndpoint);
		}

		[Test]
		public void UK_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.UK;

			Assert.AreEqual("UK", marketplace.Name);
			Assert.AreEqual("UK", marketplace.CountryCode);
			Assert.AreEqual("A1F83G8C2ARO7P", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Europe, marketplace.MwsEndpoint);
		}

		[Test]
		public void France_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.France;

			Assert.AreEqual("France", marketplace.Name);
			Assert.AreEqual("FR", marketplace.CountryCode);
			Assert.AreEqual("A13V1IB3VIYZZH", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Europe, marketplace.MwsEndpoint);
		}

		[Test]
		public void Germany_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Germany;

			Assert.AreEqual("Germany", marketplace.Name);
			Assert.AreEqual("DE", marketplace.CountryCode);
			Assert.AreEqual("A1PA6795UKMFR9", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Europe, marketplace.MwsEndpoint);
		}

		[Test]
		public void Italy_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Italy;

			Assert.AreEqual("Italy", marketplace.Name);
			Assert.AreEqual("IT", marketplace.CountryCode);
			Assert.AreEqual("APJ6JRA9NG5V4", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Europe, marketplace.MwsEndpoint);
		}

		[Test]
		public void Brazil_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Brazil;

			Assert.AreEqual("Brazil", marketplace.Name);
			Assert.AreEqual("BR", marketplace.CountryCode);
			Assert.AreEqual("A2Q3Y263D00KWC", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Brazil, marketplace.MwsEndpoint);
		}

		[Test]
		public void India_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.India;

			Assert.AreEqual("India", marketplace.Name);
			Assert.AreEqual("IN", marketplace.CountryCode);
			Assert.AreEqual("A21TJRUUN4KGV", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.India, marketplace.MwsEndpoint);
		}

		[Test]
		public void China_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.China;

			Assert.AreEqual("China", marketplace.Name);
			Assert.AreEqual("CN", marketplace.CountryCode);
			Assert.AreEqual("AAHKV2X7AFYLW", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.China, marketplace.MwsEndpoint);
		}

		[Test]
		public void Japan_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Japan;

			Assert.AreEqual("Japan", marketplace.Name);
			Assert.AreEqual("JP", marketplace.CountryCode);
			Assert.AreEqual("A1VC38T7YXB528", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Japan, marketplace.MwsEndpoint);
		}

		[Test]
		public void Australia_IsInitializedWithExpectedData()
		{
			var marketplace = MwsMarketplace.Australia;

			Assert.AreEqual("Australia", marketplace.Name);
			Assert.AreEqual("AU", marketplace.CountryCode);
			Assert.AreEqual("A39IBJ37TRP1C6", marketplace.Id);
			Assert.AreEqual(MwsEndpoint.Australia, marketplace.MwsEndpoint);
		}
	}
}
