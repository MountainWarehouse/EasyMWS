using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests.ReportProcessors
{
	public class ReportProcessorTests
	{
		private ReportProcessor _reportProcessor;
		private Mock<IReportRequestCallbackService> _reportRequestCallbackServiceMock;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IRequestReportProcessor> _requestReportProcessor;
		private Mock<ICallbackActivator> _callbackActivatorMock;
		private Mock<IAmazonReportService> _amazonReportServiceMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private static bool _called;

		[SetUp]
		public void SetUp()
		{
			var options = EasyMwsOptions.Defaults();
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_requestReportProcessor = new Mock<IRequestReportProcessor>();
			_callbackActivatorMock = new Mock<ICallbackActivator>();
			_amazonReportServiceMock = new Mock<IAmazonReportService>();
			_loggerMock = new Mock<IEasyMwsLogger>();

			_callbackActivatorMock.Setup(cam => cam.SerializeCallback(It.IsAny<Action<Stream, object>>(), It.IsAny<object>()))
				.Returns(new Callback("", "", "", ""));

			_reportProcessor = new ReportProcessor(AmazonRegion.Europe, "testMerchantId1", options,
				 _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _callbackActivatorMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object);
		}


		#region QueueReport tests 

		[Test]
		public void QueueReport_WithNullCallbackMethodArgument_NeverCallsLogError()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var callbackMethod = (Action<Stream, object>) null;

			_reportProcessor.QueueReport(_reportRequestCallbackServiceMock.Object, reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		[Test]
		public void QueueReport_WithNullReportRequestPropertiesContainerArgument_CallsLogErrorOnce()
		{
			ReportRequestPropertiesContainer reportRequestContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_reportProcessor.QueueReport(_reportRequestCallbackServiceMock.Object, reportRequestContainer, callbackMethod, new { Foo = "Bar" });

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceCreateOnceWithCorrectData()
		{
			var reportRequestContainer =
				new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.NearRealTime);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
			ReportRequestEntry createReportRequestEntryObject = null;
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<ReportRequestEntry>()))
				.Callback<ReportRequestEntry>((p) => { createReportRequestEntryObject = p; });
			_reportProcessor.QueueReport(_reportRequestCallbackServiceMock.Object, reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.Create(It.IsAny<ReportRequestEntry>()), Times.Once);
			Assert.AreEqual(JsonConvert.SerializeObject(reportRequestContainer),
				createReportRequestEntryObject.ReportRequestData);
			Assert.AreEqual(AmazonRegion.Europe, createReportRequestEntryObject.AmazonRegion);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, createReportRequestEntryObject.ContentUpdateFrequency);
			Assert.AreEqual(DateTime.MinValue, createReportRequestEntryObject.LastRequested);
			Assert.NotNull(createReportRequestEntryObject.TypeName);
			Assert.NotNull(createReportRequestEntryObject.Data);
			Assert.NotNull(createReportRequestEntryObject.DataTypeName);
			Assert.NotNull(createReportRequestEntryObject.MethodName);
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceSaveChangesOnce()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_reportProcessor.QueueReport(_reportRequestCallbackServiceMock.Object, reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion


		#region PollReports tests 

		[Test]
		public void Poll_IfNoReportIsDownloaded_NoNullPointerExceptionIsLogged()
		{
			Exception actualLoggedException = null;
			_loggerMock.Setup(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback<string, Exception>((message, exception) => { actualLoggedException = exception; });

			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			Assert.IsFalse(actualLoggedException is NullReferenceException);
		}

		[Test]
		public void Poll_IfNoReportIsDownloaded_LogErrorIsNotCalled()
		{
			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		[Test]
		public void Poll_CallsOnce_GetNextFromQueueOfReportsToRequest()
		{
			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			_requestReportProcessor.Verify(
				rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()), Times.Once);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNull_DoesNotRequestAReportFromAmazon()
		{
			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()))
				.Returns((ReportRequestEntry) null);

			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			_requestReportProcessor.Verify(
				rrp => rrp.RequestReportFromAmazon(It.IsAny<ReportRequestEntry>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_RequestsAReportFromAmazon()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()))
				.Returns(new ReportRequestEntry{ReportRequestData = serializedReportRequestData });

			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			_requestReportProcessor.Verify(
				rrp => rrp.RequestReportFromAmazon(It.IsAny<ReportRequestEntry>()), Times.Once);
		}

		[Test]
		public void
			Poll_WithGetNonRequestedReportFromQueueReturningNotNull_UpdatesLastRequestedPropertyForProcessedReportRequest()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			ReportRequestEntry testReportRequestEntry = null;

			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()))
				.Returns(new ReportRequestEntry {LastRequested = DateTime.MinValue, ReportRequestData = serializedReportRequestData});
			_reportRequestCallbackServiceMock.Setup(rrcsm => rrcsm.Update(It.IsAny<ReportRequestEntry>()))
				.Callback((ReportRequestEntry arg) =>
				{
					testReportRequestEntry = arg;
				});

			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			Assert.IsTrue(DateTime.UtcNow - testReportRequestEntry.LastRequested < TimeSpan.FromHours(1));
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.AtLeastOnce);
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseNotNull_CallsOnce_GetNextFromQueueOfReportsToGenerate()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()))
				.Returns(new ReportRequestEntry {LastRequested = DateTime.MinValue, ReportRequestData = serializedReportRequestData });
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestReportFromAmazon(It.IsAny<ReportRequestEntry>()))
				.Returns("testReportRequestId");

			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			_requestReportProcessor.Verify(
				rrp => rrp.MoveToQueueOfReportsToGenerate(It.IsAny<IReportRequestCallbackService>(), It.IsAny<ReportRequestEntry>(), It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseNull_CallsOnce_AllocateReportRequestForRetry()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()))
				.Returns(new ReportRequestEntry {LastRequested = DateTime.MinValue, ReportRequestData = serializedReportRequestData});
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestReportFromAmazon(It.IsAny<ReportRequestEntry>()))
				.Returns((string) null);

			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);
			_requestReportProcessor.Verify(rrp => rrp.MoveToRetryQueue(It.IsAny<IReportRequestCallbackService>(), It.IsAny<ReportRequestEntry>()),
				Times.Once);
		}

		[Test]
		public void Poll_WithRequestReportAmazonResponseEmpty_CallsOnce_AllocateReportRequestForRetry()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_requestReportProcessor
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<IReportRequestCallbackService>()))
				.Returns(new ReportRequestEntry {LastRequested = DateTime.MinValue, ReportRequestData = serializedReportRequestData});
			_requestReportProcessor.Setup(rrp =>
					rrp.RequestReportFromAmazon(It.IsAny<ReportRequestEntry>()))
				.Returns(string.Empty);


			_reportProcessor.PollReports(_reportRequestCallbackServiceMock.Object);

			_requestReportProcessor.Verify(rrp => rrp.MoveToRetryQueue(It.IsAny<IReportRequestCallbackService>(), It.IsAny<ReportRequestEntry>()),
				Times.Once);
		}
		#endregion
	}
}