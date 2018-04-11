﻿using System;
using System.Collections.Generic;
using System.IO;
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
		private Mock<IFeedSubmissionCallbackService> _feedSubmissionCallbackServiceMock;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IFeedSubmissionProcessor> _feedSubmissionProcessorMock;
		private Mock<ICallbackActivator> _callbackActivatorMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private static bool _called;
		private readonly AmazonRegion _amazonRegion = AmazonRegion.Europe;
		private readonly string _merchantId = "testMerchantId1";

		[SetUp]
		public void SetUp()
		{
			var options = EasyMwsOptions.Defaults();
			_feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_feedSubmissionProcessorMock = new Mock<IFeedSubmissionProcessor>();
			_callbackActivatorMock = new Mock<ICallbackActivator>();
			_loggerMock = new Mock<IEasyMwsLogger>();

			_callbackActivatorMock.Setup(cam => cam.SerializeCallback(It.IsAny<Action<Stream, object>>(), It.IsAny<object>()))
				.Returns(new Callback("", "", "", ""));

			_feedProcessor = new FeedProcessor(_amazonRegion, _merchantId, options,
				_feedSubmissionCallbackServiceMock.Object, _marketplaceWebServiceClientMock.Object,
				_feedSubmissionProcessorMock.Object, _callbackActivatorMock.Object, _loggerMock.Object);
		}

		#region QueueFeed tests 

		[Test]
		public void QueueFeed_WithNullCallbackMethodArgument_NeverCallsLogError()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var callbackMethod = (Action<Stream, object>) null;

			_feedProcessor.Queue(propertiesContainer, callbackMethod, new { Foo = "Bar" });

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		[Test]
		public void QueueFeed_WithNullReportRequestPropertiesContainerArgument_ThrowsArgumentNullException()
		{
			FeedSubmissionPropertiesContainer propertiesContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_feedProcessor.Queue(propertiesContainer, callbackMethod, new { Foo = "Bar" });

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public void QueueFeed_WithNonEmptyArguments_CallsReportRequestCallbackServiceCreateOnceWithCorrectData()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
			FeedSubmissionCallback feedSubmissionCallback = null;
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<FeedSubmissionCallback>()))
				.Callback<FeedSubmissionCallback>((p) => { feedSubmissionCallback = p; });

			_feedProcessor.Queue(propertiesContainer, callbackMethod, new CallbackActivatorTests.CallbackDataTest {Foo = "Bar"});

			_feedSubmissionCallbackServiceMock.Verify(rrcsm => rrcsm.Create(It.IsAny<FeedSubmissionCallback>()), Times.Once);
			Assert.AreEqual(JsonConvert.SerializeObject(propertiesContainer), feedSubmissionCallback.FeedSubmissionData);
			Assert.AreEqual(AmazonRegion.Europe, feedSubmissionCallback.AmazonRegion);
			Assert.NotNull(feedSubmissionCallback.TypeName);
			Assert.NotNull(feedSubmissionCallback.Data);
			Assert.NotNull(feedSubmissionCallback.DataTypeName);
			Assert.NotNull(feedSubmissionCallback.MethodName);
		}

		[Test]
		public void QueueFeed_WithNonEmptyArguments_CallsReportRequestCallbackServiceSaveChangesOnce()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_feedProcessor.Queue(propertiesContainer, callbackMethod, new CallbackActivatorTests.CallbackDataTest {Foo = "Bar"});

			_feedSubmissionCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion


		#region PollFeeds tests 

		[Test]
		public void Poll_CallsOnce_GetNextFromQueueOfFeedsToSubmit()
		{
			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.GetNextFromQueueOfFeedsToSubmit(), Times.Once);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNull_DoesNotSubmitFeedToAmazon()
		{
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit())
				.Returns((FeedSubmissionCallback) null);

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.MoveToQueueOfSubmittedFeeds(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_DoesSubmitFeedToAmazon()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer));

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()), Times.Once);
		}

		[Test]
		public void
			Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_UpdatesLastSubmittedPropertyForProcessedFeedSubmission()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			FeedSubmissionCallback feedSubmissionCallback = null;

			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.Update(It.IsAny<FeedSubmissionCallback>()))
				.Callback((FeedSubmissionCallback arg) =>
				{
					feedSubmissionCallback = arg;
				});

			_feedProcessor.Poll();

			Assert.IsTrue(DateTime.UtcNow - feedSubmissionCallback.LastSubmitted < TimeSpan.FromHours(1));
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.AtLeastOnce);
			_feedSubmissionCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseNotNull_CallsOnce_MoveToQueueOfSubmittedFeeds()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns("testFeedSubmissionId");

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.MoveToQueueOfSubmittedFeeds(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseNull_CallsOnce_MoveToRetryQueue()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns((string) null);

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.MoveToRetryQueue(It.IsAny<FeedSubmissionCallback>()),
				Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseEmpty_CallsOnce_MoveToRetryQueue()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns(string.Empty);


			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.MoveToRetryQueue(It.IsAny<FeedSubmissionCallback>()),
				Times.Once);
		}

		[Test]
		public void Poll_WithGetNextFeedFromProcessingCompleteQueueReturningNull_NeverCalls_ExecuteCallbackOrMoveToRetryQueue()
		{
			var testStreamContent = "testStreamContent";
			var testStream = StreamHelper.CreateNewMemoryStream(testStreamContent);
			var notMatchingMd5Sum = "AAAAAAAAAAAAAAAA";
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent","testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionProcessorMock
				.Setup(fspm => fspm.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns("testSubmissionId");
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.GetFeedSubmissionResultFromAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns((testStream, notMatchingMd5Sum));
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.GetNextFromQueueOfProcessingCompleteFeeds())
				.Returns((FeedSubmissionCallback)null);

			_feedProcessor.Poll();

			_callbackActivatorMock.Verify(cam => cam.CallMethod(It.IsAny<Callback>(), It.IsAny<Stream>()), Times.Never);
			_feedSubmissionProcessorMock.Verify(fspm => fspm.RemoveFromQueue(It.IsAny<FeedSubmissionCallback>()), Times.Never);
			_feedSubmissionProcessorMock.Verify(fspm => fspm.MoveToRetryQueue(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseNotMatchingMd5_NeverCalls_ExecuteCallback()
		{
			var testStreamContent = "testStreamContent";
			var testStream = StreamHelper.CreateNewMemoryStream(testStreamContent);
			var notMatchingMd5Sum = "AAAAAAAAAAAAAAAA";
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionProcessorMock
				.Setup(fspm => fspm.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns("testSubmissionId");
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.GetFeedSubmissionResultFromAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns((testStream, notMatchingMd5Sum));
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.GetNextFromQueueOfProcessingCompleteFeeds())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer));

			_feedProcessor.Poll();

			_callbackActivatorMock.Verify(cam => cam.CallMethod(It.IsAny<Callback>(), It.IsAny<Stream>()), Times.Never);
			_feedSubmissionProcessorMock.Verify(fspm => fspm.RemoveFromQueue(It.IsAny<FeedSubmissionCallback>()), Times.Never);
			_feedSubmissionProcessorMock.Verify(fspm => fspm.MoveToRetryQueue(It.IsAny<FeedSubmissionCallback>()), Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseMatchingMd5_WithCallbackMethodProvided_CallsOnce_ExecuteCallback()
		{
			var testStreamContent = "testStreamContent";	// This is the content for which an MD5 value is computed and used in the test. Do not modify this without the MD5 value.
			var testStream = StreamHelper.CreateNewMemoryStream(testStreamContent);
			var matchingMd5Sum = "rD4TzLgdje+H2K2NattkqQ==";    // This is the MD5 value for testStreamContent="testStreamContent". Do not modify this without the stream content.
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			_feedSubmissionProcessorMock
				.Setup(fspm => fspm.GetNextFromQueueOfFeedsToSubmit())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.SubmitFeedToAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns("testSubmissionId");
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.GetFeedSubmissionResultFromAmazon(It.IsAny<FeedSubmissionCallback>()))
				.Returns((testStream, matchingMd5Sum));
			_feedSubmissionProcessorMock.Setup(fspm =>
					fspm.GetNextFromQueueOfProcessingCompleteFeeds())
				.Returns(new FeedSubmissionCallback(serializedPropertiesContainer) { MethodName = "testCallbackMethodName" });

			_feedProcessor.Poll();

			_callbackActivatorMock.Verify(cam => cam.CallMethod(It.IsAny<Callback>(), It.IsAny<Stream>()), Times.Once);
			_feedSubmissionProcessorMock.Verify(fspm => fspm.RemoveFromQueue(It.IsAny<FeedSubmissionCallback>()), Times.Once);
			_feedSubmissionProcessorMock.Verify(fspm => fspm.MoveToRetryQueue(It.IsAny<FeedSubmissionCallback>()), Times.Never);
		}

		#endregion

	}
}