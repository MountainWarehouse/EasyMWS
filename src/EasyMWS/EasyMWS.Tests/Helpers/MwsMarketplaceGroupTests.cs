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

		[Test]
		public void AmazonGlobal_Returns_ANotNullOrEmptyList()
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonGlobal();

			Assert.NotNull(amazonGlobalMarketplaceIds);
			CollectionAssert.IsNotEmpty(amazonGlobalMarketplaceIds);
		}

		[TestCase("A2EUQ1WTGCTBG2")]    // Canada marketplace id
		[TestCase("ATVPDKIKX0DER")]     // US marketplace id
		[TestCase("A1AM78C64UM0Y8")]    // Mexico marketplace id
		[TestCase("A1RKKUPIHCS9HS")]    // Spain marketplace id
		[TestCase("A1F83G8C2ARO7P")]    // UK marketplace id
		[TestCase("A13V1IB3VIYZZH")]    // France marketplace id
		[TestCase("A1PA6795UKMFR9")]    // Germany marketplace id
		[TestCase("APJ6JRA9NG5V4")]     // Italy marketplace id
		[TestCase("A2Q3Y263D00KWC")]    // Brazil marketplace id
		[TestCase("A21TJRUUN4KGV")]     // India marketplace id
		[TestCase("AAHKV2X7AFYLW")]     // China marketplace id
		[TestCase("A1VC38T7YXB528")]    // Japan marketplace id
		[TestCase("A39IBJ37TRP1C6")]    // Australia marketplace id
		public void AmazonGlobal_Returns_ListContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonGlobal();

			CollectionAssert.Contains(amazonGlobalMarketplaceIds, marketplaceId);
		}

		[Test]
		public void AmazonEurope_Returns_ANotNullOrEmptyList()
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonEurope();

			Assert.NotNull(amazonGlobalMarketplaceIds);
			CollectionAssert.IsNotEmpty(amazonGlobalMarketplaceIds);
		}


		[TestCase("A1RKKUPIHCS9HS")]    // Spain marketplace id
		[TestCase("A1F83G8C2ARO7P")]    // UK marketplace id
		[TestCase("A13V1IB3VIYZZH")]    // France marketplace id
		[TestCase("A1PA6795UKMFR9")]    // Germany marketplace id
		[TestCase("APJ6JRA9NG5V4")]     // Italy marketplace id
		public void AmazonEurope_Returns_ListContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonEurope();

			CollectionAssert.Contains(amazonGlobalMarketplaceIds, marketplaceId);
		}

		[TestCase("A2EUQ1WTGCTBG2")]    // Canada marketplace id
		[TestCase("ATVPDKIKX0DER")]     // US marketplace id
		[TestCase("A1AM78C64UM0Y8")]    // Mexico marketplace id
		[TestCase("A2Q3Y263D00KWC")]    // Brazil marketplace id
		[TestCase("A21TJRUUN4KGV")]     // India marketplace id
		[TestCase("AAHKV2X7AFYLW")]     // China marketplace id
		[TestCase("A1VC38T7YXB528")]    // Japan marketplace id
		[TestCase("A39IBJ37TRP1C6")]    // Australia marketplace id
		public void AmazonEurope_Returns_ListNotContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonEurope();

			CollectionAssert.DoesNotContain(amazonGlobalMarketplaceIds, marketplaceId);
		}

		[Test]
		public void AmazonNorthAmerica_Returns_ANotNullOrEmptyList()
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonNorthAmerica();

			Assert.NotNull(amazonGlobalMarketplaceIds);
			CollectionAssert.IsNotEmpty(amazonGlobalMarketplaceIds);
		}

		[TestCase("A2EUQ1WTGCTBG2")]    // Canada marketplace id
		[TestCase("ATVPDKIKX0DER")]     // US marketplace id
		[TestCase("A1AM78C64UM0Y8")]    // Mexico marketplace id
		public void AmazonNorthAmerica_Returns_ListContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonNorthAmerica();

			CollectionAssert.Contains(amazonGlobalMarketplaceIds, marketplaceId);
		}

		[TestCase("A1RKKUPIHCS9HS")]    // Spain marketplace id
		[TestCase("A1F83G8C2ARO7P")]    // UK marketplace id
		[TestCase("A13V1IB3VIYZZH")]    // France marketplace id
		[TestCase("A1PA6795UKMFR9")]    // Germany marketplace id
		[TestCase("APJ6JRA9NG5V4")]     // Italy marketplace id
		[TestCase("A2Q3Y263D00KWC")]    // Brazil marketplace id
		[TestCase("A21TJRUUN4KGV")]     // India marketplace id
		[TestCase("AAHKV2X7AFYLW")]     // China marketplace id
		[TestCase("A1VC38T7YXB528")]    // Japan marketplace id
		[TestCase("A39IBJ37TRP1C6")]    // Australia marketplace id
		public void AmazonNorthAmerica_Returns_ListNotContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonNorthAmerica();

			CollectionAssert.DoesNotContain(amazonGlobalMarketplaceIds, marketplaceId);
		}

		[Test]
		public void AmazonOther_Returns_ANotNullOrEmptyList()
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonOther();

			Assert.NotNull(amazonGlobalMarketplaceIds);
			CollectionAssert.IsNotEmpty(amazonGlobalMarketplaceIds);
		}

		[TestCase("A2Q3Y263D00KWC")]    // Brazil marketplace id
		[TestCase("A21TJRUUN4KGV")]     // India marketplace id
		[TestCase("AAHKV2X7AFYLW")]     // China marketplace id
		[TestCase("A1VC38T7YXB528")]    // Japan marketplace id
		[TestCase("A39IBJ37TRP1C6")]    // Australia marketplace id
		public void AmazonOther_Returns_ListContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonOther();

			CollectionAssert.Contains(amazonGlobalMarketplaceIds, marketplaceId);
		}

		[TestCase("A2EUQ1WTGCTBG2")]    // Canada marketplace id
		[TestCase("ATVPDKIKX0DER")]     // US marketplace id
		[TestCase("A1AM78C64UM0Y8")]    // Mexico marketplace id
		[TestCase("A1RKKUPIHCS9HS")]    // Spain marketplace id
		[TestCase("A1F83G8C2ARO7P")]    // UK marketplace id
		[TestCase("A13V1IB3VIYZZH")]    // France marketplace id
		[TestCase("A1PA6795UKMFR9")]    // Germany marketplace id
		[TestCase("APJ6JRA9NG5V4")]     // Italy marketplace id
		public void AmazonOther_Returns_ListNotContainingExpectedMarketplaceID(string marketplaceId)
		{
			var amazonGlobalMarketplaceIds = MwsMarketplaceGroup.AmazonOther();

			CollectionAssert.DoesNotContain(amazonGlobalMarketplaceIds, marketplaceId);
		}
	}
}
