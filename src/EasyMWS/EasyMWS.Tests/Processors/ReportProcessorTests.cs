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
		private Mock<IReportRequestEntryService> _reportRequestServiceMock;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IRequestReportProcessor> _requestReportProcessor;
		private Mock<ICallbackActivator> _callbackActivatorMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private static bool _called;
		private readonly bool MarkEntryAsLocked = true;

		[SetUp]
		public void SetUp()
		{
			var options = EasyMwsOptions.Defaults();
			_reportRequestServiceMock = new Mock<IReportRequestEntryService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_requestReportProcessor = new Mock<IRequestReportProcessor>();
			_callbackActivatorMock = new Mock<ICallbackActivator>();
			_loggerMock = new Mock<IEasyMwsLogger>();

			_callbackActivatorMock.Setup(cam => cam.SerializeCallback(It.IsAny<Action<Stream, object>>(), It.IsAny<object>()))
				.Returns(new Callback("", "", "", ""));

			_reportProcessor = new ReportProcessor(AmazonRegion.Europe, "testMerchantId1", options,
				 _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _callbackActivatorMock.Object, _loggerMock.Object);
		}


		#region QueueReport tests 

		[Test]
		public void QueueReport_WithNullCallbackMethodArgument_CatchesNullArgumentExceptionAndDoesNotQueueReport()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var callbackMethod = (Action<Stream, object>) null;

			_reportProcessor.QueueReport(_reportRequestServiceMock.Object, reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestServiceMock.Verify(rrcs => rrcs.Create(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrcs => rrcs.SaveChanges(), Times.Never);
			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<ArgumentNullException>()), Times.Once);
		}

		[Test]
		public void QueueReport_WithNullReportRequestPropertiesContainerArgument_CallsLogErrorOnce()
		{
			ReportRequestPropertiesContainer reportRequestContainer = null;
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_reportProcessor.QueueReport(_reportRequestServiceMock.Object, reportRequestContainer, callbackMethod, new { Foo = "Bar" });

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestEntryServiceCreateOnceWithCorrectData()
		{
			var reportRequestContainer =
				new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.NearRealTime);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
			ReportRequestEntry createReportRequestEntryObject = null;
			_reportRequestServiceMock.Setup(rrcsm => rrcsm.Create(It.IsAny<ReportRequestEntry>()))
				.Callback<ReportRequestEntry>((p) => { createReportRequestEntryObject = p; });
			_reportProcessor.QueueReport(_reportRequestServiceMock.Object, reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestServiceMock.Verify(rrcsm => rrcsm.Create(It.IsAny<ReportRequestEntry>()), Times.Once);
			Assert.AreEqual(JsonConvert.SerializeObject(reportRequestContainer),
				createReportRequestEntryObject.ReportRequestData);
			Assert.AreEqual(AmazonRegion.Europe, createReportRequestEntryObject.AmazonRegion);
			Assert.AreEqual(ContentUpdateFrequency.NearRealTime, createReportRequestEntryObject.ContentUpdateFrequency);
			Assert.AreEqual(DateTime.MinValue, createReportRequestEntryObject.LastAmazonRequestDate);
			Assert.NotNull(createReportRequestEntryObject.TypeName);
			Assert.NotNull(createReportRequestEntryObject.Data);
			Assert.NotNull(createReportRequestEntryObject.DataTypeName);
			Assert.NotNull(createReportRequestEntryObject.MethodName);
		}

		[Test]
		public void QueueReport_WithNonEmptyArguments_CallsReportRequestEntryServiceSaveChangesOnce()
		{
			var reportRequestContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

			_reportProcessor.QueueReport(_reportRequestServiceMock.Object, reportRequestContainer, callbackMethod, new {Foo = "Bar"});

			_reportRequestServiceMock.Verify(rrcsm => rrcsm.SaveChanges(), Times.Once);
		}

		#endregion


		#region PollReports tests 

		[Test]
		public void Poll_IfNoReportIsDownloaded_NoNullPointerExceptionIsLogged()
		{
			Exception actualLoggedException = null;
			_loggerMock.Setup(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback<string, Exception>((message, exception) => { actualLoggedException = exception; });

			_reportProcessor.PollReports(_reportRequestServiceMock.Object);

			Assert.IsFalse(actualLoggedException is NullReferenceException);
		}

		[Test]
		public void Poll_IfNoReportIsDownloaded_LogErrorIsNotCalled()
		{
			_reportProcessor.PollReports(_reportRequestServiceMock.Object);

			_loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
		}

		[Test]
		public void Poll_CallsOnce_GetNextFromQueueOfReportsToRequest()
		{
			_reportProcessor.PollReports(_reportRequestServiceMock.Object);

			_reportRequestServiceMock.Verify(
				rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<string>(), It.IsAny<AmazonRegion>(), MarkEntryAsLocked), Times.Once);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNull_DoesNotRequestAReportFromAmazon()
		{
			_reportRequestServiceMock
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<string>(), It.IsAny<AmazonRegion>(), MarkEntryAsLocked))
				.Returns((ReportRequestEntry) null);

			_reportProcessor.PollReports(_reportRequestServiceMock.Object);

			_requestReportProcessor.Verify(
				rrp => rrp.RequestReportFromAmazon(It.IsAny<IReportRequestEntryService>(), It.IsAny<ReportRequestEntry>()), Times.Never);
		}

		[Test]
		public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_RequestsAReportFromAmazon()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_reportRequestServiceMock
				.Setup(rrp => rrp.GetNextFromQueueOfReportsToRequest(It.IsAny<string>(), It.IsAny<AmazonRegion>(), MarkEntryAsLocked))
				.Returns(new ReportRequestEntry{ReportRequestData = serializedReportRequestData });

			_reportProcessor.PollReports(_reportRequestServiceMock.Object);

			_requestReportProcessor.Verify(
				rrp => rrp.RequestReportFromAmazon(It.IsAny<IReportRequestEntryService>(), It.IsAny<ReportRequestEntry>()), Times.Once);
		}
		#endregion
	}
}