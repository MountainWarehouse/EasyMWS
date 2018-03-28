using System;
using System.Linq;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class MwsMarketplaceGroupTests
    {
		[Test]
		public void MwsMarketplaceGroup_Ctor_InitializesMwsAccessEndpoint()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);

			Assert.AreEqual(MwsMarketplace.Australia.MwsEndpoint.Region, MwsMarketplaceGroup.MwsEndpoint.Region);
			Assert.AreEqual(MwsMarketplace.Australia.MwsEndpoint.RegionOrMarketPlaceEndpoint, MwsMarketplaceGroup.MwsEndpoint.RegionOrMarketPlaceEndpoint);
		}

		[Test]
		public void MwsMarketplaceGroup_Ctor_AddsMarketplaceIdToInternalMarketplaceIdList()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);

			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.Australia.Id);
			Assert.AreEqual(1, MwsMarketplaceGroup.GetMarketplacesIdList.Count());
		}

		[Test]
		public void TryAddMarketplace_TryingToAddAMarketplaceToTheSameGroupTwice_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.France);
			MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Germany);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Germany));
		}

		[Test]
		public void TryAddMarketplace_TryingToAddAMarketplaceToAGroupInitializedWithTheSameMarketplace_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.France);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.France));
		}

		[Test]
		public void TryAddMarketplace_WithValidMarketplace_AddsMarketplaceIdToInternalMarketplaceIdList()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Canada);

			MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.US);

			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.Canada.Id);
			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.US.Id);
			Assert.AreEqual(2, MwsMarketplaceGroup.GetMarketplacesIdList.Count());
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUS_CanAddCanadaMarketplaceId_ToInternalMarketplaceIdList()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

			MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Canada);

			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.US.Id);
			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.Canada.Id);
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUS_CanAddMexicoMarketplaceId_ToInternalMarketplaceIdList()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

			MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Mexico);

			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.US.Id);
			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.Mexico.Id);
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUS_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.UK));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUS_CannotAddNonNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.US);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Japan));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUK_CanAddEuropeanMarketplaceId_ToInternalMarketplaceIdList()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);

			MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Germany);

			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.UK.Id);
			CollectionAssert.Contains(MwsMarketplaceGroup.GetMarketplacesIdList, MwsMarketplace.Germany.Id);
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUK_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Canada));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfUK_CannotAddNonEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.UK);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.China));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfJapan_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.France));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfJapan_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.US));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfJapan_CannotAddAnotherMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Japan);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.China));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfAustralia_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.France));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfAustralia_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.US));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfAustralia_CannotAddAnotherMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Australia);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.China));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfChina_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.China);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.France));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfChina_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.China);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.US));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfChina_CannotAddAnotherMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.China);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Brazil));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfIndia_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.France));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfIndia_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.US));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfIndia_CannotAddAnotherMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.India);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Brazil));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfBrazil_CannotAddEuropeanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Brazil);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.France));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfBrazil_CannotAddNorthAmericanMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Brazil);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.US));
		}

		[Test]
		public void TryAddMarketplace_WithInitialEndpointOfBrazil_CannotAddAnotherMarketplaceId_ThrowsInvalidOperationException()
		{
			var MwsMarketplaceGroup = new MwsMarketplaceGroup(MwsMarketplace.Brazil);

			Assert.Throws<InvalidOperationException>(() =>
				MwsMarketplaceGroup.TryAddMarketplace(MwsMarketplace.Australia));
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
