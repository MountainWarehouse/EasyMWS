using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarketplaceWebService;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.ReportProcessors;
using MountainWarehouse.EasyMWS.Services;
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
	    private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
	    private Mock<IRequestReportProcessor> _requestReportProcessor;
	    private readonly int ConfiguredMaxNumberOrReportRequestRetries = 2;
	    private Mock<IEasyMwsLogger> _loggerMock;

		public struct CallbackDataTest
	    {
		    public string Foo;
	    }

		[SetUp]
	    public void SetUp()
		{
			var options = EasyMwsOptions.Defaults;
			options.MaxRequestRetryCount = ConfiguredMaxNumberOrReportRequestRetries;

			_called = false;
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_requestReportProcessor = new Mock<IRequestReportProcessor>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "MerchantId", "", "", _reportRequestCallbackServiceMock.Object, _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object, _loggerMock.Object ,options);
		}

	    [Test]
	    public void QueueReport_WithNullReportRequestPropertiesContainerArgument_LogsErrorForArgumentNullException()
	    {
		    var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
		    var callbackMethod = (Action<Stream, object>) null;
		    var loggedMessage = string.Empty;
		    var loggedException = (Exception)null;
		    _loggerMock.Setup(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>())).Callback((string m, Exception ex) =>
		    {
			    loggedMessage = m;
			    loggedException = ex;
		    });

		    _easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

		    _loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		    Assert.IsTrue(loggedMessage.Contains("QueueReport operation failed"));
		    Assert.IsTrue(loggedException is ArgumentNullException);
		}

	    [Test]
	    public void QueueReport_WithNullCallbackMethodArgument_LogsErrorForArgumentNullException()
	    {
		    ReportRequestPropertiesContainer reportRequestContainer = null;
		    var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });
		    var loggedMessage = string.Empty;
		    var loggedException = (Exception)null;
		    _loggerMock.Setup(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>())).Callback((string m, Exception ex) =>
		    {
			    loggedMessage = m;
			    loggedException = ex;
		    });

		    _easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

		    _loggerMock.Verify(lm => lm.Error(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
		    Assert.IsTrue(loggedMessage.Contains("QueueReport operation failed"));
		    Assert.IsTrue(loggedException is ArgumentNullException);
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

	    [Test]
	    public void Poll_CallsOnce_GetNonRequestedReportFromQueue()
	    {
			_easyMwsClient.Poll();

		    _requestReportProcessor.Verify(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>()), Times.Once);
		}

		[Test]
	    public void Poll_WithGetNonRequestedReportFromQueueReturningNull_DoesNotRequestAReportFromAmazon()
		{
			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>())).Returns((ReportRequestCallback)null);

			_easyMwsClient.Poll();

			_requestReportProcessor.Verify(rrp => rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Never);
	    }

	    [Test]
	    public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_RequestsAReportFromAmazon()
	    {
		    _requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>())).Returns(new ReportRequestCallback());

		    _easyMwsClient.Poll();

		    _requestReportProcessor.Verify(rrp => rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()), Times.Once);
	    }

	    [Test]
	    public void Poll_WithGetNonRequestedReportFromQueueReturningNotNull_UpdatesLastRequestedPropertyForProcessedReportRequest()
	    {
		    ReportRequestCallback testReportRequestCallback = null;

			_requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>()))
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
		    _requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>()))
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
		    _requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>()))
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
		    _requestReportProcessor.Setup(rrp => rrp.GetNonRequestedReportFromQueue(It.IsAny<AmazonRegion>()))
			    .Returns(new ReportRequestCallback { LastRequested = DateTime.MinValue });
		    _requestReportProcessor.Setup(rrp =>
				    rrp.RequestSingleQueuedReport(It.IsAny<ReportRequestCallback>(), It.IsAny<string>()))
			    .Returns(string.Empty);


		    _easyMwsClient.Poll();

		    _requestReportProcessor.Verify(rrp => rrp.AllocateReportRequestForRetry(It.IsAny<ReportRequestCallback>()), Times.Once);
	    }

	    [Test]
	    public void Poll_DeletesReportRequests_WithRetryCountAboveMaxRequestRetryCount()
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
	}
}
