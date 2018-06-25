using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
		private Mock<IFeedSubmissionEntryService> _feedSubmissionServiceMock;
		private List<FeedSubmissionEntry> _feedSubmissionCallbacks;
		private string _merchantId = "TestMerchantId";
		private Mock<IEasyMwsLogger> _loggerMock;
		private AmazonRegion _region = AmazonRegion.Europe;

		[SetUp]
		public void Setup()
		{
			_easyMwsOptions = EasyMwsOptions.Defaults();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_feedSubmissionServiceMock = new Mock<IFeedSubmissionEntryService>();
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

			_feedSubmissionServiceMock.Setup(x => x.GetAll()).Returns(_feedSubmissionCallbacks.AsQueryable());

			_feedSubmissionServiceMock.Setup(x => x.Where(It.IsAny<Expression<Func<FeedSubmissionEntry, bool>>>()))
				.Returns((Expression<Func<FeedSubmissionEntry, bool>> e) => _feedSubmissionCallbacks.AsQueryable().Where(e));

			_feedSubmissionServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<FeedSubmissionEntry, bool>>>()))
				.Returns((Expression<Func<FeedSubmissionEntry, bool>> e) =>
					_feedSubmissionCallbacks.AsQueryable().FirstOrDefault(e));
		}

		[Test]
		public void SubmitFeedToAmazon_CalledWithNullFeedSubmissionCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, null));
		}

		[Test]
		public void SubmitFeedToAmazon_CalledWithNullFeedContent_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry{Details = new FeedSubmissionDetails{FeedContent = null}}));
		}

		[Test]
		public void SubmitFeedToAmazon_CalledWithNullFeedSubmissionData_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry { Details = new FeedSubmissionDetails { FeedContent = new byte[1] }, FeedSubmissionData = null}));
		}

		[Test]
		public void SubmitFeedToAmazon_CalledWithNullFeedType_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentException>(() =>
				_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry { Details = new FeedSubmissionDetails { FeedContent = new byte[1] }, FeedSubmissionData = "", FeedType = null}));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestCallbackWithNullFeedSubmissionData_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => _feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(null)));
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

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionServiceMock.Object, resultsInfo);

			Assert.IsTrue(_feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").IsProcessingComplete);
			Assert.AreEqual(0, _feedSubmissionCallbacks.First(x => x.FeedSubmissionId == "testId").FeedSubmissionRetryCount);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
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

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionServiceMock.Object, resultsInfo);

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
			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Exactly(5));
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_IfProcessingStatusIsCancelled_IncrementsFeedSubmissionRetryCountAndResetsFeedSubmissionId()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 555, FeedSubmissionId = "testId", IsProcessingComplete = false, FeedProcessingRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 557, FeedSubmissionId = "testId2", IsProcessingComplete = false, FeedProcessingRetryCount = 0}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_CANCELLED_"),
			};

			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionServiceMock.Object, resultsInfo);

			Assert.IsNull(_feedSubmissionCallbacks.First(x => x.Id == 555).FeedSubmissionId);
			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.Id == 555).IsProcessingComplete);
			Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.Id == 555).FeedProcessingRetryCount);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void QueueFeedsAccordingToProcessingStatus_IfProcessingStatusIsUnknown_IncrementsFeedSubmissionRetryCount()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<FeedSubmissionEntry>
			{
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 555, FeedSubmissionId = "testId", IsProcessingComplete = false, FeedProcessingRetryCount = 0},
				new FeedSubmissionEntry(serializedPropertiesContainer) {Id = 557, FeedSubmissionId = "testId2", IsProcessingComplete = false, FeedProcessingRetryCount = 0}
			};

			_feedSubmissionCallbacks.AddRange(data);

			var resultsInfo = new List<(string FeedSubmissionId, string FeedProcessingStatus)>
			{
				("testId", "_SOME_MADE_UP_STATUS_")
			};
			 
			_feedSubmissionProcessor.QueueFeedsAccordingToProcessingStatus(_feedSubmissionServiceMock.Object, resultsInfo);

			Assert.IsFalse(_feedSubmissionCallbacks.First(x => x.Id == 555).IsProcessingComplete);
			Assert.AreEqual(1, _feedSubmissionCallbacks.First(x => x.Id == 555).FeedProcessingRetryCount);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void CleanUpFeedSubmissionQueue_DeletesFeedSubmissions_WithRetryCountAboveMaxRetryCount()
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
			_feedSubmissionServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testFeedSubmissionCallbacks);

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			// Id=5 deleted - FeedSubmissionMaxRetryCount. Id=6 deleted - FeedResultFailedChecksumMaxRetryCount. Id=1,2 deleted - FeedSubmissionRequestEntryExpirationPeriod=2days exceeded.
			_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Exactly(4));
		}

		private const int RetryCountIsStrictlyBelowConfiguredRetryCountLimit = -2;
		private const int RetryCountIsBelowConfiguredRetryCountLimitEdge = -1;
		private const int RetryCountIsEqualToConfiguredRetryCountLimit = 0;
		private const int RetryCountIsAboveConfiguredRetryCountLimitEdge = 1;
		private const int RetryCountIsStrictlyAboveConfiguredRetryCountLimit = 2;

		[TestCase(RetryCountIsStrictlyBelowConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, false)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, true)]
		[TestCase(RetryCountIsStrictlyAboveConfiguredRetryCountLimit, true)]
		public void CleanUpFeedSubmissionQueue_OneEntryWithSubmissionRetryContExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, bool shouldEntryBeDeleted)
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -2 ? 0 :
				retryCountType == -1 ? _easyMwsOptions.FeedSubmissionMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.FeedSubmissionMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.FeedSubmissionMaxRetryCount + 1 :
				retryCountType == 2 ? _easyMwsOptions.FeedSubmissionMaxRetryCount + 3 : 0;

			var firstEntryToDelete = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = retryCount,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entryToLeaveIntact = new FeedSubmissionEntry
			{
				Id = 2, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entriesList = new List<FeedSubmissionEntry> { firstEntryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_feedSubmissionServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(entriesQueryable);
			var entryBeingDeleted = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(x => x.Delete(It.IsAny<FeedSubmissionEntry>())).Callback<FeedSubmissionEntry>(
				e => { entryBeingDeleted = e; });

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			if (shouldEntryBeDeleted)
			{
				_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
				Assert.NotNull(entryBeingDeleted);
				Assert.AreEqual(firstEntryToDelete.Id, entryBeingDeleted.Id);
			}
			else { _feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never); }

			_feedSubmissionServiceMock.Verify(x => x.SaveChanges(), Times.Once);
		}

		[TestCase(RetryCountIsStrictlyBelowConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, false)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, true)]
		[TestCase(RetryCountIsStrictlyAboveConfiguredRetryCountLimit, true)]
		public void CleanUpFeedSubmissionQueue_OneEntryWithProcessingRetryContExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, bool shouldEntryBeDeleted)
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -2 ? 0 :
				retryCountType == -1 ? _easyMwsOptions.FeedProcessingMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.FeedProcessingMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.FeedProcessingMaxRetryCount + 1 :
				retryCountType == 2 ? _easyMwsOptions.FeedProcessingMaxRetryCount + 3 : 0;

			var firstEntryToDelete = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = retryCount,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entryToLeaveIntact = new FeedSubmissionEntry
			{
				Id = 2, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entriesList = new List<FeedSubmissionEntry> { firstEntryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_feedSubmissionServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(entriesQueryable);
			var entryBeingDeleted = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(x => x.Delete(It.IsAny<FeedSubmissionEntry>())).Callback<FeedSubmissionEntry>(
				e => { entryBeingDeleted = e; });

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			if (shouldEntryBeDeleted)
			{
				_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
				Assert.NotNull(entryBeingDeleted);
				Assert.AreEqual(firstEntryToDelete.Id, entryBeingDeleted.Id);
			}
			else { _feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never); }

			_feedSubmissionServiceMock.Verify(x => x.SaveChanges(), Times.Once);
		}

		[TestCase(RetryCountIsStrictlyBelowConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, false)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, true)]
		[TestCase(RetryCountIsStrictlyAboveConfiguredRetryCountLimit, true)]
		public void CleanUpFeedSubmissionQueue_OneEntryWithDownloadRetryContExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, bool shouldEntryBeDeleted)
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -2 ? 0 :
				retryCountType == -1 ? _easyMwsOptions.ReportDownloadMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.ReportDownloadMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.ReportDownloadMaxRetryCount + 1 :
				retryCountType == 2 ? _easyMwsOptions.ReportDownloadMaxRetryCount + 3 : 0;

			var firstEntryToDelete = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = retryCount,
				InvokeCallbackRetryCount = 0
			};
			var entryToLeaveIntact = new FeedSubmissionEntry
			{
				Id = 2, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entriesList = new List<FeedSubmissionEntry> { firstEntryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_feedSubmissionServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(entriesQueryable);
			var entryBeingDeleted = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(x => x.Delete(It.IsAny<FeedSubmissionEntry>())).Callback<FeedSubmissionEntry>(
				e => { entryBeingDeleted = e; });

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			if (shouldEntryBeDeleted)
			{
				_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
				Assert.NotNull(entryBeingDeleted);
				Assert.AreEqual(firstEntryToDelete.Id, entryBeingDeleted.Id);
			}
			else { _feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never); }

			_feedSubmissionServiceMock.Verify(x => x.SaveChanges(), Times.Once);
		}

		[TestCase(RetryCountIsStrictlyBelowConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, false)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, false)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, true)]
		[TestCase(RetryCountIsStrictlyAboveConfiguredRetryCountLimit, true)]
		public void CleanUpFeedSubmissionQueue_OneEntryWithInvokeCallbackRetryContExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, bool shouldEntryBeDeleted)
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -2 ? 0 :
				retryCountType == -1 ? _easyMwsOptions.InvokeCallbackMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.InvokeCallbackMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.InvokeCallbackMaxRetryCount + 1 :
				retryCountType == 2 ? _easyMwsOptions.InvokeCallbackMaxRetryCount + 3 : 0;

			var firstEntryToDelete = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = retryCount
			};
			var entryToLeaveIntact = new FeedSubmissionEntry
			{
				Id = 2, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entriesList = new List<FeedSubmissionEntry> { firstEntryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_feedSubmissionServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(entriesQueryable);
			var entryBeingDeleted = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(x => x.Delete(It.IsAny<FeedSubmissionEntry>())).Callback<FeedSubmissionEntry>(
				e => { entryBeingDeleted = e; });

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			if (shouldEntryBeDeleted)
			{
				_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
				Assert.NotNull(entryBeingDeleted);
				Assert.AreEqual(firstEntryToDelete.Id, entryBeingDeleted.Id);
			}
			else { _feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never); }

			_feedSubmissionServiceMock.Verify(x => x.SaveChanges(), Times.Once);
		}

		[TestCase(false, false, false, false, false)]
		[TestCase(true, true, true, true, true)]
		[TestCase(true, true, false, false, false)]
		[TestCase(true, false, true, false, false)]
		[TestCase(true, false, false, true, false)]
		[TestCase(true, false, false, false, true)]
		[TestCase(false, true, true, false, false)]
		[TestCase(false, true, false, true, false)]
		[TestCase(false, true, false, false, true)]
		[TestCase(false, false, true, true, false)]
		[TestCase(false, false, true, false, true)]
		[TestCase(false, false, false, true, true)]
		[TestCase(true, true, true, false, false)]
		[TestCase(true, true, false, true, false)]
		[TestCase(true, true, false, false, true)]
		[TestCase(true, false, true, true, false)]
		[TestCase(true, false, true, false, true)]
		[TestCase(true, false, false, true, true)]
		[TestCase(false, true, true, true, false)]
		[TestCase(false, true, true, false, true)]
		[TestCase(false, false, true, true, true)]
		[TestCase(true, true, true, true, false)]
		[TestCase(true, true, true, false, true)]
		[TestCase(true, true, false, true, true)]
		[TestCase(true, false, true, true, true)]
		[TestCase(false, true, true, true, true)]
		public void CleanUpFeedSubmissionQueue_WithPotentiallyMultiplesEntriesToDelete_DeletesOnlyTheCorrectEntries(
			bool hasEntryWithSubmissionRetryCountExceeded,
			bool hasEntryWithDownloadRetryCountExceeded,
			bool hasEntryWithCallbackRetryCountExceeded,
			bool hasEntryWithProcessRetryCountExceeded,
			bool hasEntryWithExpirationPeriodExceeded)
		{
			// arrange - This test intends to test the behavior of the Cleanup method for feed entries
			// not just by testing every cleanup scenario executed by the method - in isolation -, 
			// but by asserting that potentially unwanted interactions don't happen between any of the cleanup scenarios
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var data = JsonConvert.SerializeObject(propertiesContainer);
			var entryToLeaveIntact = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entryWithRequestRetryCountExceeded = new FeedSubmissionEntry { Id = 2, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 0, FeedProcessingRetryCount = 0, InvokeCallbackRetryCount = 0, FeedSubmissionRetryCount = 10 };
			var entryWithDownloadRetryCountExceeded = new FeedSubmissionEntry { Id = 3, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 10, FeedProcessingRetryCount = 0, InvokeCallbackRetryCount = 0, FeedSubmissionRetryCount = 0 };
			var entryWithCallbackRetryCountExceeded = new FeedSubmissionEntry { Id = 4, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 0, FeedProcessingRetryCount = 0, InvokeCallbackRetryCount = 10, FeedSubmissionRetryCount = 0 };
			var entryWithProcessRetryCountExceeded = new FeedSubmissionEntry { Id = 5, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 0, FeedProcessingRetryCount = 10, InvokeCallbackRetryCount = 0, FeedSubmissionRetryCount = 0 };
			var entryWithExpirationPeriodExceeded = new FeedSubmissionEntry { Id = 6, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow.AddDays(-10), ReportDownloadRetryCount = 0, FeedProcessingRetryCount = 0, InvokeCallbackRetryCount = 0, FeedSubmissionRetryCount = 0 };

			var expectedNumberOfEntitiesToDelete = 0;
			var entriesList = new List<FeedSubmissionEntry> { entryToLeaveIntact };
			if (hasEntryWithSubmissionRetryCountExceeded)
			{
				entriesList.Add(entryWithRequestRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithDownloadRetryCountExceeded)
			{
				entriesList.Add(entryWithDownloadRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithCallbackRetryCountExceeded)
			{
				entriesList.Add(entryWithCallbackRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithProcessRetryCountExceeded)
			{
				entriesList.Add(entryWithProcessRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithExpirationPeriodExceeded)
			{
				entriesList.Add(entryWithExpirationPeriodExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			var entriesQueryable = entriesList.AsQueryable();
			_feedSubmissionServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			var actualListOfDeletedEntries = new List<int>();
			var actualNumberOfDeletedEntries = 0;

			// mock the service Delete method, so that whenever it is called for an entry,
			// that entry's Id is added to a list - asserts will be made against the content of that list.
			_feedSubmissionServiceMock.Setup(x => x.Delete(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(entry => {
					actualListOfDeletedEntries.Add(entry.Id);
					actualNumberOfDeletedEntries++;
				});

			// act
			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			// assert that the Delete method is called the expected number of times 
			// and that the number of cleanup scenarios (or entries expected to be deleted) is equal to the number of invocations of the Delete method.
			Assert.AreEqual(expectedNumberOfEntitiesToDelete, actualNumberOfDeletedEntries);
			_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Exactly(expectedNumberOfEntitiesToDelete));
			
			// assert that the entry that has to be left intact is never used to invoke the Delete service method
			CollectionAssert.DoesNotContain(actualListOfDeletedEntries, entryToLeaveIntact.Id);

			// for each cleanup partial scenario taking place, assert that the actual list of entries for which Delete is invoked contains the id of the expected entry to be deleted for that scenario
			if (hasEntryWithSubmissionRetryCountExceeded) { CollectionAssert.Contains(actualListOfDeletedEntries, entryWithRequestRetryCountExceeded.Id); }
			if (hasEntryWithDownloadRetryCountExceeded) { CollectionAssert.Contains(actualListOfDeletedEntries, entryWithDownloadRetryCountExceeded.Id); }
			if (hasEntryWithCallbackRetryCountExceeded) { CollectionAssert.Contains(actualListOfDeletedEntries, entryWithCallbackRetryCountExceeded.Id); }
			if (hasEntryWithProcessRetryCountExceeded) { CollectionAssert.Contains(actualListOfDeletedEntries, entryWithProcessRetryCountExceeded.Id); }
			if (hasEntryWithExpirationPeriodExceeded) { CollectionAssert.Contains(actualListOfDeletedEntries, entryWithExpirationPeriodExceeded.Id); }

			_feedSubmissionServiceMock.Verify(x => x.SaveChanges(), Times.Once);
		}

		[Test]
		public void CleanUpFeedSubmissionQueue_WithMultipleReasonsToDeleteOneEntry_OnlyCallsDeleteOneSingleTime()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var data = JsonConvert.SerializeObject(propertiesContainer);
			var entryToDelete = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow.AddDays(-10),
				FeedSubmissionRetryCount = 10,
				FeedProcessingRetryCount = 10,
				ReportDownloadRetryCount = 10,
				InvokeCallbackRetryCount = 10
			};
			var entryToLeaveIntact = new FeedSubmissionEntry
			{
				Id = 1, FeedSubmissionData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				FeedSubmissionRetryCount = 0,
				FeedProcessingRetryCount = 0,
				ReportDownloadRetryCount = 0,
				InvokeCallbackRetryCount = 0
			};
			var entriesList = new List<FeedSubmissionEntry> { entryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_feedSubmissionServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);

			_feedSubmissionProcessor.CleanUpFeedSubmissionQueue(_feedSubmissionServiceMock.Object);

			_feedSubmissionServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(x => x.SaveChanges(), Times.Once);
		}

		[Test]
		public void SubmitFeedToAmazon_WithSubmitSingleQueuedFeedToAmazonResponseNotNull_UpdatesLastSubmittedDateAndFeedSubmissionIdAndResetsRetryCounter()
		{
			var feedContent = "testFeedContent";
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(feedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var expectedFeedSubmissionId = "testFeedSubmissionId";

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.SubmitFeed(It.IsAny<SubmitFeedRequest>()))
				.Returns(new SubmitFeedResponse{SubmitFeedResult = new SubmitFeedResult{FeedSubmissionInfo = new FeedSubmissionInfo{FeedSubmissionId = expectedFeedSubmissionId } }});
			var entryBeingUpdated = (FeedSubmissionEntry) null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = GenerateValidArchive(feedContent).ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.AreEqual(expectedFeedSubmissionId, entryBeingUpdated.FeedSubmissionId);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(0, entryBeingUpdated.FeedSubmissionRetryCount);
		}

		[Test]
		public void SubmitFeedToAmazon_WithSubmitSingleQueuedFeedToAmazonResponseNull_UpdatesLastSubmittedDateAndRetryCounter()
		{
			var feedContent = "testFeedContent";
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(feedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.SubmitFeed(It.IsAny<SubmitFeedRequest>()))
				.Returns(new SubmitFeedResponse { SubmitFeedResult = new SubmitFeedResult { FeedSubmissionInfo = new FeedSubmissionInfo { FeedSubmissionId = null } } });
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = GenerateValidArchive(feedContent).ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.IsNull(entryBeingUpdated.FeedSubmissionId);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.FeedSubmissionRetryCount);
		}

		[Test]
		public void SubmitFeedToAmazon_WithSubmitSingleQueuedFeedToAmazonResponseEmpty_UpdatesLastSubmittedDateAndRetryCounter()
		{
			var feedContent = "testFeedContent";
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(feedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.SubmitFeed(It.IsAny<SubmitFeedRequest>()))
				.Returns(new SubmitFeedResponse { SubmitFeedResult = new SubmitFeedResult { FeedSubmissionInfo = new FeedSubmissionInfo { FeedSubmissionId = string.Empty } } });
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object,
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					LastSubmitted = DateTime.MinValue,
					FeedType = feedType,
					Details = new FeedSubmissionDetails {FeedContent = GenerateValidArchive(feedContent).ToArray()}
				});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.IsNull(entryBeingUpdated.FeedSubmissionId);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.FeedSubmissionRetryCount);
		}

		[TestCase("AccessToFeedProcessingResultDenied")]
		[TestCase("FeedCanceled")]
		[TestCase("FeedProcessingResultNoLongerAvailable")]
		[TestCase("InputDataError")]
		[TestCase("InvalidFeedType")]
		[TestCase("InvalidRequest")]
		public void SubmitFeedToAmazon_WithSubmitSingleQueuedFeedToAmazonThrowsFatalErrorCodeException_DeletesEntryFromQueue(string fatalErrorCode)
		{
			var feedContent = "testFeedContent";
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(feedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.SubmitFeed(It.IsAny<SubmitFeedRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, fatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));

			_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object,
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					LastSubmitted = DateTime.MinValue,
					FeedType = feedType,
					Details = new FeedSubmissionDetails { FeedContent = GenerateValidArchive(feedContent).ToArray() }
				});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
		}

		[TestCase("ContentMD5Missing")]
		[TestCase("ContentMD5DoesNotMatch")]
		[TestCase("FeedProcessingResultNotReady")]
		[TestCase("InvalidFeedSubmissionId")]
		[TestCase("SomeUnhandledNewErrorCode")]
		public void SubmitFeedToAmazon_WithSubmitSingleQueuedFeedToAmazonThrowsNonFatalErrorCodeException_UpdatesLastSubmittedDateAndRetryCounter(string nonFatalErrorCode)
		{
			var feedContent = "testFeedContent";
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(feedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.SubmitFeed(It.IsAny<SubmitFeedRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, nonFatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object,
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					LastSubmitted = DateTime.MinValue,
					FeedType = feedType,
					Details = new FeedSubmissionDetails { FeedContent = GenerateValidArchive(feedContent).ToArray() }
				});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.IsNull(entryBeingUpdated.FeedSubmissionId);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.FeedSubmissionRetryCount);
		}

		[Test]
		public void SubmitFeedToAmazon_WithSubmitSingleQueuedFeedToAmazonThrowingNonMarketplaceWebServiceException_UpdatesLastSubmittedDateAndRetryCounter()
		{
			var feedContent = "testFeedContent";
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(feedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.SubmitFeed(It.IsAny<SubmitFeedRequest>()))
				.Throws(new Exception(""));
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.SubmitFeedToAmazon(_feedSubmissionServiceMock.Object,
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					LastSubmitted = DateTime.MinValue,
					FeedType = feedType,
					Details = new FeedSubmissionDetails { FeedContent = GenerateValidArchive(feedContent).ToArray() }
				});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.IsNull(entryBeingUpdated.FeedSubmissionId);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.FeedSubmissionRetryCount);
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithNullFeedSubmissionArgument_ThrowsNullArgumentException()
		{
			Assert.Throws<ArgumentNullException>(() =>
				_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, null));
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithNullMerchantId_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
				_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry{MerchantId = null}));
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithEmptyMerchantId_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
				_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry { MerchantId = string.Empty }));
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithEmptyFeedSubmissionId_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
				_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry { MerchantId = _merchantId, FeedSubmissionId = string.Empty}));
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithNullFeedSubmissionId_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() =>
				_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry { MerchantId = _merchantId, FeedSubmissionId = null }));
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithGetFeedSubmissionResultResponseWithMatchingMd5Hash_UpdatesLastSubmittedDateAndSavesReportAsZipFileAndResetsRetryCounter()
		{
			var expectedContent = "This is some test content. Und die Katze läuft auf der Straße.";
			var expectedContentStream = StreamHelper.CreateMemoryStream(expectedContent);
			var expectedContentStreamHash = MD5ChecksumHelper.ComputeHashForAmazon(expectedContentStream);
			var expectedZippedContent = GenerateValidArchive(expectedContent);
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(expectedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var expectedFeedSubmissionId = "testFeedSubmissionId";
			var expectedMd5Hash = expectedContentStreamHash;
			var streamBeingSentToAmazon = (MemoryStream)null;

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.GetFeedSubmissionResult(It.IsAny<GetFeedSubmissionResultRequest>()))
				.Returns(new GetFeedSubmissionResultResponse
				{
					GetFeedSubmissionResultResult = new GetFeedSubmissionResultResult {ContentMD5 = expectedMd5Hash}
				})
				.Callback<GetFeedSubmissionResultRequest>(req =>
				{
					expectedContentStream.CopyTo(req.FeedSubmissionResult);
					expectedContentStream.Position = 0;

					streamBeingSentToAmazon = new MemoryStream();
					req.FeedSubmissionResult.CopyTo(streamBeingSentToAmazon);
					req.FeedSubmissionResult.Position = 0;
					streamBeingSentToAmazon.Position = 0;
				});
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				MerchantId = _merchantId,
				FeedSubmissionId = expectedFeedSubmissionId,
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = expectedZippedContent.ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(streamBeingSentToAmazon);
			Assert.NotNull(entryBeingUpdated);
			Assert.IsNull(entryBeingUpdated.Details.FeedContent);
			Assert.NotNull(entryBeingUpdated.Details.FeedSubmissionReport);
			using (var actualReportReader = new StreamReader(ExtractArchivedSingleFileToStream(entryBeingUpdated.Details.FeedSubmissionReport)))
			{
				Assert.AreEqual(expectedContent, actualReportReader.ReadToEnd());
			}
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(0, entryBeingUpdated.ReportDownloadRetryCount);
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithGetFeedSubmissionResultResponseWithNonMatchingMd5Hash_UpdatesLastSubmittedDateAndIncrementsRetryCounter()
		{
			var expectedContent = "This is some test content. Und die Katze läuft auf der Straße.";
			var expectedContentStream = StreamHelper.CreateMemoryStream(expectedContent);
			var expectedContentStreamHash = MD5ChecksumHelper.ComputeHashForAmazon(expectedContentStream);
			var expectedZippedContent = GenerateValidArchive(expectedContent);
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(expectedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var expectedFeedSubmissionId = "testFeedSubmissionId";
			var expectedMd5Hash = expectedContentStreamHash;

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.GetFeedSubmissionResult(It.IsAny<GetFeedSubmissionResultRequest>()))
				.Returns(new GetFeedSubmissionResultResponse
				{
					GetFeedSubmissionResultResult = new GetFeedSubmissionResultResult {ContentMD5 = $"{expectedMd5Hash}NonMatchingSequence"}
				});
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				MerchantId = _merchantId,
				FeedSubmissionId = expectedFeedSubmissionId,
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = expectedZippedContent.ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.NotNull(entryBeingUpdated.Details.FeedContent);
			Assert.IsNull(entryBeingUpdated.Details.FeedSubmissionReport);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.ReportDownloadRetryCount);
		}

		[TestCase("ContentMD5Missing")]
		[TestCase("ContentMD5DoesNotMatch")]
		[TestCase("FeedProcessingResultNotReady")]
		[TestCase("InvalidFeedSubmissionId")]
		[TestCase("SomeUnhandledNewErrorCode")]
		public void DownloadFeedSubmissionResultFromAmazon_WithGetFeedSubmissionResultThrowingNonFatalErrorCodeException_UpdatesLastSubmittedDateAndIncrementsRetryCounter(string nonFatalErrorCode)
		{
			var expectedContent = "This is some test content. Und die Katze läuft auf der Straße.";
			var expectedZippedContent = GenerateValidArchive(expectedContent);
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(expectedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var expectedFeedSubmissionId = "testFeedSubmissionId";

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.GetFeedSubmissionResult(It.IsAny<GetFeedSubmissionResultRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, nonFatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				MerchantId = _merchantId,
				FeedSubmissionId = expectedFeedSubmissionId,
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = expectedZippedContent.ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.NotNull(entryBeingUpdated.Details.FeedContent);
			Assert.IsNull(entryBeingUpdated.Details.FeedSubmissionReport);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.ReportDownloadRetryCount);
		}

		[Test]
		public void DownloadFeedSubmissionResultFromAmazon_WithGetFeedSubmissionResultThrowingNonMarketplaceWebServiceException_UpdatesLastSubmittedDateAndIncrementsRetryCounter()
		{
			var expectedContent = "This is some test content. Und die Katze läuft auf der Straße.";
			var expectedZippedContent = GenerateValidArchive(expectedContent);
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(expectedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var expectedFeedSubmissionId = "testFeedSubmissionId";

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.GetFeedSubmissionResult(It.IsAny<GetFeedSubmissionResultRequest>()))
				.Throws(new Exception("some random exception"));
			var entryBeingUpdated = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingUpdated = entry; });

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				MerchantId = _merchantId,
				FeedSubmissionId = expectedFeedSubmissionId,
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = expectedZippedContent.ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingUpdated);
			Assert.NotNull(entryBeingUpdated.Details.FeedContent);
			Assert.IsNull(entryBeingUpdated.Details.FeedSubmissionReport);
			Assert.AreEqual(DateTime.UtcNow.Date, entryBeingUpdated.LastSubmitted.Date);
			Assert.AreEqual(1, entryBeingUpdated.ReportDownloadRetryCount);
		}

		[TestCase("AccessToFeedProcessingResultDenied")]
		[TestCase("FeedCanceled")]
		[TestCase("FeedProcessingResultNoLongerAvailable")]
		[TestCase("InputDataError")]
		[TestCase("InvalidFeedType")]
		[TestCase("InvalidRequest")]
		public void DownloadFeedSubmissionResultFromAmazon_WithGetFeedSubmissionResultThrowingFatalErrorCodeException_DeletesEntryFromQueue(string fatalErrorCode)
		{
			var expectedContent = "This is some test content. Und die Katze läuft auf der Straße.";
			var expectedZippedContent = GenerateValidArchive(expectedContent);
			var feedType = "testFeedType";
			var propertiesContainer = new FeedSubmissionPropertiesContainer(expectedContent, feedType);
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			var expectedFeedSubmissionId = "testFeedSubmissionId";

			_marketplaceWebServiceClientMock.Setup(rrp =>
					rrp.GetFeedSubmissionResult(It.IsAny<GetFeedSubmissionResultRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, fatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var entryBeingDeleted = (FeedSubmissionEntry)null;
			_feedSubmissionServiceMock.Setup(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()))
				.Callback<FeedSubmissionEntry>(
					entry => { entryBeingDeleted = entry; });

			_feedSubmissionProcessor.DownloadFeedSubmissionResultFromAmazon(_feedSubmissionServiceMock.Object, new FeedSubmissionEntry(serializedPropertiesContainer)
			{
				MerchantId = _merchantId,
				AmazonRegion = AmazonRegion.Australia,
				FeedSubmissionId = expectedFeedSubmissionId,
				LastSubmitted = DateTime.MinValue,
				FeedType = feedType,
				Details = new FeedSubmissionDetails { FeedContent = expectedZippedContent.ToArray() }
			});

			_feedSubmissionServiceMock.Verify(fscs => fscs.Update(It.IsAny<FeedSubmissionEntry>()), Times.Never);
			_feedSubmissionServiceMock.Verify(fscs => fscs.Delete(It.IsAny<FeedSubmissionEntry>()), Times.Once);
			_feedSubmissionServiceMock.Verify(fscs => fscs.SaveChanges(), Times.Once);
			Assert.NotNull(entryBeingDeleted);
			Assert.AreEqual((feedType, AmazonRegion.Australia, _merchantId), (entryBeingDeleted.FeedType, entryBeingDeleted.AmazonRegion, entryBeingDeleted.MerchantId));
		}

		private MemoryStream GenerateValidArchive(string content)
		{
			using (var zipFileStream = new MemoryStream())
			{
				using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, true))
				{
					var fileToArchive = archive.CreateEntry("testFilename.txt", CompressionLevel.Fastest);
					using (var fileStream = fileToArchive.Open())
					using (var streamWriter = new StreamWriter(fileStream))
					{
						streamWriter.Write(content);
					}
				}

				zipFileStream.Position = 0;
				return zipFileStream;
			}
		}

		private static MemoryStream ExtractArchivedSingleFileToStream(byte[] zipArchive)
		{
			if (zipArchive == null) return null;

			using (var archiveStream = new MemoryStream(zipArchive))
			using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read))
			{
				var file = zip.Entries.FirstOrDefault();
				var resultStream = new MemoryStream();
				file?.Open()?.CopyTo(resultStream);
				resultStream.Position = 0;
				return resultStream;
			}
		}
	}
}
