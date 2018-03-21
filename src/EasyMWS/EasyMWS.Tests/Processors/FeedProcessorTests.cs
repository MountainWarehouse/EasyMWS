using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
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
		private readonly int ConfiguredMaxNumberOrReportRequestRetries = 2;
		private readonly int ConfiguredMaxNumberOrFeedSubmissionRetries = 2;
		private Mock<IFeedSubmissionCallbackService> _feedSubmissionCallbackServiceMock;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IFeedSubmissionProcessor> _feedSubmissionProcessorMock;
		private static bool _called;

		[SetUp]
		public void SetUp()
		{
			var options = EasyMwsOptions.Defaults;
			options.ReportRequestMaxRetryCount = ConfiguredMaxNumberOrReportRequestRetries;
			options.FeedSubmissionMaxRetryCount = ConfiguredMaxNumberOrFeedSubmissionRetries;

			_feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_feedSubmissionProcessorMock = new Mock<IFeedSubmissionProcessor>();

			_feedProcessor = new FeedProcessor(AmazonRegion.Europe, "testMerchantId1", options,
				_feedSubmissionCallbackServiceMock.Object, _marketplaceWebServiceClientMock.Object,
				_feedSubmissionProcessorMock.Object);
		}

		#region QueueFeed tests 

		[Test]
		public void QueueFeed_WithNullCallbackMethodArgument_ThrowsArgumentNullException()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("", "");
			var callbackMethod = (Action<Stream, object>) null;

			Assert.Throws<ArgumentNullException>(() =>
				_feedProcessor.Queue(propertiesContainer, callbackMethod, new {Foo = "Bar"}));
		}

		[Test]
		public void QueueFeed_WithNullReportRequestPropertiesContainerArgument_ThrowsArgumentNullException()
		{
			FeedSubmissionPropertiesContainer propertiesContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			Assert.Throws<ArgumentNullException>(() =>
				_feedProcessor.Queue(propertiesContainer, callbackMethod, new {Foo = "Bar"}));
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
			var propertiesContainer = new FeedSubmissionPropertiesContainer("", "");
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_feedProcessor.Queue(propertiesContainer, callbackMethod, new CallbackActivatorTests.CallbackDataTest {Foo = "Bar"});

			_feedSubmissionCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion


		#region PollFeeds tests 

		[Test]
		public void Poll_CallsOnce_GetNextFeedToSubmitFromQueue()
		{
			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNull_DoesNotSubmitFeedToAmazon()
		{
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns((FeedSubmissionCallback) null);

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.MoveToQueueOfSubmittedFeeds(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_DoesSubmitFeedToAmazon()
		{
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback());

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void
			Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_UpdatesLastSubmittedPropertyForProcessedFeedSubmission()
		{
			FeedSubmissionCallback feedSubmissionCallback = null;

			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback {LastSubmitted = DateTime.MinValue});
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
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback {LastSubmitted = DateTime.MinValue});
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()))
				.Returns("testFeedSubmissionId");

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(
				rrp => rrp.MoveToQueueOfSubmittedFeeds(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseNull_CallsOnce_AllocateFeedSubmissionForRetry()
		{
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback {LastSubmitted = DateTime.MinValue});
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()))
				.Returns((string) null);

			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.AllocateFeedSubmissionForRetry(It.IsAny<FeedSubmissionCallback>()),
				Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseEmpty_CallsOnce_AllocateFeedSubmissionForRetry()
		{
			_feedSubmissionProcessorMock
				.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback {LastSubmitted = DateTime.MinValue});
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()))
				.Returns(string.Empty);


			_feedProcessor.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.AllocateFeedSubmissionForRetry(It.IsAny<FeedSubmissionCallback>()),
				Times.Once);
		}

		[Test]
		public void Poll_DeletesFeedSubmissions_WithRetryCountAboveMaxRetryCount()
		{
			var testFeedSubmissionCallbacks = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback {Id = 1, SubmissionRetryCount = 0},
				new FeedSubmissionCallback {Id = 2, SubmissionRetryCount = 1},
				new FeedSubmissionCallback {Id = 3, SubmissionRetryCount = 2},
				new FeedSubmissionCallback {Id = 4, SubmissionRetryCount = 3},
				new FeedSubmissionCallback {Id = 5, SubmissionRetryCount = 4},
				new FeedSubmissionCallback {Id = 5, SubmissionRetryCount = 5}
			}.AsQueryable();
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testFeedSubmissionCallbacks);

			_feedProcessor.Poll();
			_feedSubmissionCallbackServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(3));
			_feedSubmissionCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		#endregion

	}
}