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
		private Mock<IReportRequestEntryService> _reportRequestCallbackServiceMock;
		private List<ReportRequestEntry> _reportRequestCallbacks;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private EasyMwsOptions _easyMwsOptions;

		[SetUp]
		public void SetUp()
		{
			_easyMwsOptions = EasyMwsOptions.Defaults();

			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_reportRequestCallbackServiceMock = new Mock<IReportRequestEntryService>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			
			_reportRequestCallbacks = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.China,
					Id = 0,
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
		public void RequestReportFromAmazon_CalledWithNullReportRequestCallback_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestReportFromAmazon(It.IsAny<IReportRequestEntryService>(), null));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestCallbackWithNullReportRequestData_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => _requestReportProcessor.RequestReportFromAmazon(It.IsAny<IReportRequestEntryService>(), new ReportRequestEntry()));
		}

		[Test]
		public void RequestReportFromAmazon_CalledWithReportRequestDataWithNoReportType_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentException>(() => _requestReportProcessor.RequestReportFromAmazon(It.IsAny<IReportRequestEntryService>(), new ReportRequestEntry {ReportRequestData = String.Empty}));
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			Assert.AreEqual("testReportRequestId", reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(0, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			Assert.AreEqual(null, reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			Assert.AreEqual(string.Empty, reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[TestCase("AccessToReportDenied")]
		[TestCase("InvalidReportId")]
		[TestCase("InvalidReportType")]
		[TestCase("InvalidRequest")]
		[TestCase("ReportNoLongerAvailable")]
		public void RequestReportFromAmazon_WithAmazonReturningFatalErrorCodeResponse_DeletesReportRequestEntryFromQueue(string fatalErrorCode)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, fatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType"
				});

			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[TestCase("ReportNotReady")]
		[TestCase("InvalidScheduleFrequency")]
		[TestCase("SomeUnhandledNewErrorCode")]
		public void RequestReportFromAmazon_WithAmazonReturningNonFatalErrorCodeResponse_IncrementsRetryCountCorrectly(string nonFatalErrorCode)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, nonFatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					Id = 1,
					RequestReportId = "Report1",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 2,
					RequestReportId = "Report2",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 3,
					RequestReportId = "Report3",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				},
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 4,
					RequestReportId = "Report4",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
				,
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 5,
					RequestReportId = "Report5",
					GeneratedReportId = null,
					ReportRequestData = serializedReportRequestData
				}
				,
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.NorthAmerica,
					Id = 6,
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
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report1").ReportProcessRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report2").GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report3").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report3").ReportProcessRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report4").GeneratedReportId);
			Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.RequestReportId == "Report4").ReportProcessRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 5).GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 5).RequestReportId);
			Assert.AreEqual(1, _reportRequestCallbacks.First(x => x.Id == 5).ReportProcessRetryCount);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 6).GeneratedReportId);
			Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 6).RequestReportId);
			Assert.AreEqual(1, _reportRequestCallbacks.First(x => x.Id == 6).ReportProcessRetryCount);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Exactly(3));
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
			Assert.IsTrue(_reportRequestCallbacks.First().ReportProcessRetryCount > 0);
			_reportRequestCallbackServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
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
					LastAmazonRequestDate = DateTime.MinValue,
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
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportDownloadRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[TestCase("AccessToReportDenied")]
		[TestCase("InvalidReportId")]
		[TestCase("InvalidReportType")]
		[TestCase("InvalidRequest")]
		[TestCase("ReportNoLongerAvailable")]
		public void DownloadGeneratedReportFromAmazon_WithAmazonReturningFatalExceptionResponse_EntryIsDeletedFromDb(string fatalErrorCode)
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, fatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[TestCase("ReportNotReady")]
		[TestCase("InvalidScheduleFrequency")]
		[TestCase("SomeUnhandledNewErrorCode")]
		public void DownloadGeneratedReportFromAmazon_WithAmazonReturningNonFatalExceptionResponse_RetryCountIncremented(string nonFatalErrorCode)
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, nonFatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestCallbackServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportDownloadRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportDownloadRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
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

		[TestCase(0, 0, 0, 0, 0)]
		[TestCase(1, 0, 0, 0, 0)]
		[TestCase(4, 0, 0, 0, 0)]
		[TestCase(5, 0, 0, 0, 1)]
		[TestCase(0, 0, 0, 0, 0)]
		[TestCase(0, 1, 0, 0, 0)]
		[TestCase(0, 4, 0, 0, 0)]
		[TestCase(0, 5, 0, 0, 1)]
		[TestCase(0, 0, 0, 0, 0)]
		[TestCase(0, 0, 1, 0, 0)]
		[TestCase(0, 0, 5, 0, 0)]
		[TestCase(0, 0, 6, 0, 1)]
		[TestCase(0, 0, 0, 0, 0)]
		[TestCase(0, 0, 0, 1, 0)]
		[TestCase(0, 0, 0, 3, 0)]
		[TestCase(0, 0, 0, 4, 1)]
		public void CleanupReportRequests_WithOneGivenEntryToDelete_DeletesOnlyTheCorrectEntry(int requestRetryCount, int downloadRetryCount, int callbackRetryCount, int processRetryCount, int timesToDelete)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);
			var firstEntryToDelete = new ReportRequestEntry
			{
				Id = 1, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				ReportDownloadRetryCount = downloadRetryCount,
				ReportProcessRetryCount = processRetryCount,
				InvokeCallbackRetryCount = callbackRetryCount,
				ReportRequestRetryCount = requestRetryCount
			};
			var entryToLeaveIntact = new ReportRequestEntry
			{
				Id = 2,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				ReportDownloadRetryCount = 0,
				ReportProcessRetryCount = 0,
				InvokeCallbackRetryCount = 0,
				ReportRequestRetryCount = 0,
				DateCreated = DateTime.UtcNow
			};
			var entriesList = new List<ReportRequestEntry> { firstEntryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			var entryBeingDeleted = (ReportRequestEntry)null;
			_reportRequestCallbackServiceMock.Setup(x => x.Delete(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(
				e => { entryBeingDeleted = e; });
			

			_requestReportProcessor.CleanupReportRequests(_reportRequestCallbackServiceMock.Object);

			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Exactly(timesToDelete));
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.Once);
			if (timesToDelete > 0)
			{
				Assert.NotNull(entryBeingDeleted);
				Assert.AreEqual(firstEntryToDelete.Id, entryBeingDeleted.Id);
			}
		}

		[TestCase(false, false, false, false, false)]
		[TestCase(true, true, true, true, true)]
		[TestCase(true, true, false, false, false)]
		[TestCase(true, false, true, false, false)]
		[TestCase(true, false, false, true, false)]
		[TestCase(true, false, false, false, true)]
		[TestCase(false, true, true, false, false)]
		[TestCase(false, true, false, true, false)]
		[TestCase(false, true, false, false, true)]
		[TestCase(false, false, true, true, false)]
		[TestCase(false, false, true, false, true)]
		[TestCase(false, false, false, true, true)]
		[TestCase(true, true, true, false, false)]
		[TestCase(true, true, false, true, false)]
		[TestCase(true, true, false, false, true)]
		[TestCase(true, false, true, true, false)]
		[TestCase(true, false, true, false, true)]
		[TestCase(true, false, false, true, true)]
		[TestCase(false, true, true, true, false)]
		[TestCase(false, true, true, false, true)]
		[TestCase(false, false, true, true, true)]
		[TestCase(true, true, true, true, false)]
		[TestCase(true, true, true, false, true)]
		[TestCase(true, true, false, true, true)]
		[TestCase(true, false, true, true, true)]
		[TestCase(false, true, true, true, true)]
		public void CleanupReportRequests_WithPotentiallyMultiplesEntriesToDelete_DeletesOnlyTheCorrectEntries(
			bool hasEntryWithRequestRetryCountExceeded,
			bool hasEntryWithDownloadRetryCountExceeded,
			bool hasEntryWithCallbackRetryCountExceeded,
			bool hasEntryWithProcessRetryCountExceeded,
			bool hasEntryWithExpirationPeriodExceeded)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);
			var entryToLeaveIntact = new ReportRequestEntry
			{
				Id = 1,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				ReportDownloadRetryCount = 0,
				ReportProcessRetryCount = 0,
				InvokeCallbackRetryCount = 0,
				ReportRequestRetryCount = 0,
				DateCreated = DateTime.UtcNow
			};
			var entryWithRequestRetryCountExceeded = new ReportRequestEntry { Id = 2, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 0, ReportProcessRetryCount = 0, InvokeCallbackRetryCount = 0, ReportRequestRetryCount = 10 };
			var entryWithDownloadRetryCountExceeded = new ReportRequestEntry { Id = 3, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 10, ReportProcessRetryCount = 0, InvokeCallbackRetryCount = 0, ReportRequestRetryCount = 0 };
			var entryWithCallbackRetryCountExceeded = new ReportRequestEntry { Id = 4, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 0, ReportProcessRetryCount = 0, InvokeCallbackRetryCount = 10, ReportRequestRetryCount = 0 };
			var entryWithProcessRetryCountExceeded = new ReportRequestEntry { Id = 5, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow, ReportDownloadRetryCount = 0, ReportProcessRetryCount = 10, InvokeCallbackRetryCount = 0, ReportRequestRetryCount = 0 };
			var entryWithExpirationPeriodExceeded = new ReportRequestEntry { Id = 6, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow.AddDays(-10), ReportDownloadRetryCount = 0, ReportProcessRetryCount = 0, InvokeCallbackRetryCount = 0, ReportRequestRetryCount = 0 };

			var expectedNumberOfEntitiesToDelete = 0;
			var entriesList = new List<ReportRequestEntry> { entryToLeaveIntact };
			if (hasEntryWithRequestRetryCountExceeded)
			{
				entriesList.Add(entryWithRequestRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithDownloadRetryCountExceeded)
			{
				entriesList.Add(entryWithDownloadRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithCallbackRetryCountExceeded)
			{
				entriesList.Add(entryWithCallbackRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithProcessRetryCountExceeded)
			{
				entriesList.Add(entryWithProcessRetryCountExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			if (hasEntryWithExpirationPeriodExceeded)
			{
				entriesList.Add(entryWithExpirationPeriodExceeded);
				expectedNumberOfEntitiesToDelete++;
			}
			var entriesQueryable = entriesList.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			var listOfDeletedEntriesIds = new List<int>();
			_reportRequestCallbackServiceMock.Setup(x => x.Delete(It.IsAny<ReportRequestEntry>()))
				.Callback<ReportRequestEntry>(entry => { listOfDeletedEntriesIds.Add(entry.Id); });


			_requestReportProcessor.CleanupReportRequests(_reportRequestCallbackServiceMock.Object);

			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Exactly(expectedNumberOfEntitiesToDelete));
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.Once);
			CollectionAssert.DoesNotContain(listOfDeletedEntriesIds, entryToLeaveIntact.Id);
			if (hasEntryWithRequestRetryCountExceeded) { CollectionAssert.Contains(listOfDeletedEntriesIds, entryWithRequestRetryCountExceeded.Id); }
			else { CollectionAssert.DoesNotContain(listOfDeletedEntriesIds, entryWithRequestRetryCountExceeded.Id); }
			if (hasEntryWithDownloadRetryCountExceeded) { CollectionAssert.Contains(listOfDeletedEntriesIds, entryWithDownloadRetryCountExceeded.Id); }
			else { CollectionAssert.DoesNotContain(listOfDeletedEntriesIds, entryWithDownloadRetryCountExceeded.Id); }
			if (hasEntryWithCallbackRetryCountExceeded) { CollectionAssert.Contains(listOfDeletedEntriesIds, entryWithCallbackRetryCountExceeded.Id); }
			else { CollectionAssert.DoesNotContain(listOfDeletedEntriesIds, entryWithCallbackRetryCountExceeded.Id); }
			if (hasEntryWithProcessRetryCountExceeded) { CollectionAssert.Contains(listOfDeletedEntriesIds, entryWithProcessRetryCountExceeded.Id); }
			else { CollectionAssert.DoesNotContain(listOfDeletedEntriesIds, entryWithProcessRetryCountExceeded.Id); }
			if (hasEntryWithExpirationPeriodExceeded) { CollectionAssert.Contains(listOfDeletedEntriesIds, entryWithExpirationPeriodExceeded.Id); }
			else { CollectionAssert.DoesNotContain(listOfDeletedEntriesIds, entryWithExpirationPeriodExceeded.Id); }
		}

		[Test]
		public void CleanupReportRequests_WithMultipleReasonsToDeleteOneEntry_OnlyCallsDeleteOneSingleTime()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);
			var entryToDelete = new ReportRequestEntry
			{
				Id = 1,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow.AddDays(-10),
				ReportDownloadRetryCount = 10,
				ReportProcessRetryCount = 10,
				InvokeCallbackRetryCount = 10,
				ReportRequestRetryCount = 10
			};
			var entryToLeaveIntact = new ReportRequestEntry
			{
				Id = 2,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				ReportDownloadRetryCount = 0,
				ReportProcessRetryCount = 0,
				InvokeCallbackRetryCount = 0,
				ReportRequestRetryCount = 0,
				DateCreated = DateTime.UtcNow
			};
			var entriesList = new List<ReportRequestEntry> { entryToDelete, entryToLeaveIntact };
			var entriesQueryable = entriesList.AsQueryable();
			_reportRequestCallbackServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);

			_requestReportProcessor.CleanupReportRequests(_reportRequestCallbackServiceMock.Object);

			_reportRequestCallbackServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestCallbackServiceMock.Verify(x => x.SaveChanges(), Times.Once);
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
