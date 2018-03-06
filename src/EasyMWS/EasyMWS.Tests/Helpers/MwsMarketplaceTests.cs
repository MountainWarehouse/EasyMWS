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
		    Assert.AreEqual(MwsEndpoint.NorthAmerica.Name, marketplace.MwsEndpoint.Name);
	    }

	    [Test]
	    public void US_IsInitializedWithExpectedData()
	    {
		    var marketplace = MwsMarketplace.US;

		    Assert.AreEqual("US", marketplace.Name);
		    Assert.AreEqual("US", marketplace.CountryCode);
		    Assert.AreEqual("ATVPDKIKX0DER", marketplace.Id);
		    Assert.AreEqual(MwsEndpoint.NorthAmerica.Name, marketplace.MwsEndpoint.Name);
	    }

	    [Test]
	    public void UK_IsInitializedWithExpectedData()
	    {
		    var marketplace = MwsMarketplace.UK;

		    Assert.AreEqual("UK", marketplace.Name);
		    Assert.AreEqual("UK", marketplace.CountryCode);
		    Assert.AreEqual("A1F83G8C2ARO7P", marketplace.Id);
		    Assert.AreEqual(MwsEndpoint.Europe.Name, marketplace.MwsEndpoint.Name);
	    }
	}
}
