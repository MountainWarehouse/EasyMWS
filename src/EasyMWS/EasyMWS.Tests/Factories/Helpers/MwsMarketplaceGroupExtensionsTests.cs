using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class MwsMarketplaceGroupExtensionsTests
    {
	    [Test]
	    public void AddMarketplace_WithValidMarketplace_AddsMarketplaceIdToInternalMarketplaceIdList()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.Germany)
			    .AddMarketplace(MwsMarketplace.France);

		    CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.Germany.Id);
		    CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.France.Id);
		    Assert.AreEqual(2, reportRequestedMarketplacesGroup.GetMarketplacesIdList.Count);
	    }

	    [Test]
	    public void AddMarketplace_TryingToAddAMarketplaceToTheSameGroupTwice_ThrowsInvalidOperationException()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.Germany);

		    Assert.Throws<InvalidOperationException>(() =>
			    reportRequestedMarketplacesGroup
				    .AddMarketplace(MwsMarketplace.France)
				    .AddMarketplace(MwsMarketplace.France));
	    }

	    [Test]
	    public void
		    AddMarketplace_TryingToAddAMarketplaceToAGroupInitializedWithTheSameMarketplace_ThrowsInvalidOperationException()
	    {
		    Assert.Throws<InvalidOperationException>(() =>
			    new MwsMarketplaceGroup(MwsMarketplace.France)
				    .AddMarketplace(MwsMarketplace.France));
	    }
	}
}
