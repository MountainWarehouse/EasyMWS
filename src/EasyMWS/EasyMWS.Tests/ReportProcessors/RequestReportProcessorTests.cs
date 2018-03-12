using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
		private Mock<IReportRequestCallbackService> _reportRequestCallbackServiceMock;
		
		[SetUp]
		public void SetUp()
		{
			_reportRequestFactoryFba = new ReportRequestFactoryFba(_region);
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_requestReportProcessor = new RequestReportProcessor(_reportRequestCallbackServiceMock.Object);

			var reportRequestCallback = new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Data = "[]",
					ReportRequestData = "{\"UpdateFrequency\":0,\"ReportType\":\"_GET_AFN_INVENTORY_DATA_\",\"Merchant\":null,\"MwsAuthToken\":null,\"Region\":20,\"MarketplaceIdList\":null}",
					MethodName = "<RequestSingleQueueReport_OneInQueue_SerializesCorrectMerchantId>b__7_0",
					TypeName = "EasyMWS.Tests.ReportProcessors.RequestReportProcessorTests+<>c, EasyMWS.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
					LastRequested = DateTime.MinValue,
					DataTypeName = "System.Collections.Generic.List`1[[System.Decimal, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e",
					ContentUpdateFrequency = 0,
					Id = 1
			};

			_reportRequestCallbackServiceMock.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<ReportRequestCallback, bool>>>())).Returns(reportRequestCallback);
		}

		[Test]
	    public void RequestSingleQueueReport_OneInQueue_SerializesCorrectMerchantId()
	    {
		    var reportRequestCallback =
			    _requestReportProcessor.GetFrontOfNonRequestedReportsQueue(AmazonRegion.Europe);
			
		    Assert.AreEqual(AmazonRegion.Europe, reportRequestCallback.AmazonRegion);
			Assert.AreEqual("List<decimal>", reportRequestCallback.DataTypeName);

			//RequestReportRequest requestReportRequest = null;
			//_mwsClientMock.Setup(mc => mc.RequestReport(It.IsAny<RequestReportRequest>()))
			//	.Callback<RequestReportRequest>(rrr => requestReportRequest = rrr);

			//_reportManager.RequestSingleQueuedReport(AccountSettings.Europe);

			//Assert.IsNotNull(requestReportRequest);
			//Assert.AreEqual(AccountSettings.Europe.MerchantId, requestReportRequest.Merchant);
		}

	}
}
