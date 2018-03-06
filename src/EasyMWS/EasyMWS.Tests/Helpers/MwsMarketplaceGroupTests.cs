using System;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class MwsMarketplaceGroupTests
    {
	    [Test]
	    public void ReportRequestedMarketplacesGroup_Ctor_InitializesMwsAccessEndpoint()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

		    Assert.AreEqual(MwsMarketplace.US.MwsEndpoint, reportRequestedMarketplacesGroup.MwsEndpoint);
	    }

		[Test]
		public void ReportRequestedMarketplacesGroup_Ctor_AddsMarketplaceIdToInternalMarketplaceIdList()
		{
			var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

			CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.US.Id);
			Assert.AreEqual(1, reportRequestedMarketplacesGroup.GetMarketplacesIdList.Count);
		}

		[Test]
		public void TryAddMarketplace_TryingToAddAMarketplaceToTheSameGroupTwice_ThrowsInvalidOperationException()
		{
			var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.US);
			reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.Canada);

			Assert.Throws<InvalidOperationException>(() =>
				reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.Canada));
		}

	    [Test]
	    public void TryAddMarketplace_TryingToAddAMarketplaceToAGroupInitializedWithTheSameMarketplace_ThrowsInvalidOperationException()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

		    Assert.Throws<InvalidOperationException>(() =>
			    reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.US));
	    }

	    [Test]
	    public void TryAddMarketplace_WithValidMarketplace_AddsMarketplaceIdToInternalMarketplaceIdList_AndLeavesPreviousListContentUnchanged()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

		    reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.US);

		    CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.Canada.Id);
		    CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.US.Id);
		    Assert.AreEqual(2, reportRequestedMarketplacesGroup.GetMarketplacesIdList.Count);
	    }

	    [Test]
	    public void TryAddMarketplace_WithInitialEndpointOfUS_CanAddCanadaMarketplaceId_ToInternalMarketplaceIdList()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

		    reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.Canada);

		    CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.US.Id);
		    CollectionAssert.Contains(reportRequestedMarketplacesGroup.GetMarketplacesIdList, MwsMarketplace.Canada.Id);
	    }

	    [Test]
	    public void TryAddMarketplace_WithInitialEndpointOfUS_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

		    Assert.Throws<InvalidOperationException>(() =>
			    reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.UK));
	    }

	    [Test]
	    public void TryAddMarketplace_WithInitialEndpointOfUK_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
	    {
		    var reportRequestedMarketplacesGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);

		    Assert.Throws<InvalidOperationException>(() =>
			    reportRequestedMarketplacesGroup.TryAddMarketplace(MwsMarketplace.Canada));
	    }
	}
}
