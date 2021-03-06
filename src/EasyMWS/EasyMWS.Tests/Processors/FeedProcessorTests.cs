﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
using Newtonsoft.Json;
using NUnit.Framework;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;

namespace EasyMWS.Tests.ReportProcessors
{
	public class FeedProcessorTests
	{
		private FeedProcessor _feedProcessor;
		private Mock<IFeedSubmissionEntryService> _feedSubmissionServiceMock;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IFeedSubmissionProcessor> _feedSubmissionProcessorMock;
		private Mock<ICallbackActivator> _callbackActivatorMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private static bool _called;
		private readonly AmazonRegion _amazonRegion = AmazonRegion.Europe;
		private readonly string _merchantId = "testMerchantId1";
        private readonly string _mwsAuthToken = "testMwsAuthToken";
        private bool MarkEntryAsHandled = true;

		[SetUp]
		public void SetUp()
		{
			var options = new EasyMwsOptions();
			_feedSubmissionServiceMock = new Mock<IFeedSubmissionEntryService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_feedSubmissionProcessorMock = new Mock<IFeedSubmissionProcessor>();
			_callbackActivatorMock = new Mock<ICallbackActivator>();
			_loggerMock = new Mock<IEasyMwsLogger>();

			_callbackActivatorMock.Setup(cam => cam.SerializeCallback(It.IsAny<Action<Stream, object>>(), It.IsAny<object>()))
				.Returns(new Callback("", "", "", ""));

			_feedProcessor = new FeedProcessor(_amazonRegion, _merchantId, _mwsAuthToken, options, _marketplaceWebServiceClientMock.Object,
				_feedSubmissionProcessorMock.Object, _callbackActivatorMock.Object, _loggerMock.Object);
		}

        #region QueueFeed tests 

