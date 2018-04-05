using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests.Processors
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
			_easyMwsOptions = EasyMwsOptions.Defaults();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionCallbackService>();
			_feedSubmissionProcessor = new FeedSubmissionProcessor(_marketplaceWebServiceClientMock.Object,
				_feedSubmissionCallbackServiceMock.Object, _easyMwsOptions);

			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionCallbacks = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
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
				.Returns((Expression<Func<FeedSubmissionCallback, bool>> e) =>
					_feedSubmissionCallbacks.AsQueryable().FirstOrDefault(e));
		}

		[Test]
		public void
			GetNextFromQueueOfFeedsToSubmit_ReturnsFirstFeedSubmissionFromQueueWithNullFeedSubmissionId_AndSkipsEntriesWithNonNullFeedSubmissionId()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			var testMerchantId = "test merchant id";
			var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 2,
				FeedSubmissionId = "testSubmissionId2",
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 3,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 4,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};


			_feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
			_feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
			_feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);

			var feedSubmissionCallback =
				_feedSubmissionProcessor.GetNextFromQueueOfFeedsToSubmit(AmazonRegion.Europe, testMerchantId);

			Assert.IsNotNull(feedSubmissionCallback);
			Assert.AreEqual(feedSubmissionWithNullFeedSubmissionId1.Id, feedSubmissionCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfFeedsToSubmit_ReturnsFirstFeedSubmissionFromQueueForGivenRegionAndMerchantId()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var merchantId1 = "test merchant id 1";
			var merchantId2 = "test merchant id 2";
			var feedSubmissionWithDifferentRegion = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = merchantId1,
				Id = 2,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithSameRegionButDifferentMerchantId = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Australia,
				MerchantId = merchantId1,
				Id = 2,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithSameRegionAndMerchantId1 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Australia,
				MerchantId = merchantId2,
				Id = 3,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithSameRegionAndMerchantId2 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Australia,
				MerchantId = merchantId2,
				Id = 4,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};


			_feedSubmissionCallbacks.Add(feedSubmissionWithDifferentRegion);
			_feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionButDifferentMerchantId);
			_feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionAndMerchantId1);
			_feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionAndMerchantId2);

			var feedSubmissionCallback =
				_feedSubmissionProcessor.GetNextFromQueueOfFeedsToSubmit(AmazonRegion.Australia, merchantId2);

			Assert.IsNotNull(feedSubmissionCallback);
			Assert.AreEqual(feedSubmissionWithSameRegionAndMerchantId1.Id, feedSubmissionCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfFeedsToSubmit_CalledWithNullMerchantId_ReturnsNull()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var testMerchantId = "test merchant id";
			var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 2,
				FeedSubmissionId = "testSubmissionId2",
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 3,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = testMerchantId,
				Id = 4,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};
			var feedSubmissionWithNullMerchant = new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
			{
				AmazonRegion = AmazonRegion.Europe,
				MerchantId = null,
				Id = 5,
				FeedSubmissionId = null,
				SubmissionRetryCount = 0
			};


			_feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
			_feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
			_feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);
			_feedSubmissionCallbacks.Add(feedSubmissionWithNullMerchant);

			var feedSubmissionCallback =
				_feedSubmissionProcessor.GetNextFromQueueOfFeedsToSubmit(AmazonRegion.Europe, null);

			Assert.IsNull(feedSubmissionCallback);
		}

		[Test]
		public void SubmitFeedToAmazon_CalledWithNullFeedSubmissionCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_feedSubmissionProcessor.SubmitFeedToAmazon(null));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestCallbackWithNullFeedSubmissionData_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => _feedSubmissionProcessor.SubmitFeedToAmazon(new FeedSubmissionCallback(It.IsAny<Callback>(), null)));
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
		public void MoveToRetryQueue_CalledOnce_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _feedSubmissionCallbacks.First().SubmissionRetryCount);

			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbacks.First());

			Assert.AreEqual(1, _feedSubmissionCallbacks.First().SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
		}

		[Test]
		public void MoveToRetryQueue_CalledMultipleTimes_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _feedSubmissionCallbacks.First().SubmissionRetryCount);

			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbacks.First());
			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbacks.First());
			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbacks.First());

			Assert.AreEqual(3, _feedSubmissionCallbacks.First().SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(3));
		}

		[Test]
		public void GetAllSubmittedFeedsFromQueue_ReturnsListOfSubmittedFeeds_ForGivenMerchant()
		{
			// Arrange
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var testMerchantId2 = "test merchant id 2";
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = testMerchantId2,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId1",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = testMerchantId2,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId2",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 4,
					FeedSubmissionId = "FeedSubmissionId3",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 5,
					FeedSubmissionId = "FeedSubmissionId4",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 6,
					FeedSubmissionId = null,
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 6,
					FeedSubmissionId = "FeedSubmissionId5",
					IsProcessingComplete = true
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					MerchantId = _merchantId,
					Id = 7,
					FeedSubmissionId = "FeedSubmissionId6",
					IsProcessingComplete = false
				}
			};

			_feedSubmissionCallbacks.AddRange(data);

			// Act
			var listSubmittedFeeds = _feedSubmissionProcessor.GetAllSubmittedFeedsFromQueue(_region, _merchantId);

			// Assert
			Assert.AreEqual(2, listSubmittedFeeds.Count());
			Assert.IsTrue(listSubmittedFeeds.Count(sf => sf.Id == 4 || sf.Id == 5) == 2);
		}

		[Test]
		public void GetAllSubmittedFeedsFromQueue_CalledWithNullMerchantId_ReturnsNull()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = _merchantId,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId1",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId2",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 2,
					FeedSubmissionId = "FeedSubmissionId3",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 3,
					FeedSubmissionId = "FeedSubmissionId4",
					IsProcessingComplete = false
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					MerchantId = null,
					Id = 4,
					FeedSubmissionId = "FeedSubmissionId5",
					IsProcessingComplete = true
				},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					MerchantId = null,
					Id = 5,
					FeedSubmissionId = "FeedSubmissionId6",
					IsProcessingComplete = false
				}
			};

			_feedSubmissionCallbacks.AddRange(data);

			// Act
			var listOfSubmittedFeeds = _feedSubmissionProcessor.GetAllSubmittedFeedsFromQueue(_region, null);

			// Assert
			Assert.IsEmpty(listOfSubmittedFeeds);

		}

		[Test]
		public void RequestFeedSubmissionStatusesFromAmazon_WithMultiplePendingReports_SubmitsAmazonRequest()
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
			var testRequestIdList = new List<string> {"Report1", "Report2", "Report3"};
			_marketplaceWebServiceClientMock.Setup(x => x.GetFeedSubmissionList(It.IsAny<GetFeedSubmissionListRequest>()))
				.Returns(getFeedSubmissionListResponse);

			var result = _feedSubmissionProcessor.RequestFeedSubmissionStatusesFromAmazon(testRequestIdList, "");

			Assert.AreEqual("_DONE_", result.First(x => x.FeedSubmissionId == "feed1").FeedProcessingStatus);
			Assert.AreEqual("_OTHER_", result.First(x => x.FeedSubmissionId == "feed3").FeedProcessingStatus);
			Assert.AreEqual("_CANCELLED_", result.First(x => x.FeedSubmissionId == "feed2").FeedProcessingStatus);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_MovesFeedToProcessingCompleteQueue_IfProcessingStatusIsDone()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					FeedSubmissionId = "testId",
					IsProcessingComplete = false,
					SubmissionRetryCount = 0
				}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_DONE_")
			};

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(resultsInfo);

			Assert.IsTrue(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

		[Test]
		public void
			QueueFeedsAccordingToProcessingStatus_LeavesFeedsInTheAwaitProcessingQueue_IfProcessingStatusIsAsExpected()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId1", IsProcessingComplete = false, SubmissionRetryCount = 0},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId2", IsProcessingComplete = false, SubmissionRetryCount = 0},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId3", IsProcessingComplete = false, SubmissionRetryCount = 0},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId4", IsProcessingComplete = false, SubmissionRetryCount = 0},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId5", IsProcessingComplete = false, SubmissionRetryCount = 0}
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

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId1").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId1").SubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId2").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId2").SubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId3").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId3").SubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId4").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId4").SubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId5").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId5").SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(5));
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_RemovesFeedFromDb_IfProcessingStatusIsCancelled()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId", IsProcessingComplete = false, SubmissionRetryCount = 0},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId2", IsProcessingComplete = false, SubmissionRetryCount = 0}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_CANCELLED_"),
			};

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_RemovesFeedFromDb_IfProcessingStatusIsUnknown()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId", IsProcessingComplete = false, SubmissionRetryCount = 0},
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {FeedSubmissionId = "testId2", IsProcessingComplete = false, SubmissionRetryCount = 0}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_SOME_MADE_UP_STATUS_")
			};
			 
			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").SubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionCallback>()), Times.Once);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

		[Test]
		public void Poll_DeletesFeedSubmissions_WithRetryCountAboveMaxRetryCount()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var testFeedSubmissionCallbacks = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {Id = 1, SubmissionRetryCount = 0, AmazonRegion = _region, MerchantId = _merchantId },
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {Id = 2, SubmissionRetryCount = 1, AmazonRegion = _region, MerchantId = _merchantId },
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {Id = 3, SubmissionRetryCount = 2, AmazonRegion = _region, MerchantId = _merchantId },
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {Id = 4, SubmissionRetryCount = 3, AmazonRegion = _region, MerchantId = _merchantId },
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {Id = 5, SubmissionRetryCount = 4, AmazonRegion = _region, MerchantId = _merchantId, FeedSubmissionId = null },
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer) {Id = 6, SubmissionRetryCount = 5, AmazonRegion = _region, MerchantId = _merchantId, FeedSubmissionId = "testFeedSubmissionId" }
			}.AsQueryable();
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testFeedSubmissionCallbacks);

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_region, _merchantId);
			_feedSubmissionCallbackServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(2));
		}
	}
}
