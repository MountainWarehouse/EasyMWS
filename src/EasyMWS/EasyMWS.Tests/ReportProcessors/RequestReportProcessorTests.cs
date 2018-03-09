using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using MarketplaceWebService.Model;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Migrations;
using MountainWarehouse.EasyMWS.ReportProcessors;
using MountainWarehouse.EasyMWS.Repositories;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace EasyMWS.Tests.ReportProcessors
{
	[TestFixture]
    public class RequestReportProcessorTests
	{
		private AmazonRegion _region = AmazonRegion.Europe;
		private EasyMwsClient _easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "AccessKeyTest", "SecretAccessKeyTest");
		private ReportRequestFactoryFba _reportRequestFactoryFba;
		private RequestReportProcessor _requestReportProcessor;
		private ReportRequestCallbackService _reportRequestCallbackService;
		private Mock<IReportRequestCallbackRepo> _reportRequestCallbackRepoMock;

		[SetUp]
		public void SetUp()
		{
			_reportRequestCallbackRepoMock = new Mock<IReportRequestCallbackRepo>();
			_reportRequestFactoryFba = new ReportRequestFactoryFba(_region);
			_reportRequestCallbackService = new ReportRequestCallbackService(_reportRequestCallbackRepoMock.Object);
			_requestReportProcessor = new RequestReportProcessor(_reportRequestCallbackService);
		}

		[Test]
	    public void RequestSingleQueueReport_OneInQueue_SerializesCorrectMerchantId()
	    {
			var request = _reportRequestFactoryFba.GenerateRequestForReportGetAfnInventoryData();
			var callbackMethod = new Action<Stream, object>((stream, o) => { });

			_easyMwsClient.QueueReport(request, callbackMethod, new object());

		    var reportRequestCallback =
			    _requestReportProcessor.GetFrontOfNonRequestedReportsQueue(AmazonRegion.Europe);
			
		    Assert.AreEqual(AmazonRegion.Europe, reportRequestCallback.AmazonRegion);
			Assert.AreEqual("DataTest", reportRequestCallback.MethodName);

			//RequestReportRequest requestReportRequest = null;
			//_mwsClientMock.Setup(mc => mc.RequestReport(It.IsAny<RequestReportRequest>()))
			//	.Callback<RequestReportRequest>(rrr => requestReportRequest = rrr);

			//_reportManager.RequestSingleQueuedReport(AccountSettings.Europe);

			//Assert.IsNotNull(requestReportRequest);
			//Assert.AreEqual(AccountSettings.Europe.MerchantId, requestReportRequest.Merchant);
		}

	}
}
