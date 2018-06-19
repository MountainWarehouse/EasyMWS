using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.CallbackLogic;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
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
		private Mock<IFeedSubmissionEntryService> _feedSubmissionCallbackServiceMock;
		private List<FeedSubmissionEntry> _feedSubmissionCallbacks;
		private string _merchantId = "TestMerchantId";
		private Mock<IEasyMwsLogger> _loggerMock;
		private AmazonRegion _region = AmazonRegion.Europe;

		[SetUp]
		public void Setup()
		{
			_easyMwsOptions = EasyMwsOptions.Defaults();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionEntryService>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_feedSubmissionProcessor = new FeedSubmissionProcessor(_region, _merchantId,_marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);

			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionCallbacks = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 1,
					FeedSubmissionId = "testSubmissionId1",
				}
			};

			_feedSubmissionCallbackServiceMock.Setup(x => x.GetAll()).Returns(_feedSubmissionCallbacks.AsQueryable());

			_feedSubmissionCallbackServiceMock.Setup(x => x.Where(It.IsAny<Expression<Func<FeedSubmissionEntry, bool>>>()))
				.Returns((Expression<Func<FeedSubmissionEntry, bool>> e) => _feedSubmissionCallbacks.AsQueryable().Where(e));

			_feedSubmissionCallbackServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<FeedSubmissionEntry, bool>>>()))
				.Returns((Expression<Func<FeedSubmissionEntry, bool>> e) =>
					_feedSubmissionCallbacks.AsQueryable().FirstOrDefault(e));
		}

		[Test]
		public void SubmitFeedToAmazon_CalledWithNullFeedSubmissionCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionCallbackServiceMock.Object, null));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestCallbackWithNullFeedSubmissionData_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => _feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionCallbackServiceMock.Object, new FeedSubmissionEntry(null)));
		}

		[Test]
		public void MoveToQueueOfSubmittedFeeds_UpdatesFeedSubmissionId_OnTheCallback()
		{
			var testFeedSubmissionId = "testFeedSubmissionId";

			_feedSubmissionProcessor.MoveToQueueOfSubmittedFeeds(_feedSubmissionCallbackServiceMock.Object, _feedSubmissionCallbacks[0], testFeedSubmissionId);

			Assert.AreEqual("testFeedSubmissionId", _feedSubmissionCallbacks[0].FeedSubmissionId);
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
		}

		[Test]
		public void MoveToRetryQueue_CalledOnce_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _feedSubmissionCallbacks.First().FeedSubmissionRetryCount);

			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbackServiceMock.Object, _feedSubmissionCallbacks.First());

			Assert.AreEqual(1, _feedSubmissionCallbacks.First().FeedSubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
		}

		[Test]
		public void MoveToRetryQueue_CalledMultipleTimes_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _feedSubmissionCallbacks.First().FeedSubmissionRetryCount);

			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbackServiceMock.Object, _feedSubmissionCallbacks.First());
			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbackServiceMock.Object, _feedSubmissionCallbacks.First());
			_feedSubmissionProcessor.MoveToRetryQueue(_feedSubmissionCallbackServiceMock.Object, _feedSubmissionCallbacks.First());

			Assert.AreEqual(3, _feedSubmissionCallbacks.First().FeedSubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionEntry>()), Times.Exactly(3));
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
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					FeedSubmissionId = "testId",
					IsProcessingComplete = false,
					FeedSubmissionRetryCount = 0
				}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_DONE_")
			};

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionCallbackServiceMock.Object, resultsInfo);

			Assert.IsTrue(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").FeedSubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void
			QueueFeedsAccordingToProcessingStatus_LeavesFeedsInTheAwaitProcessingQueue_IfProcessingStatusIsAsExpected()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId1", IsProcessingComplete = false, FeedSubmissionRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId2", IsProcessingComplete = false, FeedSubmissionRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId3", IsProcessingComplete = false, FeedSubmissionRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId4", IsProcessingComplete = false, FeedSubmissionRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId5", IsProcessingComplete = false, FeedSubmissionRetryCount = 0}
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

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionCallbackServiceMock.Object, resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId1").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId1").FeedSubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId2").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId2").FeedSubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId3").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId3").FeedSubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId4").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId4").FeedSubmissionRetryCount);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId5").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId5").FeedSubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Exactly(5));
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_RemovesFeedFromDb_IfProcessingStatusIsCancelled()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId", IsProcessingComplete = false, FeedSubmissionRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId2", IsProcessingComplete = false, FeedSubmissionRetryCount = 0}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_CANCELLED_"),
			};

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionCallbackServiceMock.Object, resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").FeedSubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_RemovesFeedFromDb_IfProcessingStatusIsUnknown()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId", IsProcessingComplete = false, FeedSubmissionRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {FeedSubmissionId = "testId2", IsProcessingComplete = false, FeedSubmissionRetryCount = 0}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_SOME_MADE_UP_STATUS_")
			};
			 
			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionCallbackServiceMock.Object, resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").FeedSubmissionRetryCount);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionCallbackServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void Poll_DeletesFeedSubmissions_WithRetryCountAboveMaxRetryCount()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var testFeedSubmissionCallbacks = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 1, FeedSubmissionRetryCount = 0, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-3) },
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 2, FeedSubmissionRetryCount = 1, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-2).AddHours(-1) },
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 3, FeedSubmissionRetryCount = 2, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-2).AddHours(1) },
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 4, FeedSubmissionRetryCount = 3, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-1) },
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 5, FeedSubmissionRetryCount = 5, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-1), FeedSubmissionId = null },
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 6, FeedSubmissionRetryCount = 5, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-1), FeedSubmissionId = "testFeedSubmissionId" }
			}.AsQueryable();
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testFeedSubmissionCallbacks);

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionCallbackServiceMock.Object);

			// Id=5 deleted - FeedSubmissionMaxRetryCount. Id=6 deleted - FeedResultFailedChecksumMaxRetryCount. Id=1,2 deleted - FeedSubmissionRequestEntryExpirationPeriod=2days exceeded.
			_feedSubmissionCallbackServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Exactly(4));
		}
	}
}