        [Test]
        public void QueueFeed_WithNullTargetEventId_SavesFeedEntry_NeverCallsLogError()
        {
            var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
            var callbackMethod = (Action<Stream, object>)null;

            _feedProcessor.QueueFeed(_feedSubmissionServiceMock.Object, propertiesContainer, null, new Dictionary<string, object> { { "key", "value"} });

            _feedSubmissionServiceMock.Verify(rrcs => rrcs.Create(It.IsAny<FeedSubmissionEntry>()), Times.Once);
            _feedSubmissionServiceMock.Verify(rrcs => rrcs.SaveChanges(), Times.Once);
            _loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<ArgumentNullException>()), Times.Never);
        }

        [Test]
        public void QueueFeed_WithNullFeedSubmissionPropertiesContainerArgument_ThrowsArgumentNullException()
        {
            FeedSubmissionPropertiesContainer propertiesContainer = null;
            var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

            _feedProcessor.QueueFeed(_feedSubmissionServiceMock.Object, propertiesContainer);

            _loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }

        [Test]
        public void QueueFeed_WithNonEmptyArguments_CallsFeedSubmissionEntryServiceCreateOnceWithCorrectData()
        {
            var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
            var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
            FeedSubmissionEntry feedSubmissionEntry = null;
            _feedSubmissionServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<FeedSubmissionEntry>()))
                .Callback<FeedSubmissionEntry>((p) => { feedSubmissionEntry = p; });

            _feedProcessor.QueueFeed(_feedSubmissionServiceMock.Object, propertiesContainer, "TargetHandlerId", new Dictionary<string, object> { { "k1", "v1"}, { "k2", "v2" } });

            _feedSubmissionServiceMock.Verify(rrcsm => rrcsm.Create(It.IsAny<FeedSubmissionEntry>()), Times.Once);
            Assert.AreEqual(JsonConvert.SerializeObject(propertiesContainer), feedSubmissionEntry.FeedSubmissionData);
            Assert.AreEqual(AmazonRegion.Europe, feedSubmissionEntry.AmazonRegion);
            Assert.AreEqual("TargetHandlerId", feedSubmissionEntry.TargetHandlerId);
            Assert.AreEqual("{\"k1\":\"v1\",\"k2\":\"v2\"}", feedSubmissionEntry.TargetHandlerArgs);
        }

        //[Test]
        //public void QueueFeed_WithNonEmptyArguments_CallsFeedSubmissionEntryServiceSaveChangesOnce()
        //{
        //	var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
        //	var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

        //	_feedProcessor.QueueFeed(_feedSubmissionServiceMock.Object, propertiesContainer, callbackMethod, new CallbackActivatorTests.CallbackDataTest {Foo = "Bar"});

        //	_feedSubmissionServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
        //}

        #endregion


        #region PollFeeds tests 

        [Test]
		public void Poll_CallsOnce_GetNextFromQueueOfFeedsToSubmit()
		{
			_feedProcessor.PollFeeds(_feedSubmissionServiceMock.Object);

			_feedSubmissionServiceMock.Verify(
				rrp => rrp.GetNextFromQueueOfFeedsToSubmit(It.IsAny<string>(), It.IsAny<AmazonRegion>(), MarkEntryAsHandled), Times.Once);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNull_DoesNotSubmitFeedToAmazon()
		{
			_feedSubmissionServiceMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit(It.IsAny<string>(), It.IsAny<AmazonRegion>(), MarkEntryAsHandled))
				.Returns((FeedSubmissionEntry) null);

			_feedProcessor.PollFeeds(_feedSubmissionServiceMock.Object);

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.SubmitFeedToAmazon(It.IsAny<IFeedSubmissionEntryService>(),It.IsAny<FeedSubmissionEntry>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_DoesSubmitFeedToAmazon()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionServiceMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit(It.IsAny<string>(), It.IsAny<AmazonRegion>(), MarkEntryAsHandled))
				.Returns(new FeedSubmissionEntry(serializedPropertiesContainer));

			_feedProcessor.PollFeeds(_feedSubmissionServiceMock.Object);

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.SubmitFeedToAmazon(It.IsAny<IFeedSubmissionEntryService>(),It.IsAny<FeedSubmissionEntry>()), Times.Once);
		}

		[Test]
		public void RequestFeedSubmissionStatusesAllFromAmazon_GivenMultipleEntriesReadyToHaveTheirStatusUpdatedFromAmazon_ThenAttemptToUpdateTheStatusOfAllSuchEntries()
		{
			// arrange
			var numberOfEntriesToBeUpdated = 176;
			var expectedBatches = 176 / 20 + 1;
			var entriesToBeUpdated = new List<FeedSubmissionEntry>();
            for (int i = 0; i < numberOfEntriesToBeUpdated; i++)
            {
				entriesToBeUpdated.Add(GetNewEntryReadyToHaveItsStatusUpdatedFromAmazon($"EntryToBeUpdated_SubmissionId_{i}"));
			}

			_feedSubmissionServiceMock
				.Setup(_ => _.GetAllSubmittedFeedsFromQueue(It.IsAny<string>(), It.IsAny<AmazonRegion>(), It.IsAny<bool>()))
				.Returns(entriesToBeUpdated);

			var entriesMarkedAsLocked = new List<FeedSubmissionEntry>();
			_feedSubmissionServiceMock
				.Setup(_ => _.LockMultipleEntries(It.IsAny<List<FeedSubmissionEntry>>(), It.IsAny<string>()))
				.Callback<List<FeedSubmissionEntry>, string>((entriesMarkedAsLockedArg, reasonArg) => 
				{
					entriesMarkedAsLocked.AddRange(entriesMarkedAsLockedArg);
				});

			var entriesMarkedAsUnlocked = new List<FeedSubmissionEntry>();
			_feedSubmissionServiceMock
				.Setup(_ => _.UnlockMultipleEntries(It.IsAny<List<FeedSubmissionEntry>>(), It.IsAny<string>()))
				.Callback<List<FeedSubmissionEntry>, string>((entriesMarkedAsUnlockedArg, reasonArg) =>
				{
					entriesMarkedAsUnlocked.AddRange(entriesMarkedAsUnlockedArg);
				});

			var entriesHavingTheirStatusUpdateRequested = new List<string>();
			_feedSubmissionProcessorMock
				.Setup(_ => _.RequestFeedSubmissionStatusesFromAmazon(It.IsAny<IFeedSubmissionEntryService>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
				.Callback<IFeedSubmissionEntryService, IEnumerable<string>, string>((fseService, feedSubmissionsToUpdate, merchandId) => 
				{
					entriesHavingTheirStatusUpdateRequested.AddRange(feedSubmissionsToUpdate);
				});

			// act
			_feedProcessor.RequestFeedSubmissionStatusesAllFromAmazon(_feedSubmissionServiceMock.Object);

			// assert
			Assert.IsNotEmpty(entriesMarkedAsLocked);
			Assert.IsTrue(entriesMarkedAsLocked.All(actualLockedEntry => entriesToBeUpdated.SingleOrDefault(givenEntry => givenEntry.FeedSubmissionId == actualLockedEntry.FeedSubmissionId) != null));

			Assert.IsNotEmpty(entriesMarkedAsUnlocked);
			Assert.IsTrue(entriesMarkedAsUnlocked.All(actualUnlockedEntry => entriesToBeUpdated.SingleOrDefault(givenEntry => givenEntry.FeedSubmissionId == actualUnlockedEntry.FeedSubmissionId) != null));

			_feedSubmissionProcessorMock.Verify(_ => _.RequestFeedSubmissionStatusesFromAmazon(It.IsAny<IFeedSubmissionEntryService>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()), Times.Exactly(expectedBatches));

			Assert.IsNotEmpty(entriesHavingTheirStatusUpdateRequested);
			Assert.IsTrue(entriesHavingTheirStatusUpdateRequested.All(actualUpdatedSubmissionId => entriesToBeUpdated.SingleOrDefault(givenEntry => givenEntry.FeedSubmissionId == actualUpdatedSubmissionId) != null));
		}

		private FeedSubmissionEntry GetNewEntryReadyToHaveItsStatusUpdatedFromAmazon(string feedSubmissionId = null)
			=> new FeedSubmissionEntry
			{
				AmazonRegion = _amazonRegion,
				MerchantId = _merchantId,
				FeedSubmissionId = feedSubmissionId ?? Guid.NewGuid().ToString(),
				IsProcessingComplete = false,
				IsLocked = false
			};
		
		#endregion
	}
}