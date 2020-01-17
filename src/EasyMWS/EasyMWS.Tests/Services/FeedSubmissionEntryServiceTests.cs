using System.Collections.Generic;
using System.Linq;
using Moq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.Repositories;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests.Services
{
    public class FeedSubmissionEntryServiceTests
    {
	    private Mock<IFeedSubmissionEntryRepository> _feedSubmissionCallbackRepoMock;
	    private IFeedSubmissionEntryService _feedSubmissionEntryService;
	    private List<FeedSubmissionEntry> _feedSubmissionEntries;
	    private readonly string _merchantId = "TestMerchantId";
	    private readonly AmazonRegion _region = AmazonRegion.Europe;
	    private EasyMwsOptions _options;

        [SetUp]
        public void Setup()
        {
            _options = new EasyMwsOptions();
            var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType", false,
                new List<string>(MwsMarketplaceGroup.AmazonEurope().Select(m => m.Id)));
            var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

            _feedSubmissionEntries = new List<FeedSubmissionEntry>
            {
                new FeedSubmissionEntry(serializedPropertiesContainer){ Id = 2 },
                new FeedSubmissionEntry(serializedPropertiesContainer)
                {
                    Id = 1,
                    AmazonRegion = AmazonRegion.Europe,
                    TargetHandlerId = "TargetHandlerId",
                    TargetHandlerArgs = JsonConvert.SerializeObject(new Dictionary<string, object>{ { "key1", "value1"} }),
                    FeedType = propertiesContainer.FeedType,
                    FeedSubmissionData = JsonConvert.SerializeObject(propertiesContainer)
                }
            };

            _feedSubmissionCallbackRepoMock = new Mock<IFeedSubmissionEntryRepository>();
            _feedSubmissionCallbackRepoMock.Setup(x => x.GetAll()).Returns(_feedSubmissionEntries.AsQueryable());
            _feedSubmissionEntryService = new FeedSubmissionEntryService(_feedSubmissionCallbackRepoMock.Object, _options);
        }

        [Test]
        public void FirstOrDefault_TwoInQueue_ReturnsFirstObjectPopulatedWithCorrectData()
        {
            var feedSubmissionEntry = _feedSubmissionEntryService.FirstOrDefault();
            var feedSubmissionData = JsonConvert.DeserializeObject<FeedSubmissionPropertiesContainer>(feedSubmissionEntry.FeedSubmissionData);

            Assert.AreEqual(AmazonRegion.Europe, feedSubmissionEntry.AmazonRegion);
            Assert.AreEqual("TargetHandlerId", feedSubmissionEntry.TargetHandlerId);
            Assert.AreEqual("{\"key1\":\"value1\"}", feedSubmissionEntry.TargetHandlerArgs);
            Assert.AreEqual("testFeedType", feedSubmissionEntry.FeedType);
            CollectionAssert.AreEquivalent(new List<string>(MwsMarketplaceGroup.AmazonEurope().Select(m => m.Id)), feedSubmissionData.MarketplaceIdList);
        }

        [Test]
	    public void
		    GetNextFromQueueOfFeedsToSubmit_ReturnsFirstFeedSubmissionFromQueueWithNullFeedSubmissionId_AndSkipsEntriesWithNonNullFeedSubmissionId()
	    {
		    var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
		    var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

		    var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionEntry(serializedPropertiesContainer)
		    {
			    AmazonRegion = AmazonRegion.Europe,
			    MerchantId = _merchantId,
			    Id = 2,
			    FeedSubmissionId = "testSubmissionId2",
			    FeedSubmissionRetryCount = 0
		    };
		    var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionEntry(serializedPropertiesContainer)
		    {
			    AmazonRegion = AmazonRegion.Europe,
			    MerchantId = _merchantId,
			    Id = 3,
			    FeedSubmissionId = null,
			    FeedSubmissionRetryCount = 0
		    };
		    var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionEntry(serializedPropertiesContainer)
		    {
			    AmazonRegion = AmazonRegion.Europe,
			    MerchantId = _merchantId,
			    Id = 4,
			    FeedSubmissionId = null,
			    FeedSubmissionRetryCount = 0
		    };


		    _feedSubmissionEntries.Add(feedSubmissionWithNonNullFeedSubmissionId1);
		    _feedSubmissionEntries.Add(feedSubmissionWithNullFeedSubmissionId1);
		    _feedSubmissionEntries.Add(feedSubmissionWithNullFeedSubmissionId2);

		    var feedSubmissionCallback =
			    _feedSubmissionEntryService.GetNextFromQueueOfFeedsToSubmit(_merchantId, _region);

		    Assert.IsNotNull(feedSubmissionCallback);
		    Assert.AreEqual(feedSubmissionWithNullFeedSubmissionId1.Id, feedSubmissionCallback.Id);
	    }

		[Test]
		public void GetNextFromQueueOfFeedsToSubmit_ReturnsFirstFeedSubmissionFromQueueForGivenRegionAndMerchantId()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var merchantId1 = "test merchant id 1";
			var feedSubmissionWithDifferentRegion = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = merchantId1,
				Id = 2,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};
			var feedSubmissionWithSameRegionButDifferentMerchantId = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Australia,
				MerchantId = merchantId1,
				Id = 2,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};
			var feedSubmissionWithSameRegionAndMerchantId1 = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Australia,
				MerchantId = _merchantId,
				Id = 3,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};
			var feedSubmissionWithSameRegionAndMerchantId2 = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Australia,
				MerchantId = _merchantId,
				Id = 4,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};


			_feedSubmissionEntries.Add(feedSubmissionWithDifferentRegion);
			_feedSubmissionEntries.Add(feedSubmissionWithSameRegionButDifferentMerchantId);
			_feedSubmissionEntries.Add(feedSubmissionWithSameRegionAndMerchantId1);
			_feedSubmissionEntries.Add(feedSubmissionWithSameRegionAndMerchantId2);

			var feedSubmissionCallback =
				_feedSubmissionEntryService.GetNextFromQueueOfFeedsToSubmit(_merchantId, AmazonRegion.Australia);

			Assert.IsNotNull(feedSubmissionCallback);
			Assert.AreEqual(feedSubmissionWithSameRegionAndMerchantId1.Id, feedSubmissionCallback.Id);
		}

		[Test]
		public void GetAllSubmittedFeedsFromQueue_ReturnsListOfSubmittedFeeds_ForGivenMerchant()
		{
			// Arrange
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var testMerchantId2 = "test merchant id 2";
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = testMerchantId2,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId2",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = testMerchantId2,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId3",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 4,
					FeedSubmissionId = "FeedSubmissionId4",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 5,
					FeedSubmissionId = "FeedSubmissionId5",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 6,
					FeedSubmissionId = null,
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 7,
					FeedSubmissionId = "FeedSubmissionId7",
					IsProcessingComplete = true
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					MerchantId = _merchantId,
					Id = 8,
					FeedSubmissionId = "FeedSubmissionId8",
					IsProcessingComplete = false
				}
			};

			_feedSubmissionEntries.AddRange(data);

			// Act
			var listSubmittedFeeds = _feedSubmissionEntryService.GetIdsForSubmittedFeedsFromQueue(_merchantId, _region);

			// Assert
			Assert.AreEqual(2, listSubmittedFeeds.Count());
			Assert.IsTrue(listSubmittedFeeds.Count(sf => sf == "FeedSubmissionId4" || sf == "FeedSubmissionId5") == 2);
		}
	}
}
