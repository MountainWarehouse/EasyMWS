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
    public class FeedSubmissionCallbackServiceTests
    {
	    private Mock<IFeedSubmissionCallbackRepo> _feedSubmissionCallbackRepoMock;
	    private IFeedSubmissionCallbackService _feedSubmissionCallbackService;
	    private List<FeedSubmissionEntry> _feedSubmissionEntries;
	    private readonly string _merchantId = "TestMerchantId";
	    private readonly AmazonRegion _region = AmazonRegion.Europe;
	    private EasyMwsOptions _options;

		[SetUp]
	    public void Setup()
	    {
		    _options = EasyMwsOptions.Defaults();
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType", false,
			    new List<string>(MwsMarketplaceGroup.AmazonEurope()));
		    var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionEntries = new List<FeedSubmissionEntry>
		    {
				new FeedSubmissionEntry(serializedPropertiesContainer){ Id = 2 },
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					Id = 1,
					AmazonRegion = AmazonRegion.Europe,
					TypeName = "testTypeName",
					DataTypeName = "testDataTypeName",
					MethodName = "testMethodName",
					Data = "testData",
					FeedType = propertiesContainer.FeedType,
					FeedSubmissionData = JsonConvert.SerializeObject(propertiesContainer)
				}
			};

		    _feedSubmissionCallbackRepoMock = new Mock<IFeedSubmissionCallbackRepo>();
		    _feedSubmissionCallbackRepoMock.Setup(x => x.GetAll()).Returns(_feedSubmissionEntries.AsQueryable());
		    _feedSubmissionCallbackService = new FeedSubmissionCallbackService(_feedSubmissionCallbackRepoMock.Object);
		}

	    [Test]
	    public void FirstOrDefault_TwoInQueue_ReturnsFirstObjectPopulatedWithCorrectData()
	    {
			var feedSubmissionEntry = _feedSubmissionCallbackService.FirstOrDefault();
		    var feedSubmissionData = JsonConvert.DeserializeObject<FeedSubmissionPropertiesContainer>(feedSubmissionEntry.FeedSubmissionData);

		    Assert.AreEqual(AmazonRegion.Europe, feedSubmissionEntry.AmazonRegion);
		    Assert.AreEqual("testData", feedSubmissionEntry.Data);
		    Assert.AreEqual("testMethodName", feedSubmissionEntry.MethodName);
		    Assert.AreEqual("testTypeName", feedSubmissionEntry.TypeName);
		    Assert.AreEqual("testDataTypeName", feedSubmissionEntry.DataTypeName);
		    Assert.AreEqual("testFeedType", feedSubmissionEntry.FeedType);
			CollectionAssert.AreEquivalent(new List<string>(MwsMarketplaceGroup.AmazonEurope()), feedSubmissionData.MarketplaceIdList);
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
			    _feedSubmissionCallbackService.GetNextFromQueueOfFeedsToSubmit(_options, _merchantId, _region);

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
				_feedSubmissionCallbackService.GetNextFromQueueOfFeedsToSubmit(_options, _merchantId, AmazonRegion.Australia);

			Assert.IsNotNull(feedSubmissionCallback);
			Assert.AreEqual(feedSubmissionWithSameRegionAndMerchantId1.Id, feedSubmissionCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfFeedsToSubmit_CalledWithNullMerchantId_ReturnsNull()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var testMerchantId = "test merchant id";
			var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 2,
				FeedSubmissionId = "testSubmissionId2",
				FeedSubmissionRetryCount = 0
			};
			var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 3,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};
			var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 4,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};
			var feedSubmissionWithNullMerchant = new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = null,
				Id = 5,
				FeedSubmissionId = null,
				FeedSubmissionRetryCount = 0
			};


			_feedSubmissionEntries.Add(feedSubmissionWithNonNullFeedSubmissionId1);
			_feedSubmissionEntries.Add(feedSubmissionWithNullFeedSubmissionId1);
			_feedSubmissionEntries.Add(feedSubmissionWithNullFeedSubmissionId2);
			_feedSubmissionEntries.Add(feedSubmissionWithNullMerchant);

			var feedSubmissionCallback =
				_feedSubmissionCallbackService.GetNextFromQueueOfFeedsToSubmit(_options, null, _region);

			Assert.IsNull(feedSubmissionCallback);
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
			var listSubmittedFeeds = _feedSubmissionCallbackService.GetIdsForSubmittedFeedsFromQueue(_options, _merchantId, _region);

			// Assert
			Assert.AreEqual(2, listSubmittedFeeds.Count());
			Assert.IsTrue(listSubmittedFeeds.Count(sf => sf == "FeedSubmissionId4" || sf == "FeedSubmissionId5") == 2);
		}

		[Test]
		public void GetAllSubmittedFeedsFromQueue_CalledWithNullMerchantId_ReturnsNull()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId1",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId2",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId3",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId4",
					IsProcessingComplete = false
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 4,
					FeedSubmissionId = "FeedSubmissionId5",
					IsProcessingComplete = true
				},
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					MerchantId = null,
					Id = 5,
					FeedSubmissionId = "FeedSubmissionId6",
					IsProcessingComplete = false
				}
			};

			_feedSubmissionEntries.AddRange(data);

			// Act
			var listOfSubmittedFeeds = _feedSubmissionCallbackService.GetIdsForSubmittedFeedsFromQueue(_options, null, _region);

			// Assert
			Assert.IsEmpty(listOfSubmittedFeeds);

		}
	}
}
