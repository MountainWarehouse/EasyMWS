using System;
using System.IO;
using System.Threading.Tasks;
using EasyMWS.Tests.ReportProcessors;
using MarketplaceWebService;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
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

		public struct CallbackDataTest
	    {
		    public string Foo;
	    }

		[SetUp]
	    public void SetUp()
		{
			_called = false;
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_requestReportProcessor = new Mock<IRequestReportProcessor>();
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "MerchantId", "", "", _reportRequestCallbackServiceMock.Object, _marketplaceWebServiceClientMock.Object, _requestReportProcessor.Object);
		}

	    [Test]
	    public void QueueReport_WithNullReportRequestPropertiesContainerArgument_ThrowsArgumentNullException()
	    {
		    var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
		    var callbackMethod = (Action<Stream, object>) null;

		    Assert.Throws<ArgumentNullException>(() =>
			    _easyMwsClient.QueueReport(reportRequestContainer, callbackMethod, new CallbackDataTest{Foo = "Bar"}));
	    }

	    [Test]
	    public void QueueReport_WithNullCallbackMethodArgument_ThrowsArgumentNullException()
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

	    [Test]
	    public async Task QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceCreateAsyncOnceWithCorrectData()
	    {
		    var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
		    var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

		    await _easyMwsClient.QueueReportAsync(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

		    _reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.CreateAsync(It.IsAny<ReportRequestCallback>()), Times.Once);
	    }

	    [Test]
	    public async Task QueueReport_WithNonEmptyArguments_CallsReportRequestCallbackServiceSaveChangesAsyncOnce()
	    {
		    var reportRequestContainer = new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown);
		    var callbackMethod = new Action<Stream, object>((stream, o) => { _called = true; });

		    await _easyMwsClient.QueueReportAsync(reportRequestContainer, callbackMethod, new CallbackDataTest { Foo = "Bar" });

		    _reportRequestCallbackServiceMock.Verify(rrcsm => rrcsm.SaveChangesAsync(), Times.Once);
	    }
	}
}
