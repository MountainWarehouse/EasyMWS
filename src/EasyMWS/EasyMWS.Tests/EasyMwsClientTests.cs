using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.ReportProcessors;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests
{
    public class EasyMwsClientTests
    {
	    private AmazonRegion _region = AmazonRegion.Europe;
	    private EasyMwsClient _easyMwsClient;
	    private static bool _called;
	    private Mock<IReportRequestCallbackService> _reportRequestCallbackServiceMock;
	    private Mock<IFeedSubmissionCallbackService> _feedSubmissionCallbackServiceMock;
	    private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
	    private Mock<IRequestReportProcessor> _requestReportProcessor;
	    private Mock<IFeedSubmissionProcessor> _feedSubmissionProcessorMock;
		private readonly int ConfiguredMaxNumberOrReportRequestRetries = 2;
	    private readonly int ConfiguredMaxNumberOrFeedSubmissionRetries = 2;
	    private Mock<IEasyMwsLogger> _loggerMock;

		public struct CallbackDataTest
	    {
		    public string Foo;
	    }

		[SetUp]
	    public void SetUp()
		{
			var options = EasyMwsOptions.Defaults;
			options.ReportRequestMaxRetryCount = ConfiguredMaxNumberOrReportRequestRetries;
			options.FeedSubmissionMaxRetryCount = ConfiguredMaxNumberOrFeedSubmissionRetries;

			_called = false;
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_requestReportProcessor = new Mock<IRequestReportProcessor>();
			_feedSubmissionProcessorMock = new Mock<IFeedSubmissionProcessor>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "MerchantId", "", "", _feedSubmissionCallbackServiceMock.Object, _reportRequestCallbackServiceMock.Object, _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _feedSubmissionProcessorMock.Object, _loggerMock.Object, options);
		}

		#region QueueReport tests

		[Test]
		public void QueueReport_WithNullCallbackMethodArgument_ThrowsArgumentNullException()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
			var callbackMethod = (Action<Stream, object>)null;

			Assert.Throws<ArgumentNullException>(() =>
				_easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" }));
		}

		[Test]
		public void QueueReport_WithNullReportRequestPropertiesContainerArgument_ThrowsArgumentNullException()
		{
			ReportRequestPropertiesContainer reportRequestContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			Assert.Throws<ArgumentNullException>(() =>
				_easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" }));
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceCreateOnceWithCorrectData()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.NearRealTime);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
			ReportRequestCallback createReportRequestCallbackObject = null;
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<ReportRequestCallback>()))
				.Callback<ReportRequestCallback>((p) => { createReportRequestCallbackObject = p; });

			_easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

			_reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.Create(It.IsAny<ReportRequestCallback>()), Times.Once);
			Assert.AreEqual(JsonConvert.SerializeObject(reportRequestContainer), createReportRequestCallbackObject.ReportRequestData);
			Assert.AreEqual(AmazonRegion.Europe, createReportRequestCallbackObject.AmazonRegion);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, createReportRequestCallbackObject.ContentUpdateFrequency);
			Assert.AreEqual(DateTime.MinValue, createReportRequestCallbackObject.LastRequested);
			Assert.NotNull(createReportRequestCallbackObject.TypeName);
			Assert.NotNull(createReportRequestCallbackObject.Data);
			Assert.NotNull(createReportRequestCallbackObject.DataTypeName);
			Assert.NotNull(createReportRequestCallbackObject.MethodName);
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceSaveChangesOnce()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

			_reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion

		#region QueueFeed tests

		[Test]
		public void QueueFeed_WithNullCallbackMethodArgument_ThrowsArgumentNullException()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("", "");
			var callbackMethod = (Action<Stream, object>)null;

			Assert.Throws<ArgumentNullException>(() =>
				_easyMwsClient.QueueFeed(propertiesContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" }));
		}

		[Test]
		public void QueueFeed_WithNullReportRequestPropertiesContainerArgument_ThrowsArgumentNullException()
		{
			FeedSubmissionPropertiesContainer propertiesContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			Assert.Throws<ArgumentNullException>(() =>
				_easyMwsClient.QueueFeed(propertiesContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" }));
		}

		[Test]
		public void QueueFeed_WithNonEmptyArguments_CallsReportRequestCallbackServiceCreateOnceWithCorrectData()
		{
			var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType");
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
			FeedSubmissionCallback feedSubmissionCallback = null;
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<FeedSubmissionCallback>()))
				.Callback<FeedSubmissionCallback>((p) => { feedSubmissionCallback = p; });

			_easyMwsClient.QueueFeed(propertiesContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

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

			_easyMwsClient.QueueFeed(propertiesContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

			_feedSubmissionCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion

		#region PollReports tests

	    [Test]
	    public void InitializingClient_WithNullMerchant_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() =>
			    _easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, null, "", "",
				    _feedSubmissionCallbackServiceMock.Object, _reportRequestCallbackServiceMock.Object,
				    _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _feedSubmissionProcessorMock.Object, _loggerMock.Object, EasyMwsOptions.Defaults));
	    }

	    [Test]
	    public void InitializingClient_WithNullAccessKeyId_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() =>
			    _easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "MerchantId", null, "",
				    _feedSubmissionCallbackServiceMock.Object, _reportRequestCallbackServiceMock.Object,
				    _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _feedSubmissionProcessorMock.Object, _loggerMock.Object, EasyMwsOptions.Defaults));
	    }

	    [Test]
	    public void InitializingClient_WithNullMwsSecretAccessKey_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() =>
			    _easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "MerchantId", "", null,
				    _feedSubmissionCallbackServiceMock.Object, _reportRequestCallbackServiceMock.Object,
				    _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _feedSubmissionProcessorMock.Object, _loggerMock.Object, EasyMwsOptions.Defaults));
	    }

		[Test]
		public void Poll_CallsOnce_GetNonRequestedReportFromQueue()
		{
			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNull_DoesNotRequestAReportFromAmazon()
		{
			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>())).Returns((ReportRequestCallback)null);

			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_RequestsAReportFromAmazon()
		{
			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>())).Returns(new ReportRequestCallback());

			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_UpdatesLastRequestedPropertyForProcessedReportRequest()
		{
			ReportRequestCallback testReportRequestCallback = null;

			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback { LastRequested = DateTime.MinValue });
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.Update(It.IsAny<ReportRequestCallback>()))
				.Callback((ReportRequestCallback arg) =>
				{
					testReportRequestCallback = arg;
				});

			_easyMwsClient.Poll();

			Assert.IsTrue(DateTime.UtcNow - testReportRequestCallback.LastRequested < TimeSpan.FromHours(1));
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.AtLeastOnce);
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseNotNull_CallsOnce_MoveToNonGeneratedReportsQueue()
		{
			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback { LastRequested = DateTime.MinValue });
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
				.Returns("testReportRequestId");

			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.MoveToNonGeneratedReportsQueue(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseNull_CallsOnce_AllocateReportRequestForRetry()
		{
			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback { LastRequested = DateTime.MinValue });
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
				.Returns((string)null);

			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.AllocateReportRequestForRetry(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseEmpty_CallsOnce_AllocateReportRequestForRetry()
		{
			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback { LastRequested = DateTime.MinValue });
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
				.Returns(string.Empty);


			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.AllocateReportRequestForRetry(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void Poll_DeletesReportRequests_WithRetryCountAboveMaxRetryCount()
		{
			var testReportRequestCallbacks = new List<ReportRequestCallback>
			{
				new ReportRequestCallback { Id = 1, RequestRetryCount = 0 },
				new ReportRequestCallback { Id = 2, RequestRetryCount = 1 },
				new ReportRequestCallback { Id = 3, RequestRetryCount = 2 },
				new ReportRequestCallback { Id = 4, RequestRetryCount = 3 },
				new ReportRequestCallback { Id = 5, RequestRetryCount = 4 },
				new ReportRequestCallback { Id = 5, RequestRetryCount = 5 }
			}.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testReportRequestCallbacks);

			_easyMwsClient.Poll();
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestCallback>()), Times.Exactly(3));
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		#endregion

		#region PollFeeds tests

		[Test]
		public void Poll_CallsOnce_GetNextFeedToSubmitFromQueue()
		{
			_easyMwsClient.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNull_DoesNotSubmitFeedToAmazon()
		{
			_feedSubmissionProcessorMock.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>())).Returns((FeedSubmissionCallback)null);

			_easyMwsClient.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.MoveToQueueOfSubmittedFeeds(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_DoesSubmitFeedToAmazon()
		{
			_feedSubmissionProcessorMock.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>())).Returns(new FeedSubmissionCallback());

			_easyMwsClient.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNextFeedToSubmitFromQueueReturningNotNull_UpdatesLastSubmittedPropertyForProcessedFeedSubmission()
		{
			FeedSubmissionCallback feedSubmissionCallback = null;

			_feedSubmissionProcessorMock.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback { LastSubmitted = DateTime.MinValue });
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.Update(It.IsAny<FeedSubmissionCallback>()))
				.Callback((FeedSubmissionCallback arg) =>
				{
					feedSubmissionCallback = arg;
				});

			_easyMwsClient.Poll();

			Assert.IsTrue(DateTime.UtcNow - feedSubmissionCallback.LastSubmitted < TimeSpan.FromHours(1));
			_feedSubmissionCallbackServiceMock.Verify(x => x.Update(It.IsAny<FeedSubmissionCallback>()), Times.AtLeastOnce);
			_feedSubmissionCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseNotNull_CallsOnce_MoveToQueueOfSubmittedFeeds()
		{
			_feedSubmissionProcessorMock.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()))
				.Returns("testFeedSubmissionId");

			_easyMwsClient.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.MoveToQueueOfSubmittedFeeds(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseNull_CallsOnce_AllocateFeedSubmissionForRetry()
		{
			_feedSubmissionProcessorMock.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()))
				.Returns((string)null);

			_easyMwsClient.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.AllocateFeedSubmissionForRetry(It.IsAny<FeedSubmissionCallback>()), Times.Once);
		}

		[Test]
		public void Poll_WithSubmitSingleQueuedFeedToAmazonResponseEmpty_CallsOnce_AllocateFeedSubmissionForRetry()
		{
			_feedSubmissionProcessorMock.Setup(rrp => rrp.GetNextFeedToSubmitFromQueue(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new FeedSubmissionCallback { LastSubmitted = DateTime.MinValue });
			_feedSubmissionProcessorMock.Setup(rrp =>
					rrp.SubmitSingleQueuedFeedToAmazon(It.IsAny<FeedSubmissionCallback>(), It.IsAny<string>()))
				.Returns(string.Empty);


			_easyMwsClient.Poll();

			_feedSubmissionProcessorMock.Verify(rrp => rrp.AllocateFeedSubmissionForRetry(It.IsAny<FeedSubmissionCallback>()), Times.Once);
		}

		[Test]
		public void Poll_DeletesFeedSubmissions_WithRetryCountAboveMaxRetryCount()
		{
			var testFeedSubmissionCallbacks = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback { Id = 1, SubmissionRetryCount = 0 },
				new FeedSubmissionCallback { Id = 2, SubmissionRetryCount = 1 },
				new FeedSubmissionCallback { Id = 3, SubmissionRetryCount = 2 },
				new FeedSubmissionCallback { Id = 4, SubmissionRetryCount = 3 },
				new FeedSubmissionCallback { Id = 5, SubmissionRetryCount = 4 },
				new FeedSubmissionCallback { Id = 5, SubmissionRetryCount = 5 }
			}.AsQueryable();
			_feedSubmissionCallbackServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testFeedSubmissionCallbacks);

			_easyMwsClient.Poll();
			_feedSubmissionCallbackServiceMock.Verify(x => x.Delete(It.IsAny<FeedSubmissionCallback>()), Times.Exactly(3));
			_feedSubmissionCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		#endregion
	}
}
