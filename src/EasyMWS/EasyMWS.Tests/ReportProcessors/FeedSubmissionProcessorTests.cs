using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.ReportProcessors;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.ReportProcessors
{
    public class FeedSubmissionProcessorTests
    {
	    private IFeedSubmissionProcessor _feedSubmissionProcessor;
	    private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
	    private EasyMwsOptions _easyMwsOptions;
		private Mock<IFeedSubmissionCallbackService> _feedSubmissionCallbackServiceMock;
	    private List<FeedSubmissionCallback> _feedSubmissionCallbacks;
	    private string _merchantId = "TestMerchantId";
	    private AmazonRegion _region = AmazonRegion.Europe;

		[SetUp]
	    public void Setup()
	    {
		    _easyMwsOptions = EasyMwsOptions.Defaults;
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
		    _feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionCallbackService>();
			_feedSubmissionProcessor = new FeedSubmissionProcessor(_marketplaceWebServiceClientMock.Object, _feedSubmissionCallbackServiceMock.Object, _easyMwsOptions);

			_feedSubmissionCallbacks = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 1,
					FeedSubmissionId = "testSubmissionId1",
					FeedSubmissionData = "testFeedSubmissionData"
				}
			};

			_feedSubmissionCallbackServiceMock.Setup(x => x.GetAll()).Returns(_feedSubmissionCallbacks.AsQueryable());

		    _feedSubmissionCallbackServiceMock.Setup(x => x.Where(It.IsAny<Expression<Func<FeedSubmissionCallback, bool>>>()))
			    .Returns((Expression<Func<FeedSubmissionCallback, bool>> e) => _feedSubmissionCallbacks.AsQueryable().Where(e));

			_feedSubmissionCallbackServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<FeedSubmissionCallback, bool>>>()))
			    .Returns((Expression<Func<FeedSubmissionCallback, bool>> e) => _feedSubmissionCallbacks.AsQueryable().FirstOrDefault(e));
		}

		[Test]
	    public void GetNextFeedToSubmitFromQueue_ReturnsFirstFeedSubmissionFromQueueWithNullFeedSubmissionId_AndSkipsEntriesWithNonNullFeedSubmissionId()
	    {
		    var testMerchantId = "test merchant id";
		    var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 2, FeedSubmissionId = "testSubmissionId2", SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };


		    _feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);

		    var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue(AmazonRegion.Europe, testMerchantId);

			Assert.IsNotNull(feedSubmissionCallback);
		    Assert.AreEqual(feedSubmissionWithNullFeedSubmissionId1.Id, feedSubmissionCallback.Id);
	    }

	    [Test]
	    public void GetNextFeedToSubmitFromQueue_ReturnsFirstFeedSubmissionFromQueueForGivenRegionAndMerchantId()
	    {
		    var merchantId1 = "test merchant id 1";
		    var merchantId2 = "test merchant id 2";
		    var feedSubmissionWithDifferentRegion = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = merchantId1, Id = 2, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithSameRegionButDifferentMerchantId = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = merchantId1, Id = 2, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithSameRegionAndMerchantId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = merchantId2, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithSameRegionAndMerchantId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = merchantId2, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };


			_feedSubmissionCallbacks.Add(feedSubmissionWithDifferentRegion);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionButDifferentMerchantId);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionAndMerchantId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionAndMerchantId2);

			var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue(AmazonRegion.Australia, merchantId2);

		    Assert.IsNotNull(feedSubmissionCallback);
		    Assert.AreEqual(feedSubmissionWithSameRegionAndMerchantId1.Id, feedSubmissionCallback.Id);
		}

	    [Test]
	    public void GetNextFeedToSubmitFromQueue_CalledWithNullMerchantId_ReturnsNull()
	    {
			var testMerchantId = "test merchant id";
		    var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 2, FeedSubmissionId = "testSubmissionId2", SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };
			var feedSubmissionWithNullMerchant = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = null, Id = 5, FeedSubmissionId = null, SubmissionRetryCount = 0 };


			_feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullMerchant);

			var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue(AmazonRegion.Europe, null);

		    Assert.IsNull(feedSubmissionCallback);
		}

	    [Test]
	    public void SubmitSingleQueuedFeedToAmazon_CalledWithNullFeedSubmissionCallback_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(()=> _feedSubmissionProcessor.SubmitSingleQueuedFeedToAmazon(null, "testMerchantId"));
	    }

	    [Test]
	    public void SubmitSingleQueuedFeedToAmazon_CalledWithNullMerchantId_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() => _feedSubmissionProcessor.SubmitSingleQueuedFeedToAmazon(new FeedSubmissionCallback(), null));
	    }

	    [Test]
	    public void SubmitSingleQueuedFeedToAmazon_CalledWithEmptyMerchantId_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() => _feedSubmissionProcessor.SubmitSingleQueuedFeedToAmazon(new FeedSubmissionCallback(), string.Empty));
	    }

	    [Test]
	    public void MoveToQueueOfSubmittedFeeds_UpdatesFeedSubmissionId_OnTheCallback()
	    {
		    var testFeedSubmissionId = "testFeedSubmissionId";

		    _feedSubmissionProcessor.MoveToQueueOfSubmittedFeeds(_feedSubmissionCallbacks[0], testFeedSubmissionId);

		    Assert.AreEqual("testFeedSubmissionId", _feedSubmissionCallbacks[0].FeedSubmissionId);
		    _feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
	    }

	    [Test]
	    public void AllocateFeedSubmissionForRetry_CalledOnce_IncrementsRequestRetryCountCorrectly()
	    {
		    Assert.AreEqual(0, _feedSubmissionCallbacks.First().SubmissionRetryCount);

		    _feedSubmissionProcessor.AllocateFeedSubmissionForRetry(_feedSubmissionCallbacks.First());

		    Assert.AreEqual(1, _feedSubmissionCallbacks.First().SubmissionRetryCount);
		    _feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
	    }

	    [Test]
		public void AllocateFeedSubmissionForRetry_CalledMultipleTimes_IncrementsRequestRetryCountCorrectly()
	    {
		    Assert.AreEqual(0, _feedSubmissionCallbacks.First().SubmissionRetryCount);

		    _feedSubmissionProcessor.AllocateFeedSubmissionForRetry(_feedSubmissionCallbacks.First());
		    _feedSubmissionProcessor.AllocateFeedSubmissionForRetry(_feedSubmissionCallbacks.First());
		    _feedSubmissionProcessor.AllocateFeedSubmissionForRetry(_feedSubmissionCallbacks.First());

			Assert.AreEqual(3, _feedSubmissionCallbacks.First().SubmissionRetryCount);
		    _feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(3));
	    }

		[Test]
		public void GetAllSubmittedFeeds_ReturnsListOfSubmittedFeeds_ForGivenMerchant()
		{
			// Arrange
			var testMerchantId2 = "test merchant id 2";
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId1",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId2",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 4,
					FeedSubmissionId = "FeedSubmissionId3",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 5,
					FeedSubmissionId = "FeedSubmissionId4",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 6,
					FeedSubmissionId = null,
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 6,
					FeedSubmissionId = "FeedSubmissionId5",
					IsProcessingComplete = true
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = _merchantId,
					Id = 7,
					FeedSubmissionId = "FeedSubmissionId6",
					IsProcessingComplete = false
				}
			};

			_feedSubmissionCallbacks.AddRange(data);

			// Act
			var listSubmittedFeeds = _feedSubmissionProcessor.GetAllSubmittedFeeds(_region, _merchantId);

			// Assert
			Assert.AreEqual(2, listSubmittedFeeds.Count());
			Assert.IsTrue(listSubmittedFeeds.Count(sf => sf.Id == 4 || sf.Id == 5) == 2);
		}

		[Test]
		public void GetAllSubmittedFeeds_CalledWithNullMerchantId_ReturnsNull()
		{
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId1",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId2",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId3",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId4",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 4,
					FeedSubmissionId = "FeedSubmissionId5",
					IsProcessingComplete = true
				},
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = null,
					Id = 5,
					FeedSubmissionId = "FeedSubmissionId6",
					IsProcessingComplete = false
				}
			};

			_feedSubmissionCallbacks.AddRange(data);

			// Act
			var listOfSubmittedFeeds = _feedSubmissionProcessor.GetAllSubmittedFeeds(_region, null);

			// Assert
			Assert.IsEmpty(listOfSubmittedFeeds);

		}

	    [Test]
	    public void RequestReportsStatuses_WithMultiplePendingReports_SubmitsAmazonRequest()
	    {
		    var getFeedSubmissionListResponse = new GetFeedSubmissionListResponse
		    {
			    GetFeedSubmissionListResult = new GetFeedSubmissionListResult
				{
					FeedSubmissionInfo = new List<FeedSubmissionInfo>
				    {
					    new FeedSubmissionInfo
						{
							FeedProcessingStatus = "_DONE_",
							FeedSubmissionId = "feed1"
					    },
					    new FeedSubmissionInfo
						{
							FeedProcessingStatus = "_CANCELLED_",
							FeedSubmissionId = "feed2"
						},
					    new FeedSubmissionInfo
						{
							FeedProcessingStatus = "_OTHER_",
							FeedSubmissionId = "feed3"

						}
				    }
			    }
		    };
			var testRequestIdList = new List<string> { "Report1", "Report2", "Report3" };
		    _marketplaceWebServiceClientMock.Setup(x => x.GetFeedSubmissionList(It.IsAny<GetFeedSubmissionListRequest>()))
			    .Returns(getFeedSubmissionListResponse);

			var result = _feedSubmissionProcessor.GetFeedSubmissionResults(testRequestIdList, "");

		    Assert.AreEqual("_DONE_", result.First(x => x.FeedSubmissionId == "feed1").FeedProcessingStatus);
		    Assert.AreEqual("_OTHER_", result.First(x => x.FeedSubmissionId == "feed3").FeedProcessingStatus);
			Assert.AreEqual("_CANCELLED_", result.First(x => x.FeedSubmissionId == "feed2").FeedProcessingStatus);
	    }

	    [Test]
	    public void MoveFeedsToQueuesAccordingToProcessingStatus_MovesFeedToProcessingCompleteQueue_IfProcessingStatusIsDone()
	    {
		    var data = new List<FeedSubmissionCallback>
		    {
			    new FeedSubmissionCallback{FeedSubmissionId = "testId", IsProcessingComplete = false, SubmissionRetryCount = 0}
		    };

			_feedSubmissionCallbacks.AddRange(data);

		    var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
		    {
				("testId", "_DONE_")
			};

			_feedSubmissionProcessor.MoveFeedsToQueuesAccordingToProcessingStatus(resultsInfo);

			Assert.IsTrue(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
		    _feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

	    [Test]
	    public void MoveFeedsToQueuesAccordingToProcessingStatus_ReturnsFeedToRetryGetResultsQueue_IfProcessingStatusIsAsExpected()
	    {
		    var data = new List<FeedSubmissionCallback>
		    {
			    new FeedSubmissionCallback{FeedSubmissionId = "testId1", IsProcessingComplete = false, SubmissionRetryCount = 0},
			    new FeedSubmissionCallback{FeedSubmissionId = "testId2", IsProcessingComplete = false, SubmissionRetryCount = 0},
			    new FeedSubmissionCallback{FeedSubmissionId = "testId3", IsProcessingComplete = false, SubmissionRetryCount = 0},
			    new FeedSubmissionCallback{FeedSubmissionId = "testId4", IsProcessingComplete = false, SubmissionRetryCount = 0},
			    new FeedSubmissionCallback{FeedSubmissionId = "testId5", IsProcessingComplete = false, SubmissionRetryCount = 0}
			};

		    _feedSubmissionCallbacks.AddRange(data);

		    var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
		    {
			    ("testId1", "_AWAITING_ASYNCHRONOUS_REPLY_"),
			    ("testId2", "_IN_PROGRESS_"),
			    ("testId3", "_IN_SAFETY_NET_"),
			    ("testId4", "_SUBMITTED_"),
			    ("testId5", "_UNCONFIRMED_")
			};

		    _feedSubmissionProcessor.MoveFeedsToQueuesAccordingToProcessingStatus(resultsInfo);

		    Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId1").IsProcessingComplete);
		    Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId1").SubmissionRetryCount);
		    Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId2").IsProcessingComplete);
		    Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId2").SubmissionRetryCount);
		    Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId3").IsProcessingComplete);
		    Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId3").SubmissionRetryCount);
		    Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId4").IsProcessingComplete);
		    Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId4").SubmissionRetryCount);
		    Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId5").IsProcessingComplete);
		    Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId5").SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(5));
		    _feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

	    [Test]
	    public void MoveFeedsToQueuesAccordingToProcessingStatus_RemovesFeedFromDb_IfProcessingStatusIsCancelledOrUnknown()
	    {
		    var data = new List<FeedSubmissionCallback>
		    {
			    new FeedSubmissionCallback{FeedSubmissionId = "testId", IsProcessingComplete = false, SubmissionRetryCount = 0},
			    new FeedSubmissionCallback{FeedSubmissionId = "testId2", IsProcessingComplete = false, SubmissionRetryCount = 0}
			};

		    _feedSubmissionCallbacks.AddRange(data);

		    var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
		    {
			    ("testId", "_CANCELLED_"),
			    ("testId2", "_SOME_MADE_UP_STATUS_")
			};

		    _feedSubmissionProcessor.MoveFeedsToQueuesAccordingToProcessingStatus(resultsInfo);

		    _feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		    _feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(2));
		}
	}
}
