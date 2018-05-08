using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests.Processors
{
	[TestFixture]
	public class RequestReportProcessorTests
	{
		private AmazonRegion _region = AmazonRegion.Europe;
		private string _merchantId = "TestMerchantId";
		private IRequestReportProcessor _requestReportProcessor;
		private Mock<IReportRequestCallbackService> _reportRequestCallbackServiceMock;
		private List<ReportRequestCallback> _reportRequestCallbacks;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IAmazonReportService> _amazonReportServiceMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private EasyMwsOptions _easyMwsOptions;

		[SetUp]
		public void SetUp()
		{
			_easyMwsOptions = EasyMwsOptions.Defaults();

			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_amazonReportServiceMock = new Mock<IAmazonReportService>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);
			
			_reportRequestCallbacks = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.China,
					Id = 1,
					RequestReportId = null,
					GeneratedReportId = null,
					ReportRequestData = "{\"UpdateFrequency\":0,\"ReportType\":\"_GET_AFN_INVENTORY_DATA_\",\"Merchant\":null,\"MwsAuthToken\":null,\"Region\":20,\"MarketplaceIdList\":null}"
				}
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

			var getReportRequestListResponse = new GetReportRequestListResponse
			{
				GetReportRequestListResult = new GetReportRequestListResult
				{
					ReportRequestInfo = new List<ReportRequestInfo>
					{
						new ReportRequestInfo
						{
							ReportProcessingStatus = "_DONE_",
							ReportRequestId = "Report1",
							GeneratedReportId = "testGeneratedReportId"
						},
						new ReportRequestInfo
						{
							ReportProcessingStatus = "_CANCELLED_",
							ReportRequestId = "Report2",
							GeneratedReportId = null
						},
						new ReportRequestInfo
						{
							ReportProcessingStatus = "_OTHER_",
							ReportRequestId = "Report3",
							GeneratedReportId = null
						}
					}
				}
			};

			var reportRequestCallbacks = _reportRequestCallbacks.AsQueryable();

			_reportRequestCallbackServiceMock.Setup(x => x.Where(It.IsAny<Expression<Func<ReportRequestCallback, bool>>>()))
				.Returns((Expression<Func<ReportRequestCallback, bool>> e) => reportRequestCallbacks.Where(e));

			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(reportRequestCallbacks);

			_marketplaceWebServiceClientMock.Setup(x => x.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(requestReportRequest);

			_marketplaceWebServiceClientMock.Setup(x => x.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()))
				.Returns(getReportRequestListResponse);

			_reportRequestCallbackServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<ReportRequestCallback, bool>>>()))
				.Returns((Expression<Func<ReportRequestCallback, bool>> e) => reportRequestCallbacks.FirstOrDefault(e));

			_marketplaceWebServiceClientMock.Setup(x => x.GetReport(It.IsAny<GetReportRequest>()))
				.Returns(new GetReportResponse());
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_CalledWithNullMerchantId_ReturnsNull()
		{
			var testMerchantId2 = "testMerchantId2";
			var reportRequestWithDifferentMerchant = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2, Id = 2, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithNullMerchant = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = null, Id = 5, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentMerchant);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);
			_reportRequestCallbacks.Add(reportRequestWithNullMerchant);

			_requestReportProcessor = new RequestReportProcessor(_region, null, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.IsNull(reportRequestCallback);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueForGivenMerchant_AndSkipsReportRequestsForDifferentMerchants()
		{
			var testMerchantId2 = "testMerchantId2";
			var reportRequestWithDifferentMerchant = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2, Id = 2, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentMerchant);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithCorrectRegion1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueForGivenRegion_AndSkipsReportRequestsForDifferentRegions()
		{
			var reportRequestWithDifferentRegion = new ReportRequestCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 0};
			var reportRequestWithCorrectRegion1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentRegion);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithCorrectRegion1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueWithNullRequestReportId_AndSkipsReportRequestsWithNonNullRequestReportId()
		{
			var reportRequestWithDifferentRegion = new ReportRequestCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithNonNullRequestReportId = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = "testRequestReportId", RequestRetryCount = 0 };
			var reportRequestWithNullRequestReportId1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithNullRequestReportId2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 5, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentRegion);
			_reportRequestCallbacks.Add(reportRequestWithNonNullRequestReportId);
			_reportRequestCallbacks.Add(reportRequestWithNullRequestReportId1);
			_reportRequestCallbacks.Add(reportRequestWithNullRequestReportId2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithNullRequestReportId1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueWithNoRequestRetryCount_AndSkipsReportRequestsWithRequestRetryPeriodIncomplete()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddHours(-1)};
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromHours(2);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromHours(2);
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRequestRetryCount1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.MinValue };
			var reportRequestWithNoRequestRetryCount2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.MinValue };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRequestRetryCount1);
			_reportRequestCallbacks.Add(reportRequestWithNoRequestRetryCount2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithNoRequestRetryCount1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueWithCompleteRetryPeriod_AndSkipsReportRequestsWithRequestRetryPeriodIncomplete()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-30) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromHours(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromHours(1);
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_WithConfiguredTimeToWaitBeforeFirstRetry_AndInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(60);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(1);
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback = _requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_WithRetryPeriodTypeConfiguredAsArithmeticProgression_AndNonInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(60);
			_easyMwsOptions.ReportRequestRetryType = RetryPeriodType.ArithmeticProgression;
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback = _requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_WithRetryPeriodTypeConfiguredAsGeometricProgression_AndNonInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var testRequestRetryCount = 5;
			var minutesBetweenRetries = 60;
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 2, RequestReportId = null,
				RequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(minutesBetweenRetries);
			_easyMwsOptions.ReportRequestRetryType = RetryPeriodType.GeometricProgression;
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 3, RequestReportId = null,
				RequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 4, RequestReportId = null,
				RequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-(testRequestRetryCount * minutesBetweenRetries - 1)) };
			var reportRequestWithNoRetryPeriodComplete3 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 5, RequestReportId = null,
				RequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-(testRequestRetryCount * minutesBetweenRetries - 1)) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete3);

			var reportRequestCallback = _requestReportProcessor.GetNextFromQueueOfReportsToRequest();

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete2.Id, reportRequestCallback.Id);
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithNullReportRequestCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestReportFromAmazon(null));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestCallbackWithNullReportRequestData_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestReportFromAmazon(new ReportRequestCallback()));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestDataWithNoReportType_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentException>(() => _requestReportProcessor.RequestReportFromAmazon(new ReportRequestCallback{ReportRequestData = String.Empty}));
		}

		[Test]
		public void RequestReportFromAmazon_OneInQueue_SubmitsToAmazon()
		{
			var reportId = _requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbacks[0]);

			_marketplaceWebServiceClientMock.Verify(mwsc => mwsc.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);
			Assert.AreEqual("Report001", reportId);
		}

		[Test]
		public void RequestReportFromAmazon_WithAllOptions_SubmitsCorrectDataToAmazon()
		{

			var reportRequestPropertiesContainer = new ReportRequestPropertiesContainer("testReportType800",
				ContentUpdateFrequency.Unknown, new List<string> {"testMpId1", "testMpId2"}, new DateTime(1000, 10, 10), new DateTime(2000, 12, 20), "testOptions");
			var reportRequestCallback = new ReportRequestCallback
			{
				MerchantId = "testMerchant800",
				ReportRequestData = JsonConvert.SerializeObject(reportRequestPropertiesContainer)
			};
			var requestReportRequest = (RequestReportRequest) null;
			_marketplaceWebServiceClientMock.Setup(mwscm => mwscm.RequestReport(It.IsAny<RequestReportRequest>()))
				.Callback<RequestReportRequest>((rrr) => { requestReportRequest = rrr; });

			_requestReportProcessor.RequestReportFromAmazon(reportRequestCallback);

			Assert.AreEqual("testMerchant800", requestReportRequest.Merchant);
			Assert.AreEqual("testReportType800", requestReportRequest.ReportType);
			Assert.AreEqual(new DateTime(1000, 10, 10), requestReportRequest.StartDate);
			Assert.AreEqual(new DateTime(2000, 12, 20), requestReportRequest.EndDate);
			Assert.AreEqual("testOptions", requestReportRequest.ReportOptions);
			Assert.NotNull(requestReportRequest.MarketplaceIdList);
			CollectionAssert.AreEquivalent(new List<string> { "testMpId1", "testMpId2" }, requestReportRequest.MarketplaceIdList.Id);
		}

		[Test]
		public void RequestReportFromAmazon_WithNoOptions_SubmitsCorrectDataToAmazon()
		{

			var reportRequestPropertiesContainer = new ReportRequestPropertiesContainer("testReportType800", ContentUpdateFrequency.Unknown);
			var reportRequestCallback = new ReportRequestCallback
			{
				MerchantId = "testMerchant800",
				ReportRequestData = JsonConvert.SerializeObject(reportRequestPropertiesContainer)
			};
			var requestReportRequest = (RequestReportRequest)null;
			_marketplaceWebServiceClientMock.Setup(mwscm => mwscm.RequestReport(It.IsAny<RequestReportRequest>()))
				.Callback<RequestReportRequest>((rrr) => { requestReportRequest = rrr; });

			_requestReportProcessor.RequestReportFromAmazon(reportRequestCallback);

			Assert.AreEqual("testMerchant800", requestReportRequest.Merchant);
			Assert.AreEqual("testReportType800", requestReportRequest.ReportType);
			Assert.AreEqual(DateTime.MinValue,requestReportRequest.StartDate);
			Assert.AreEqual(DateTime.MinValue, requestReportRequest.EndDate);
			Assert.IsNull(requestReportRequest.ReportOptions);
			Assert.IsNull(requestReportRequest.MarketplaceIdList);
		}

		[Test]
		public void GetNextFromQueueOfReportsToGenerate_UpdatesRequestReportId_OnTheCallback()
		{
			var reportRequestId = "testReportRequestId";

			 _requestReportProcessor.GetNextFromQueueOfReportsToGenerate(_reportRequestCallbacks[0], reportRequestId);

			Assert.AreEqual("testReportRequestId", _reportRequestCallbacks[0].RequestReportId);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void GetAllPendingReportFromQueue_ReturnListReportRequestId_ForGivenMerchant()
		{
			// Arrange
			var testMerchantId2 = "test merchant id 2";
			var data = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 4,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 5,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 6,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest1"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = _merchantId,
					Id = 7,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};
			
			_reportRequestCallbacks.AddRange(data);

			// Act
			var listPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue();
			
			// Assert
			Assert.AreEqual(2, listPendingReports.Count());
			Assert.IsTrue(listPendingReports.Count(sf => sf.Id == 4 || sf.Id == 5) == 2);
		}

		[Test]
		public void GetAllPendingReportFromQueue_CalledWithNullMerchantId_ReturnsNull()
		{
			var data = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 4,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest1"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = null,
					Id = 5,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};

			_reportRequestCallbacks.AddRange(data);

			_requestReportProcessor = new RequestReportProcessor(_region, null, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);


			// Act
			var listPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue();

			// Assert
			Assert.IsEmpty(listPendingReports);

		}

		[Test]
		public void GetReportProcessingStatusesFromAmazon_WithMultiplePendingReports_SubmitsAmazonRequest()
		{
			var testRequestIdList = new List<string>{ "Report1", "Report2", "Report3" };

			var result = _requestReportProcessor.GetReportProcessingStatusesFromAmazon(testRequestIdList, "");

			Assert.AreEqual("testGeneratedReportId", result.First(x => x.ReportRequestId == "Report1").GeneratedReportId);
			Assert.AreEqual("_DONE_", result.First(x => x.ReportRequestId == "Report1").ReportProcessingStatus);
			Assert.AreEqual("_CANCELLED_", result.First(x => x.ReportRequestId == "Report2").ReportProcessingStatus);
			Assert.IsNull(result.First(x => x.ReportRequestId == "Report2").GeneratedReportId);
		}

		[Test]
		public void QueueReportsAccordingToProcessingStatus_UpdateGeneratedRequestId()
		{
			// Arrange
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);
			var data = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 4,
					RequestReportId = "Report3",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 5,
					RequestReportId = "Report4",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
				,
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 6,
					RequestReportId = "Report5",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
				,
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 7,
					RequestReportId = "Report6",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
			};

			_reportRequestCallbacks.AddRange(data);

			var dataResult = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>
			{
				("Report1", "GeneratedId1", "_DONE_"),
				("Report2", "GeneratedId2", "_DONE_NO_DATA_"),
				("Report3", null, "_SUBMITTED_"),
				("Report4", null, "_IN_PROGRESS_"),
				("Report5", null, "_CANCELLED_"),
				("Report6", null, "_OTHER_")
			};

			_requestReportProcessor.QueueReportsAccordingToProcessingStatus(dataResult);

			Assert.AreEqual("GeneratedId1", _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").RequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report2").GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report3").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report3").RequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report4").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report4").RequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 6).GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 6).RequestReportId);
			Assert.AreEqual(1, _reportRequestCallbacks.First(x => x.Id == 6).RequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 7).GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 7).RequestReportId);
			Assert.AreEqual(1, _reportRequestCallbacks.First(x => x.Id == 7).RequestRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Exactly(5));
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void QueueReportsAccordingToProcessingStatus_UpdateReportRequestId()
		{
			_reportRequestCallbacks.First().RequestReportId = "Report3";

			var data = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>
			{
				("Report1", "GeneratedId1", "_DONE_"),
				("Report2", "GeneratedId2", "_DONE_"),
				("Report3", null, "_CANCELLED_"),
				("Report4", null, "_OTHER_")
			};

			_requestReportProcessor.QueueReportsAccordingToProcessingStatus(data);

			Assert.IsNull(_reportRequestCallbacks.First().RequestReportId);
			Assert.IsTrue(_reportRequestCallbacks.First().RequestRetryCount > 0);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void GetNextFromQueueOfReportsToDownload_ReturnListOfReports_GeneratedIdNotNull_ForGivenRegionAndMerchantId()
		{
			// Arrange
			var merchantId2 = "test merchant id 2";
			var data = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = merchantId2,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 4,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 5,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest3"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = _merchantId,
					Id = 6,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};
			_reportRequestCallbacks.AddRange(data);

			var result = _requestReportProcessor.GetNextFromQueueOfReportsToDownload();

			Assert.AreEqual(4, result.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToDownload_CalledWithNullMerchantId_ReturnsNull()
		{
			// Arrange
			var data = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 4,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 5,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest3"
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = null,
					Id = 6,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};
			_reportRequestCallbacks.AddRange(data);

			_requestReportProcessor = new RequestReportProcessor(_region, null, _marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _amazonReportServiceMock.Object, _loggerMock.Object, _easyMwsOptions);

			var result = _requestReportProcessor.GetNextFromQueueOfReportsToDownload();

			Assert.IsNull(result);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_ShouldDownloadReportFromAmazon_ReturnStream()
		{
			// Arrange
			var merchantId = "testMerchantId";
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			var reportRequestCallback = new ReportRequestCallback(serializedReportRequestData)
			{
				Data = null,
				AmazonRegion = AmazonRegion.Europe,
				Id = 4,
				RequestReportId = "Report3",
				GeneratedReportId = "GeneratedIdTest1"
			};

			_reportRequestCallbacks.Add(reportRequestCallback);

			// Act
			var testData = _reportRequestCallbacks.Find(x => x.GeneratedReportId == "GeneratedIdTest1");
			var result = _requestReportProcessor.DownloadGeneratedReportFromAmazon(testData);

			// Assert
			_marketplaceWebServiceClientMock.Verify(x => x.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
			Assert.IsNotNull(result);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithStoreReportsEnabled_CallsCreateAmazonReport_WithExpectedObject()
		{
			// Arrange
			var merchantId = "testMerchantId";
			_easyMwsOptions.KeepAmazonReportsInLocalDbAfterCallbackIsPerformed = true;

			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType",ContentUpdateFrequency.Unknown);

			var reportRequestCallback = new ReportRequestCallback
			{
				Data = null,
				AmazonRegion = AmazonRegion.Europe,
				Id = 4,
				RequestReportId = "Report3",
				GeneratedReportId = "GeneratedIdTest1",
				ReportRequestData = JsonConvert.SerializeObject(propertiesContainer)
			};
			_reportRequestCallbacks.Add(reportRequestCallback);
			AmazonReport amazonReportCreateArgument = null;
			_amazonReportServiceMock.Setup(arsm => arsm.Create(It.IsAny<AmazonReport>()))
				.Callback<AmazonReport>(report => { amazonReportCreateArgument = report; });
			_marketplaceWebServiceClientMock.Setup(mwscm => mwscm.GetReport(It.IsAny<GetReportRequest>()))
				.Returns(new GetReportResponse
				{
					ResponseHeaderMetadata = new ResponseHeaderMetadata("testRequestId", null, "testTimestamp")
				})
				.Callback<GetReportRequest>(request =>
				{
					request.Report = StreamHelper.CreateNewMemoryStream("testReportContent");
				});

			// Act
			var testData = _reportRequestCallbacks.Find(x => x.GeneratedReportId == "GeneratedIdTest1");
			var result = _requestReportProcessor.DownloadGeneratedReportFromAmazon(testData);

			// Assert
			_marketplaceWebServiceClientMock.Verify(x => x.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
			_amazonReportServiceMock.Verify(arsm=>arsm.Create(It.IsAny<AmazonReport>()), Times.Once);
			_amazonReportServiceMock.Verify(arsm => arsm.SaveChanges(), Times.Once);
			Assert.IsNotNull(result);
			Assert.AreEqual("testReportType", amazonReportCreateArgument.ReportType);
			Assert.AreEqual("testRequestId", amazonReportCreateArgument.DownloadRequestId);
			Assert.AreEqual("testTimestamp", amazonReportCreateArgument.DownloadTimestamp);
			Assert.IsTrue(DateTime.Compare(DateTime.UtcNow, amazonReportCreateArgument.DateCreated.AddMinutes(-5)) > 0);
			Assert.AreEqual("testReportContent", amazonReportCreateArgument.Content);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithStoreReportsDisabled_NeverCallsCreateAmazonReport()
		{
			// Arrange
			var merchantId = "testMerchantId";
			_easyMwsOptions.KeepAmazonReportsInLocalDbAfterCallbackIsPerformed = false;

			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);

			var reportRequestCallback = new ReportRequestCallback
			{
				Data = null,
				AmazonRegion = AmazonRegion.Europe,
				Id = 4,
				RequestReportId = "Report3",
				GeneratedReportId = "GeneratedIdTest1",
				ReportRequestData = JsonConvert.SerializeObject(propertiesContainer)
			};
			_reportRequestCallbacks.Add(reportRequestCallback);
			AmazonReport amazonReportCreateArgument = null;
			_amazonReportServiceMock.Setup(arsm => arsm.Create(It.IsAny<AmazonReport>()))
				.Callback<AmazonReport>(report => { amazonReportCreateArgument = report; });
			_marketplaceWebServiceClientMock.Setup(mwscm => mwscm.GetReport(It.IsAny<GetReportRequest>()))
				.Returns(new GetReportResponse
				{
					ResponseHeaderMetadata = new ResponseHeaderMetadata("testRequestId", null, "testTimestamp")
				})
				.Callback<GetReportRequest>(request =>
				{
					request.Report = StreamHelper.CreateNewMemoryStream("testReportContent");
				});

			// Act
			var testData = _reportRequestCallbacks.Find(x => x.GeneratedReportId == "GeneratedIdTest1");
			var result = _requestReportProcessor.DownloadGeneratedReportFromAmazon(testData);

			// Assert
			_marketplaceWebServiceClientMock.Verify(x => x.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
			_amazonReportServiceMock.Verify(arsm => arsm.Create(It.IsAny<AmazonReport>()), Times.Never);
			_amazonReportServiceMock.Verify(arsm => arsm.SaveChanges(), Times.Never);
			Assert.IsNotNull(result);
		}

		[Test]
		public void MoveToRetryQueue_CalledOnce_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _reportRequestCallbacks.First().RequestRetryCount);

			_requestReportProcessor.MoveToRetryQueue(_reportRequestCallbacks.First());

			Assert.AreEqual(1, _reportRequestCallbacks.First().RequestRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void MoveToRetryQueue_CalledMultipleTimes_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _reportRequestCallbacks.First().RequestRetryCount);

			_requestReportProcessor.MoveToRetryQueue(_reportRequestCallbacks.First());
			_requestReportProcessor.MoveToRetryQueue(_reportRequestCallbacks.First());
			_requestReportProcessor.MoveToRetryQueue(_reportRequestCallbacks.First());

			Assert.AreEqual(3, _reportRequestCallbacks.First().RequestRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Exactly(3));
		}

		[Test]
		public void Poll_DeletesReportRequests_WithRetryCountAboveMaxRetryCount()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			var testReportRequestCallbacks = new List<ReportRequestCallback>
			{
				new ReportRequestCallback {Id = 1, RequestRetryCount = 0, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 2, RequestRetryCount = 1, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 3, RequestRetryCount = 2, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 4, RequestRetryCount = 3, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 5, RequestRetryCount = 4, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 6, RequestRetryCount = 5, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 7, RequestRetryCount = 5, ReportRequestData = serializedReportRequestData, AmazonRegion = AmazonRegion.Brazil, MerchantId = _merchantId },
				new ReportRequestCallback {Id = 8, RequestRetryCount = 5, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = "someDifferentMerchantId" }
			}.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(testReportRequestCallbacks);

			_requestReportProcessor.CleanupReportRequests();
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestCallback>()), Times.Exactly(1));
		}
	}
}
