using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
		private List<ReportRequestEntry> _reportRequestCallbacks;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private EasyMwsOptions _easyMwsOptions;

		[SetUp]
		public void SetUp()
		{
			_easyMwsOptions = EasyMwsOptions.Defaults();

			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_reportRequestCallbackServiceMock = new Mock<IReportRequestCallbackService>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			
			_reportRequestCallbacks = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.China,
					Id = 1,
					RequestReportId = null,
					GeneratedReportId = null,
					ReportRequestData = "{\"UpdateFrequency\":0,\"Merchant\":null,\"MwsAuthToken\":null,\"Region\":20,\"MarketplaceIdList\":null}",
					ReportType = "_GET_AFN_INVENTORY_DATA_"
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

			_reportRequestCallbackServiceMock.Setup(x => x.Where(It.IsAny<Expression<Func<ReportRequestEntry, bool>>>()))
				.Returns((Expression<Func<ReportRequestEntry, bool>> e) => reportRequestCallbacks.Where(e));

			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(reportRequestCallbacks);

			_marketplaceWebServiceClientMock.Setup(x => x.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(requestReportRequest);

			_marketplaceWebServiceClientMock.Setup(x => x.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()))
				.Returns(getReportRequestListResponse);

			_reportRequestCallbackServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Expression<Func<ReportRequestEntry, bool>>>()))
				.Returns((Expression<Func<ReportRequestEntry, bool>> e) => reportRequestCallbacks.FirstOrDefault(e));

			_marketplaceWebServiceClientMock.Setup(x => x.GetReport(It.IsAny<GetReportRequest>()))
				.Returns(new GetReportResponse());
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_CalledWithNullMerchantId_ReturnsNull()
		{
			var testMerchantId2 = "testMerchantId2";
			var reportRequestWithDifferentMerchant = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2, Id = 2, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithCorrectRegion1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithNullMerchant = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = null, Id = 5, RequestReportId = null, ReportRequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentMerchant);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);
			_reportRequestCallbacks.Add(reportRequestWithNullMerchant);

			_requestReportProcessor = new RequestReportProcessor(_region, null, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.IsNull(reportRequestCallback);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueForGivenMerchant_AndSkipsReportRequestsForDifferentMerchants()
		{
			var testMerchantId2 = "testMerchantId2";
			var reportRequestWithDifferentMerchant = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2, Id = 2, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithCorrectRegion1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentMerchant);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithCorrectRegion1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueForGivenRegion_AndSkipsReportRequestsForDifferentRegions()
		{
			var reportRequestWithDifferentRegion = new ReportRequestEntry { AmazonRegion = AmazonRegion.Australia, MerchantId = _merchantId, Id = 2, RequestReportId = null, ReportRequestRetryCount = 0};
			var reportRequestWithCorrectRegion1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithCorrectRegion2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentRegion);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion1);
			_reportRequestCallbacks.Add(reportRequestWithCorrectRegion2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithCorrectRegion1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueWithNullRequestReportId_AndSkipsReportRequestsWithNonNullRequestReportId()
		{
			var reportRequestWithDifferentRegion = new ReportRequestEntry { AmazonRegion = AmazonRegion.Australia, MerchantId = _merchantId, Id = 2, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithNonNullRequestReportId = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = "testRequestReportId", ReportRequestRetryCount = 0 };
			var reportRequestWithNullRequestReportId1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 0 };
			var reportRequestWithNullRequestReportId2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 5, RequestReportId = null, ReportRequestRetryCount = 0 };


			_reportRequestCallbacks.Add(reportRequestWithDifferentRegion);
			_reportRequestCallbacks.Add(reportRequestWithNonNullRequestReportId);
			_reportRequestCallbacks.Add(reportRequestWithNullRequestReportId1);
			_reportRequestCallbacks.Add(reportRequestWithNullRequestReportId2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithNullRequestReportId1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueWithNoRequestRetryCount_AndSkipsReportRequestsWithRequestRetryPeriodIncomplete()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, ReportRequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddHours(-1)};
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromHours(2);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromHours(2);
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRequestRetryCount1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 0, LastRequested = DateTime.MinValue };
			var reportRequestWithNoRequestRetryCount2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 0, LastRequested = DateTime.MinValue };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRequestRetryCount1);
			_reportRequestCallbacks.Add(reportRequestWithNoRequestRetryCount2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithNoRequestRetryCount1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_ReturnsFirstReportRequestFromQueueWithCompleteRetryPeriod_AndSkipsReportRequestsWithRequestRetryPeriodIncomplete()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, ReportRequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-30) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromHours(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromHours(1);
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object,  _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 0, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 0, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback =
				_requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_WithConfiguredTimeToWaitBeforeFirstRetry_AndInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, ReportRequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(60);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(1);
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 1, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_WithRetryPeriodTypeConfiguredAsArithmeticProgression_AndNonInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 2, RequestReportId = null, ReportRequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(60);
			_easyMwsOptions.ReportRequestRetryType = RetryPeriodType.ArithmeticProgression;
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 3, RequestReportId = null, ReportRequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId, Id = 4, RequestReportId = null, ReportRequestRetryCount = 5, LastRequested = DateTime.UtcNow.AddMinutes(-61) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);

			var reportRequestCallback = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete1.Id, reportRequestCallback.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToRequest_WithRetryPeriodTypeConfiguredAsGeometricProgression_AndNonInitialRetryCount_ReturnsReportRequestWithTheExpectedCompleteRetryPeriod()
		{
			var testRequestRetryCount = 5;
			var minutesBetweenRetries = 60;
			var reportRequestWithRequestRetryPeriodIncomplete = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 2, RequestReportId = null,
				ReportRequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-61) };
			_easyMwsOptions.ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(1);
			_easyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(minutesBetweenRetries);
			_easyMwsOptions.ReportRequestRetryType = RetryPeriodType.GeometricProgression;
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			var reportRequestWithNoRetryPeriodComplete1 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 3, RequestReportId = null,
				ReportRequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-59) };
			var reportRequestWithNoRetryPeriodComplete2 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 4, RequestReportId = null,
				ReportRequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-(testRequestRetryCount * minutesBetweenRetries - 1)) };
			var reportRequestWithNoRetryPeriodComplete3 = new ReportRequestEntry { AmazonRegion = AmazonRegion.Europe,
				MerchantId = _merchantId, Id = 5, RequestReportId = null,
				ReportRequestRetryCount = testRequestRetryCount, LastRequested = DateTime.UtcNow.AddMinutes(-(testRequestRetryCount * minutesBetweenRetries - 1)) };

			_reportRequestCallbacks.Add(reportRequestWithRequestRetryPeriodIncomplete);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete1);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete2);
			_reportRequestCallbacks.Add(reportRequestWithNoRetryPeriodComplete3);

			var reportRequestCallback = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(reportRequestWithNoRetryPeriodComplete2.Id, reportRequestCallback.Id);
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithNullReportRequestCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestReportFromAmazon(It.IsAny<IReportRequestCallbackService>(), null));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestCallbackWithNullReportRequestData_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestReportFromAmazon(It.IsAny<IReportRequestCallbackService>(), new ReportRequestEntry()));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestDataWithNoReportType_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentException>(() => _requestReportProcessor.RequestReportFromAmazon(It.IsAny<IReportRequestCallbackService>(), new ReportRequestEntry {ReportRequestData = String.Empty}));
		}

		[Test]
		public void RequestReportFromAmazon_WithRequestReportAmazonResponseNotNull_CallsOnce_GetNextFromQueueOfReportsToGenerate()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(new RequestReportResponse{RequestReportResult = new RequestReportResult{ ReportRequestInfo = new ReportRequestInfo{ReportRequestId = "testReportRequestId" } }});
			var reportRequestEntryBeingUpdated = (ReportRequestEntry) null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
				{
					reportRequestEntryBeingUpdated = entry;
				}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			Assert.AreEqual("testReportRequestId", reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(0, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithRequestReportAmazonResponseNull_CallsOnce_AllocateReportRequestForRetry()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(new RequestReportResponse{RequestReportResult = new RequestReportResult{ ReportRequestInfo = new ReportRequestInfo{ReportRequestId =  null} }});
			var reportRequestEntryBeingUpdated = (ReportRequestEntry) null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
				{
					reportRequestEntryBeingUpdated = entry;
				}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			Assert.AreEqual(null, reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithRequestReportAmazonResponseEmpty_CallsOnce_AllocateReportRequestForRetry()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(new RequestReportResponse { RequestReportResult = new RequestReportResult { ReportRequestInfo = new ReportRequestInfo { ReportRequestId = string.Empty } } });
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			Assert.AreEqual(string.Empty, reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithAmazonReturningFatalErrorCodeResponse_DeletesReportRequestEntryFromQueue()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, "InvalidReportType", "errorType", "123", "xml", new ResponseHeaderMetadata()));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithAmazonReturningNonFatalErrorCodeResponse_IncrementsRetryCountCorrectly()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, "ReportNotReady", "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithAmazonReturningUnknownErrorCodeResponse_IncrementsRetryCountCorrectly()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, "UnknownErrorCode", "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithAmazonCallThrowingNonMarketplaceWebServiceException_IncrementsRetryCountCorrectly()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new Exception(""));
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_OneInQueue_SubmitsToAmazon()
		{
			var requestBeingSentToAmazon = (RequestReportRequest)null;
			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Callback<RequestReportRequest>(
					(req) =>
					{
						requestBeingSentToAmazon = req;
					});

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object, _reportRequestCallbacks[0]);
			var expectedReportType = _reportRequestCallbacks[0].ReportType;

			_marketplaceWebServiceClientMock.Verify(mwsc => mwsc.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);

			Assert.AreEqual(expectedReportType, requestBeingSentToAmazon.ReportType);
		}

		[Test]
		public void RequestReportFromAmazon_WithAllOptions_SubmitsCorrectDataToAmazon()
		{

			var reportRequestPropertiesContainer = new ReportRequestPropertiesContainer("testReportType800",
				ContentUpdateFrequency.Unknown, new List<string> {"testMpId1", "testMpId2"}, new DateTime(1000, 10, 10), new DateTime(2000, 12, 20), "testOptions");
			var reportRequestEntry = new ReportRequestEntry
			{
				MerchantId = "testMerchant800",
				ReportRequestData = JsonConvert.SerializeObject(reportRequestPropertiesContainer),
				ReportType = reportRequestPropertiesContainer.ReportType
			};
			var requestReportRequest = (RequestReportRequest) null;
			_marketplaceWebServiceClientMock.Setup(mwscm => mwscm.RequestReport(It.IsAny<RequestReportRequest>()))
				.Callback<RequestReportRequest>((rrr) => { requestReportRequest = rrr; });

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object, reportRequestEntry);

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
			var reportRequestCallback = new ReportRequestEntry
			{
				MerchantId = "testMerchant800",
				ReportRequestData = JsonConvert.SerializeObject(reportRequestPropertiesContainer),
				ReportType = reportRequestPropertiesContainer.ReportType
			};
			var requestReportRequest = (RequestReportRequest)null;
			_marketplaceWebServiceClientMock.Setup(mwscm => mwscm.RequestReport(It.IsAny<RequestReportRequest>()))
				.Callback<RequestReportRequest>((rrr) => { requestReportRequest = rrr; });

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object, reportRequestCallback);

			Assert.AreEqual("testMerchant800", requestReportRequest.Merchant);
			Assert.AreEqual("testReportType800", requestReportRequest.ReportType);
			Assert.AreEqual(DateTime.MinValue,requestReportRequest.StartDate);
			Assert.AreEqual(DateTime.MinValue, requestReportRequest.EndDate);
			Assert.IsNull(requestReportRequest.ReportOptions);
			Assert.IsNull(requestReportRequest.MarketplaceIdList);
		}

		[Test]
		public void GetAllPendingReportFromQueue_ReturnListReportRequestId_ForGivenMerchant()
		{
			// Arrange
			var testMerchantId2 = "test merchant id 2";
			var data = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId2,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 4,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 5,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 6,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest1"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = _merchantId,
					Id = 7,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};
			
			_reportRequestCallbacks.AddRange(data);

			// Act
			var listPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue(_reportRequestCallbackServiceMock.Object).ToList();
			
			// Assert
			Assert.AreEqual(2, listPendingReports.Count());
			Assert.IsTrue(listPendingReports.Count(sf => sf == "Report1" || sf == "Report2") == 2);
		}

		[Test]
		public void GetAllPendingReportFromQueue_CalledWithNullMerchantId_ReturnsNull()
		{
			var data = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 4,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest1"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = null,
					Id = 5,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};

			_reportRequestCallbacks.AddRange(data);

			_requestReportProcessor = new RequestReportProcessor(_region, null, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);


			// Act
			var listPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue(_reportRequestCallbackServiceMock.Object);

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
			var data = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 4,
					RequestReportId = "Report3",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 5,
					RequestReportId = "Report4",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
				,
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 6,
					RequestReportId = "Report5",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
				,
				new ReportRequestEntry
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

			_requestReportProcessor.QueueReportsAccordingToProcessingStatus(_reportRequestCallbackServiceMock.Object, dataResult);

			Assert.AreEqual("GeneratedId1", _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").ReportRequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report2").GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report3").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report3").ReportRequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report4").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report4").ReportRequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 6).GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 6).RequestReportId);
			Assert.AreEqual(1, _reportRequestCallbacks.First(x => x.Id == 6).ReportRequestRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 7).GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 7).RequestReportId);
			Assert.AreEqual(1, _reportRequestCallbacks.First(x => x.Id == 7).ReportRequestRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Exactly(5));
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
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

			_requestReportProcessor.QueueReportsAccordingToProcessingStatus(_reportRequestCallbackServiceMock.Object, data);

			Assert.IsNull(_reportRequestCallbacks.First().RequestReportId);
			Assert.IsTrue(_reportRequestCallbacks.First().ReportRequestRetryCount > 0);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
		}

		[Test]
		public void GetNextFromQueueOfReportsToDownload_ReturnListOfReports_GeneratedIdNotNull_ForGivenRegionAndMerchantId()
		{
			// Arrange
			var merchantId2 = "test merchant id 2";
			var data = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = merchantId2,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 4,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 5,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest3"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = _merchantId,
					Id = 6,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};
			_reportRequestCallbacks.AddRange(data);

			var result = _requestReportProcessor.GetNextFromQueueOfReportsToDownload(_reportRequestCallbackServiceMock.Object);

			Assert.AreEqual(4, result.Id);
		}

		[Test]
		public void GetNextFromQueueOfReportsToDownload_CalledWithNullMerchantId_ReturnsNull()
		{
			// Arrange
			var data = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = _merchantId,
					Id = 2,
					RequestReportId = "Report1",
					GeneratedReportId = null
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 3,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 4,
					RequestReportId = "Report2",
					GeneratedReportId = "GeneratedIdTest2"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe, MerchantId = null,
					Id = 5,
					RequestReportId = "Report3",
					GeneratedReportId = "GeneratedIdTest3"
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica, MerchantId = null,
					Id = 6,
					RequestReportId = "Report4",
					GeneratedReportId = null
				}
			};
			_reportRequestCallbacks.AddRange(data);

			_requestReportProcessor = new RequestReportProcessor(_region, null, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);

			var result = _requestReportProcessor.GetNextFromQueueOfReportsToDownload(_reportRequestCallbackServiceMock.Object);

			Assert.IsNull(result);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_ShouldCallMwsGetReportOnce()
		{
			// Arrange
			var merchantId = "testMerchantId";
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			var reportRequestCallback = new ReportRequestEntry(serializedReportRequestData)
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
			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object, testData);

			// Assert
			_marketplaceWebServiceClientMock.Verify(x => x.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithDownloadSuccessfulAndValidHash_ReportContentSavedInDbAsZip()
		{
			var expectedReportContent = StreamHelper.CreateMemoryStream("This is some test content. Und die Katze läuft auf der Straße.");
			var expectedMd5HashValue = MD5ChecksumHelper.ComputeHashForAmazon(expectedReportContent);

			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Callback<GetReportRequest>((req) => { expectedReportContent.CopyTo(req.Report); })
				.Returns(new GetReportResponse { GetReportResult = new GetReportResult { ContentMD5 = expectedMd5HashValue } });

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.NotNull(reportRequestEntryBeingUpdated.Details);

			expectedReportContent.Position = 0;
			using (var expectedReportReader = new StreamReader(expectedReportContent))
			using (var actualReportReader = new StreamReader(ExtractArchivedSingleFileToStream(reportRequestEntryBeingUpdated.Details.ReportContent)))
			{
				Assert.AreEqual(expectedReportReader.ReadToEnd(), actualReportReader.ReadToEnd());
			}

			Assert.AreEqual(0, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithDownloadSuccessfulAndInvalidHash_RetryCountIncremented()
		{
			var expectedReportContent = StreamHelper.CreateMemoryStream("This is some test content. Und die Katze läuft auf der Straße.");
			var nonMatchingMd5HashValue = $"{MD5ChecksumHelper.ComputeHashForAmazon(expectedReportContent)} non_matching_seq" ;

			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Callback<GetReportRequest>((req) => { expectedReportContent.CopyTo(req.Report); })
				.Returns(new GetReportResponse { GetReportResult = new GetReportResult { ContentMD5 = nonMatchingMd5HashValue } });

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithAmazonReturningFatalExceptionResponse_EntryIsDeletedFromDb()
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, "ReportNoLongerAvailable", "errorType", "123", "xml", new ResponseHeaderMetadata()));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithAmazonReturningNonFatalExceptionResponse_RetryCountIncremented()
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, "ReportNotReady", "errorType", "123", "xml", new ResponseHeaderMetadata()));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithArbitraryExceptionThrownWhenCallingAmazon_RetryCountIncremented()
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new Exception("Random exception thrown during download attempt"));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastRequested = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastRequested.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void CleanupReportRequests_DeletesReportRequests_WithRetryCountAboveMaxRetryCount()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			var testReportRequestCallbacks = new List<ReportRequestEntry>
			{
				new ReportRequestEntry {Id = 1, ReportRequestRetryCount = 0, ReportRequestData = serializedReportRequestData, DateCreated = DateTime.UtcNow.AddDays(-2), AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 2, ReportRequestRetryCount = 1, ReportRequestData = serializedReportRequestData, DateCreated = DateTime.UtcNow.AddDays(-1).AddHours(-1), AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 3, ReportRequestRetryCount = 2, ReportRequestData = serializedReportRequestData, DateCreated = DateTime.UtcNow.AddDays(-1).AddHours(1),  AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 4, ReportRequestRetryCount = 3, ReportRequestData = serializedReportRequestData, DateCreated = DateTime.UtcNow.AddHours(-6), AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 5, ReportRequestRetryCount = 4, ReportRequestData = serializedReportRequestData, DateCreated = DateTime.UtcNow.AddHours(-6), AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 6, ReportRequestRetryCount = 5, ReportRequestData = serializedReportRequestData, DateCreated = DateTime.UtcNow.AddHours(-6), AmazonRegion = _region, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 7, ReportRequestRetryCount = 5, ReportRequestData = serializedReportRequestData, AmazonRegion = AmazonRegion.Brazil, MerchantId = _merchantId },
				new ReportRequestEntry {Id = 8, ReportRequestRetryCount = 5, ReportRequestData = serializedReportRequestData, AmazonRegion = _region, MerchantId = "someDifferentMerchantId" }
			}.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(testReportRequestCallbacks);

			_requestReportProcessor.CleanupReportRequests(_reportRequestCallbackServiceMock.Object);

			// Id=6 deleted - ReportRequestMaxRetryCount. Id=1,2 deleted ReportDownloadRequestEntryExpirationPeriod=1day exceeded.
			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Exactly(3));
		}

		private static MemoryStream ExtractArchivedSingleFileToStream(byte[] zipArchive)
		{
			if (zipArchive == null) return null;

			using (var archiveStream = new MemoryStream(zipArchive))
			using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read))
			{
				var file = zip.Entries.FirstOrDefault();
				var resultStream = new MemoryStream();
				file?.Open()?.CopyTo(resultStream);
				resultStream.Position = 0;
				return resultStream;
			}
		}
	}
}
