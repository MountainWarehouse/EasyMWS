using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
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
		private EasyMwsOptions _easyMwsOptions;

		[SetUp]
		public void SetUp()
		{
			_easyMwsOptions = EasyMwsOptions.Defaults;

			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _easyMwsOptions);
			
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
		public void GetNonRequestedReportsFromQueue_CalledWithNullMerchantId_ReturnsNull()
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

			var reportRequestCallback =
				_requestReportProcessor.GetNonRequestedReportFromQueue(_region, null);

			Assert.IsNull(reportRequestCallback);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_ReturnsFirstReportRequestFromQueueForGivenMerchant_AndSkipsReportRequestsForDifferentMerchants()
		{
			var testMerchantId2 = "testMerchantId2";
			var reportRequestWithDifferentMerchant = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2, Id = 2, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentMerchant);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);

			var reportRequestCallback =
				_requestReportProcessor.GetNonRequestedReportFromQueue(_region, _merchantId);

			Assert.AreEqual(reportRequestWithCorrectRegion1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_ReturnsFirstReportRequestFromQueueForGivenRegion_AndSkipsReportRequestsForDifferentRegions()
		{
			var testRegion = AmazonRegion.Europe;
			var reportRequestWithDifferentRegion = new ReportRequestCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 0};
			var reportRequestWithCorrectRegion1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentRegion);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);

			var reportRequestCallback =
				_requestReportProcessor.GetNonRequestedReportFromQueue(testRegion, _merchantId);

			Assert.AreEqual(reportRequestWithCorrectRegion1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_ReturnsFirstReportRequestFromQueueWithNullRequestReportId_AndSkipsReportRequestsWithNonNullRequestReportId()
		{
			var testRegion = AmazonRegion.Europe;
			var reportRequestWithDifferentRegion = new ReportRequestCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithNonNullRequestReportId = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = "testRequestReportId", RequestRetryCount = 0 };
			var reportRequestWithNullRequestReportId1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0 };
			var reportRequestWithNullRequestReportId2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 5, RequestReportId = null, RequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentRegion);
			_reportRequestCallbacks.Add(reportRequestWithNonNullRequestReportId);
			_reportRequestCallbacks.Add(reportRequestWithNullRequestReportId1);
			_reportRequestCallbacks.Add(reportRequestWithNullRequestReportId2);

			var reportRequestCallback =
				_requestReportProcessor.GetNonRequestedReportFromQueue(testRegion, _merchantId);

			Assert.AreEqual(reportRequestWithNullRequestReportId1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_ReturnsFirstReportRequestFromQueueWithNoRequestRetryCount_AndSkipsReportRequestsWithRequestRetryPeriodIncomplete()
		{
			var testRegion = AmazonRegion.Europe;
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddHours(-1)};
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromHours(2);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromHours(2);
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _easyMwsOptions);
			var reportRequestWithNoRequestRetryCount1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.MinValue };
			var reportRequestWithNoRequestRetryCount2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.MinValue };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRequestRetryCount1);
			_reportRequestCallbacks.Add(reportRequestWithNoRequestRetryCount2);

			var reportRequestCallback =
				_requestReportProcessor.GetNonRequestedReportFromQueue(testRegion, _merchantId);

			Assert.AreEqual(reportRequestWithNoRequestRetryCount1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_ReturnsFirstReportRequestFromQueueWithCompleteRetryPeriod_AndSkipsReportRequestsWithRequestRetryPeriodIncomplete()
		{
			var testRegion = AmazonRegion.Europe;
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-30) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromHours(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromHours(1);
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 0, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback =
				_requestReportProcessor.GetNonRequestedReportFromQueue(testRegion, _merchantId);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_WithConfiguredTimeToWaitBeforeFirstRetry_AndInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(60);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(1);
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback = _requestReportProcessor.GetNonRequestedReportFromQueue(AmazonRegion.Europe, _merchantId);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_WithRetryPeriodTypeConfiguredAsArithmeticProgression_AndNonInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, RequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(60);
			_easyMwsOptions.ReportRequestMaxRetryCount = 10;
			_easyMwsOptions.ReportRequestRetryType = RetryPeriodType.ArithmeticProgression;
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, RequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, RequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback = _requestReportProcessor.GetNonRequestedReportFromQueue(AmazonRegion.Europe, _merchantId);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNonRequestedReportsFromQueue_WithRetryPeriodTypeConfiguredAsGeometricProgression_AndNonInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var testRequestRetryCount = 5;
			var minutesBetweenRetries = 60;
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestCallback { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 2, RequestReportId = null,
				RequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(minutesBetweenRetries);
			_easyMwsOptions.ReportRequestMaxRetryCount = 10;
			_easyMwsOptions.ReportRequestRetryType = RetryPeriodType.GeometricProgression;
			_requestReportProcessor = new RequestReportProcessor(_marketplaceWebServiceClientMock.Object, _reportRequestCallbackServiceMock.Object, _easyMwsOptions);
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

			var reportRequestCallback = _requestReportProcessor.GetNonRequestedReportFromQueue(AmazonRegion.Europe, _merchantId);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete2.Id, reportRequestCallback.Id);
		}

		[Test]
		public void RequestSingleQueuedReport_CalledWithNullReportRequestCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestSingleQueuedReport(null, "testMerchantId"));
		}

		[Test]
		public void RequestSingleQueuedReport_CalledWithNullMerchantId_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestSingleQueuedReport(new ReportRequestCallback(), null));
		}

		[Test]
		public void RequestSingleQueuedReport_CalledWithEmptyMerchantId_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestSingleQueuedReport(new ReportRequestCallback(), string.Empty));
		}

		[Test]
		public void RequestSingleQueuedReport_OneInQueue_SubmitsToAmazon()
		{
			var reportId = _requestReportProcessor.RequestSingleQueuedReport(_reportRequestCallbacks[0], "testMerchantId");

			_marketplaceWebServiceClientMock.Verify(mwsc => mwsc.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);
			Assert.AreEqual("Report001", reportId);
		}

		[Test]
		public void RequestSingleQueuedReport_WithAllOptions_SubmitsCorrectDataToAmazon()
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

			_requestReportProcessor.RequestSingleQueuedReport(reportRequestCallback, "testMerchantId");

			Assert.AreEqual("testMerchant800", requestReportRequest.Merchant);
			Assert.AreEqual("testReportType800", requestReportRequest.ReportType);
			Assert.AreEqual(new DateTime(1000, 10, 10), requestReportRequest.StartDate);
			Assert.AreEqual(new DateTime(2000, 12, 20), requestReportRequest.EndDate);
			Assert.AreEqual("testOptions", requestReportRequest.ReportOptions);
			Assert.NotNull(requestReportRequest.MarketplaceIdList);
			CollectionAssert.AreEquivalent(new List<string> { "testMpId1", "testMpId2" }, requestReportRequest.MarketplaceIdList.Id);
		}

		[Test]
		public void RequestSingleQueuedReport_WithNoOptions_SubmitsCorrectDataToAmazon()
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

			_requestReportProcessor.RequestSingleQueuedReport(reportRequestCallback, "testMerchantId");

			Assert.AreEqual("testMerchant800", requestReportRequest.Merchant);
			Assert.AreEqual("testReportType800", requestReportRequest.ReportType);
			Assert.AreEqual(DateTime.MinValue,requestReportRequest.StartDate);
			Assert.AreEqual(DateTime.MinValue, requestReportRequest.EndDate);
			Assert.IsNull(requestReportRequest.ReportOptions);
			Assert.IsNull(requestReportRequest.MarketplaceIdList);
		}

		[Test]
		public void MoveToNonGeneratedReportsQueue_UpdatesRequestReportId_OnTheCallback()
		{
			var reportRequestId = "testReportRequestId";

			 _requestReportProcessor.MoveToNonGeneratedReportsQueue(_reportRequestCallbacks[0], reportRequestId);

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
			var listPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue(_region, _merchantId);
			
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

			// Act
			var listPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue(_region, null);

			// Assert
			Assert.IsEmpty(listPendingReports);

		}

		[Test]
		public void RequestReportsStatuses_WithMultiplePendingReports_SubmitsAmazonRequest()
		{
			var testRequestIdList = new List<string>{ "Report1", "Report2", "Report3" };

			var result = _requestReportProcessor.GetReportRequestListResponse(testRequestIdList, "");

			Assert.AreEqual("testGeneratedReportId", result.First(x => x.ReportRequestId == "Report1").GeneratedReportId);
			Assert.AreEqual("_DONE_", result.First(x => x.ReportRequestId == "Report1").ReportProcessingStatus);
			Assert.AreEqual("_CANCELLED_", result.First(x => x.ReportRequestId == "Report2").ReportProcessingStatus);
			Assert.IsNull(result.First(x => x.ReportRequestId == "Report2").GeneratedReportId);
		}

		[Test]
		public void MoveReportsToQueuesAccordingToProcessingStatus_UpdateGeneratedRequestId()
		{
			// Arrange
			var data = new List<ReportRequestCallback>
			{
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 4,
					RequestReportId = "Report3",
					GeneratedReportId = null
				},
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 5,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
				,
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 6,
					RequestReportId = "Report5",
					GeneratedReportId = null
				}
				,
				new ReportRequestCallback
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 7,
					RequestReportId = "Report6",
					GeneratedReportId = null
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

			_requestReportProcessor.MoveReportsToQueuesAccordingToProcessingStatus(dataResult);

			Assert.AreEqual("GeneratedId1", _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").RequestRetryCount);
			Assert.AreEqual("GeneratedId2", _reportRequestCallbacks.First(x => x.RequestReportId == "Report2").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report2").RequestRetryCount);
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
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Exactly(6));
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestCallback>()), Times.Never);
		}

		[Test]
		public void MoveReportsToQueuesAccordingToProcessingStatus_UpdateReportRequestId()
		{
			_reportRequestCallbacks.First().RequestReportId = "Report3";

			var data = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>
			{
				("Report1", "GeneratedId1", "_DONE_"),
				("Report2", "GeneratedId2", "_DONE_"),
				("Report3", null, "_CANCELLED_"),
				("Report4", null, "_OTHER_")
			};

			_requestReportProcessor.MoveReportsToQueuesAccordingToProcessingStatus(data);

			Assert.IsNull(_reportRequestCallbacks.First().RequestReportId);
			Assert.IsTrue(_reportRequestCallbacks.First().RequestRetryCount > 0);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void GetReadyForDownloadReportsFromQueue_ReturnListOfReports_GeneratedIdNotNull_ForGivenRegionAndMerchantId()
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

			var result = _requestReportProcessor.GetReadyForDownloadReportsFromQueue(_region, _merchantId);

			Assert.AreEqual(4, result.Id);
		}

		[Test]
		public void GetReadyForDownloadReportsFromQueue_CalledWithNullMerchantId_ReturnsNull()
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

			var result = _requestReportProcessor.GetReadyForDownloadReportsFromQueue(_region, null);

			Assert.IsNull(result);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_ShouldDownloadReportFromAmazon_ReturnStream()
		{
			// Arrange
			var merchantId = "testMerchantId";

			var reportRequestCallback = new ReportRequestCallback
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
			var result = _requestReportProcessor.DownloadGeneratedReportFromAmazon(testData, merchantId);

			// Assert
			_marketplaceWebServiceClientMock.Verify(x => x.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
			Assert.IsNotNull(result);
		}

		[Test]
		public void AllocateReportRequestForRetry_CalledOnce_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _reportRequestCallbacks.First().RequestRetryCount);

			_requestReportProcessor.AllocateReportRequestForRetry(_reportRequestCallbacks.First());

			Assert.AreEqual(1, _reportRequestCallbacks.First().RequestRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Once);
		}

		[Test]
		public void AllocateReportRequestForRetry_CalledMultipleTimes_IncrementsRequestRetryCountCorrectly()
		{
			Assert.AreEqual(0, _reportRequestCallbacks.First().RequestRetryCount);

			_requestReportProcessor.AllocateReportRequestForRetry(_reportRequestCallbacks.First());
			_requestReportProcessor.AllocateReportRequestForRetry(_reportRequestCallbacks.First());
			_requestReportProcessor.AllocateReportRequestForRetry(_reportRequestCallbacks.First());

			Assert.AreEqual(3, _reportRequestCallbacks.First().RequestRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestCallback>()), Times.Exactly(3));
		}
	}
}
