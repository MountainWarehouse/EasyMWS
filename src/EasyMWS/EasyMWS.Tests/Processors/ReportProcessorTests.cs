using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests.ReportProcessors
{
	public class ReportProcessorTests
	{
		private ReportProcessor _reportProcessor;
		private readonly int ConfiguredMaxNumberOrReportRequestRetries = 2;
		private readonly int ConfiguredMaxNumberOrFeedSubmissionRetries = 2;
		private Mock<IReportRequestCallbackService> _reportRequestCallbackServiceMock;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IRequestReportProcessor> _requestReportProcessor;
		private Mock<ICallbackActivator> _callbackActivatorMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private static bool _called;

		[SetUp]
		public void SetUp()
		{
			var options = EasyMwsOptions.Defaults;
			options.ReportRequestMaxRetryCount = ConfiguredMaxNumberOrReportRequestRetries;
			options.FeedSubmissionMaxRetryCount = ConfiguredMaxNumberOrFeedSubmissionRetries;

			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_requestReportProcessor = new Mock<IRequestReportProcessor>();
			_callbackActivatorMock = new Mock<ICallbackActivator>();
			_loggerMock = new Mock<IEasyMwsLogger>();

			_callbackActivatorMock.Setup(cam => cam.SerializeCallback(It.IsAny<Action<Stream, object>>(), It.IsAny<object>()))
				.Returns(new Callback("", "", "", ""));

			_reportProcessor = new ReportProcessor(AmazonRegion.Europe, "testMerchantId1", options,
				_reportRequestCallbackServiceMock.Object, _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _callbackActivatorMock.Object, _loggerMock.Object);
		}


		#region QueueReport tests 

		[Test]
		public void QueueReport_WithNullCallbackMethodArgument_CallsLogErrorOnce()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
			var callbackMethod = (Action<Stream, object>) null;

			_reportProcessor.Queue(reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public void QueueReport_WithNullReportRequestPropertiesContainerArgument_CallsLogErrorOnce()
		{
			ReportRequestPropertiesContainer reportRequestContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_reportProcessor.Queue(reportRequestContainer, callbackMethod, new { Foo = "Bar" });

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceCreateOnceWithCorrectData()
		{
			var reportRequestContainer =
				new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.NearRealTime);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
			ReportRequestCallback createReportRequestCallbackObject = null;
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<ReportRequestCallback>()))
				.Callback<ReportRequestCallback>((p) => { createReportRequestCallbackObject = p; });
			_reportProcessor.Queue(reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.Create(It.IsAny<ReportRequestCallback>()), Times.Once);
			Assert.AreEqual(JsonConvert.SerializeObject(reportRequestContainer),
				createReportRequestCallbackObject.ReportRequestData);
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

			_reportProcessor.Queue(reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion


		#region PollReports tests 

		[Test]
		public void Poll_CallsOnce_GetNextFromQueueOfReportsToRequest()
		{
			_reportProcessor.Poll();

			_requestReportProcessor.Verify(
				rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNull_DoesNotRequestAReportFromAmazon()
		{
			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns((ReportRequestCallback) null);

			_reportProcessor.Poll();

			_requestReportProcessor.Verify(
				rrp => rrp.RequestReportFromAmazon(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_RequestsAReportFromAmazon()
		{
			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback());

			_reportProcessor.Poll();

			_requestReportProcessor.Verify(
				rrp => rrp.RequestReportFromAmazon(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void
			Poll_WithGetNonRequestedReportFromQueueReturningNotNull_UpdatesLastRequestedPropertyForProcessedReportRequest()
		{
			ReportRequestCallback testReportRequestCallback = null;

			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback {LastRequested = DateTime.MinValue});
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.Update(It.IsAny<ReportRequestCallback>()))
				.Callback((ReportRequestCallback arg) =>
				{
					testReportRequestCallback = arg;
				});

			_reportProcessor.Poll();

			Assert.IsTrue(DateTime.UtcNow - testReportRequestCallback.LastRequested < TimeSpan.FromHours(1));
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.AtLeastOnce);
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseNotNull_CallsOnce_GetNextFromQueueOfReportsToGenerate()
		{
			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback {LastRequested = DateTime.MinValue});
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestReportFromAmazon(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
				.Returns("testReportRequestId");

			_reportProcessor.Poll();

			_requestReportProcessor.Verify(
				rrp => rrp.GetNextFromQueueOfReportsToGenerate(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseNull_CallsOnce_AllocateReportRequestForRetry()
		{
			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback {LastRequested = DateTime.MinValue});
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestReportFromAmazon(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
				.Returns((string) null);

			_reportProcessor.Poll();
			_requestReportProcessor.Verify(rrp => rrp.MoveToRetryQueue(It.IsAny<ReportRequestCallback>()),
				Times.Once);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseEmpty_CallsOnce_AllocateReportRequestForRetry()
		{
			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<AmazonRegion>(), It.IsAny<string>()))
				.Returns(new ReportRequestCallback {LastRequested = DateTime.MinValue});
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestReportFromAmazon(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
				.Returns(string.Empty);


			_reportProcessor.Poll();

			_requestReportProcessor.Verify(rrp => rrp.MoveToRetryQueue(It.IsAny<ReportRequestCallback>()),
				Times.Once);
		}

		[Test]
		public void Poll_DeletesReportRequests_WithRetryCountAboveMaxRetryCount()
		{
			var testReportRequestCallbacks = new List<ReportRequestCallback>
			{
				new ReportRequestCallback {Id = 1, RequestRetryCount = 0},
				new ReportRequestCallback {Id = 2, RequestRetryCount = 1},
				new ReportRequestCallback {Id = 3, RequestRetryCount = 2},
				new ReportRequestCallback {Id = 4, RequestRetryCount = 3},
				new ReportRequestCallback {Id = 5, RequestRetryCount = 4},
				new ReportRequestCallback {Id = 5, RequestRetryCount = 5}
			}.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.GetAll()).Returns(testReportRequestCallbacks);

			_reportProcessor.Poll();
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestCallback>()), Times.Exactly(3));
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		#endregion
	}
}