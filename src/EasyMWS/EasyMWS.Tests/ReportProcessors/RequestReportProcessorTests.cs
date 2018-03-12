using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using MarketplaceWebService;
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
		private ReportRequestCallback _reportRequestCallback;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;

		[SetUp]
		public void SetUp()
		{
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_reportRequestFactoryFba = new ReportRequestFactoryFba(_region);
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object);
			
			_reportRequestCallback = new ReportRequestCallback
			{
				AmazonRegion = AmazonRegion.Europe,
				Data = "[]",
				ReportRequestData =
					"{\"UpdateFrequency\":0,\"ReportType\":\"_GET_AFN_INVENTORY_DATA_\",\"Merchant\":null,\"MwsAuthToken\":null,\"Region\":20,\"MarketplaceIdList\":null}",
				MethodName = "<RequestSingleQueueReport_OneInQueue_SerializesCorrectMerchantId>b__7_0",
				TypeName =
					"EasyMWS.Tests.ReportProcessors.RequestReportProcessorTests+<>c, EasyMWS.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
				LastRequested = DateTime.MinValue,
				DataTypeName =
					"System.Collections.Generic.List`1[[System.Decimal, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e",
				ContentUpdateFrequency = 0,
				Id = 1
			};

			var requestReportRequest = new RequestReportResponse
			{
				RequestReportResult = new RequestReportResult
				{
					ReportRequestInfo = new ReportRequestInfo
					{
						ReportRequestId = "Report001"
					}
				}
			};


			_marketplaceWebServiceClientMock.Setup(x => x.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(requestReportRequest);

			_reportRequestCallbackServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<ReportRequestCallback, bool>>>()))
				.Returns(_reportRequestCallback);
		}

		// Todo: redo test as not really relevant
		[Test]
		public void GetFrontOfNonRequestedReportsQueue_FrontOfQueue()
		{
			var reportRequestCallback =
				_requestReportProcessor.GetFrontOfNonRequestedReportsQueue(AmazonRegion.Europe);

			Assert.AreEqual(1, reportRequestCallback.Id);
		}

		[Test]
		public void RequestSingleQueuedReport_OneInQueue_SubmitsToAmazon()
		{
			var reportId = _requestReportProcessor.RequestSingleQueuedReport(_reportRequestCallback);

			_marketplaceWebServiceClientMock.Verify(mwsc => mwsc.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);
			Assert.AreEqual("Report001", reportId);
		}
	}
}
