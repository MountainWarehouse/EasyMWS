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
		private Mock<IReportRequestEntryService> _reportRequestServiceMock;
		private List<ReportRequestEntry> _reportRequestCallbacks;
		private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
		private Mock<IEasyMwsLogger> _loggerMock;
		private EasyMwsOptions _easyMwsOptions;

		[SetUp]
		public void SetUp()
		{
			_easyMwsOptions = new EasyMwsOptions();

			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
			_reportRequestServiceMock = new Mock<IReportRequestEntryService>();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_requestReportProcessor = new RequestReportProcessor(_region, _merchantId, _marketplaceWebServiceClientMock.Object, _loggerMock.Object, _easyMwsOptions);
			
			_reportRequestCallbacks = new List<ReportRequestEntry>
			{
				new ReportRequestEntry
				{
					AmazonRegion = AmazonRegion.China,
					Id = 0,
					RequestReportId = null,
					GeneratedReportId = "generatedReportId",
					ReportRequestData = "{\"UpdateFrequency\":0,\"Merchant\":null,\"MwsAuthToken\":null,\"Region\":20,\"MarketplaceIdList\":null}",
					ReportType = "_GET_AFN_INVENTORY_DATA_",
					MerchantId = _merchantId
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

			_reportRequestServiceMock.Setup(x => x.Where(It.IsAny<Func<ReportRequestEntry, bool>>()))
				.Returns((Func<ReportRequestEntry, bool> e) => reportRequestCallbacks.Where(e));

			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(reportRequestCallbacks);

			_marketplaceWebServiceClientMock.Setup(x => x.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(requestReportRequest);

			_marketplaceWebServiceClientMock.Setup(x => x.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()))
				.Returns(getReportRequestListResponse);

			_reportRequestServiceMock
				.Setup(x => x.FirstOrDefault(It.IsAny<Func<ReportRequestEntry, bool>>()))
				.Returns((Func<ReportRequestEntry, bool> e) => reportRequestCallbacks.FirstOrDefault(e));

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
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
				{
					reportRequestEntryBeingUpdated = entry;
				}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					MerchantId = _merchantId
				});

			Assert.AreEqual("testReportRequestId", reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(0, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[TestCase(null)]
		[TestCase("")]
		public void RequestReportFromAmazon_WithRequestReportAmazonResponseNull_CallsOnce_AllocateReportRequestForRetry(string reportRequestId)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Returns(new RequestReportResponse{RequestReportResult = new RequestReportResult{ ReportRequestInfo = new ReportRequestInfo{ReportRequestId = reportRequestId } }});
			var reportRequestEntryBeingUpdated = (ReportRequestEntry) null;
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
				{
					reportRequestEntryBeingUpdated = entry;
				}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					MerchantId = _merchantId
				});

			Assert.AreEqual(reportRequestId, reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
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

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					MerchantId = _merchantId
				});

			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
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
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1,
					MerchantId = _merchantId
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithAmazonReturningUnknownErrorCodeResponse_IncrementsRetryCountCorrectly()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, "UnknownErrorCode", "errorType", "123", "xml", new ResponseHeaderMetadata()));
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1,
					MerchantId = _merchantId
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void RequestReportFromAmazon_WithAmazonCallThrowingNonMarketplaceWebServiceException_IncrementsRetryCountCorrectly()
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);

			_marketplaceWebServiceClientMock.Setup(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()))
				.Throws(new Exception(""));
			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportRequestData = serializedReportRequestData,
					ReportType = "testReportType",
					ReportRequestRetryCount = 1,
					MerchantId = _merchantId
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.RequestReportId);
			Assert.AreEqual(2, reportRequestEntryBeingUpdated.ReportRequestRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
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

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object, _reportRequestCallbacks[0]);
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

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object, reportRequestEntry);

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

			_requestReportProcessor.RequestReportFromAmazon(_reportRequestServiceMock.Object, reportRequestCallback);

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
        public void QueueReportsAccordingToProcessingStatus_InvokeCallbackForReportStatusDoneNoDataTrue_DoesNotDeleteEntry()
        {
            _easyMwsOptions.InvokeCallbackForReportStatusDoneNoData = true;
            var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
            var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);
            var data = new List<ReportRequestEntry>
            {
                new ReportRequestEntry
                {
                    AmazonRegion = AmazonRegion.Europe,
                    Id = 2,
                    RequestReportId = "Report2",
                    GeneratedReportId = null,
                    ReportRequestData = serializedReportRequestData
                }
            };

            _reportRequestCallbacks.AddRange(data);

            var dataResult = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>
            {
                ("Report2", "GeneratedId2", "_DONE_NO_DATA_")
            };

            _requestReportProcessor.QueueReportsAccordingToProcessingStatus(_reportRequestServiceMock.Object, dataResult);

            Assert.IsNull(_reportRequestCallbacks.First(x => x.RequestReportId == "Report2").GeneratedReportId);

            Assert.IsNull(_reportRequestCallbacks.First(x => x.Id == 2).GeneratedReportId);
            Assert.AreEqual(0, _reportRequestCallbacks.First(x => x.Id == 2).ReportProcessRetryCount);
            _reportRequestServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
            _reportRequestServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
        }

        [Test]
        public void QueueReportsAccordingToProcessingStatus_InvokeCallbackForReportStatusDoneNoDataFalse_DeletesEntry()
        {
            _easyMwsOptions.InvokeCallbackForReportStatusDoneNoData = false;
            var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
            var serializedReportRequestData = JsonConvert.SerializeObject(propertiesContainer);
            var data = new List<ReportRequestEntry>
            {
                new ReportRequestEntry
                {
                    AmazonRegion = AmazonRegion.Europe,
                    Id = 2,
                    RequestReportId = "Report2",
                    GeneratedReportId = null,
                    ReportRequestData = serializedReportRequestData
                }
            };

            _reportRequestCallbacks.AddRange(data);

            var dataResult = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>
            {
                ("Report2", "GeneratedId2", "_DONE_NO_DATA_")
            };

            _requestReportProcessor.QueueReportsAccordingToProcessingStatus(_reportRequestServiceMock.Object, dataResult);

            _reportRequestServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
            _reportRequestServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
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

			_requestReportProcessor.QueueReportsAccordingToProcessingStatus(_reportRequestServiceMock.Object, dataResult);

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
			_reportRequestServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Exactly(5));
			_reportRequestServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
		}

		[Test]
		public void QueueReportsAccordingToProcessingStatus_UpdateReportRequestId()
		{
			_reportRequestCallbacks.First().RequestReportId = "Report3";
			_reportRequestCallbacks.First().GeneratedReportId = null;

			var data = new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>
			{
				("Report1", "GeneratedId1", "_DONE_"),
				("Report2", "GeneratedId2", "_DONE_"),
				("Report3", null, "_CANCELLED_"),
				("Report4", null, "_OTHER_")
			};

			_requestReportProcessor.QueueReportsAccordingToProcessingStatus(_reportRequestServiceMock.Object, data);

			Assert.IsNull(_reportRequestCallbacks.First().RequestReportId);
			Assert.IsTrue(_reportRequestCallbacks.First().ReportProcessRetryCount > 0);
            Assert.AreEqual("_CANCELLED_", _reportRequestCallbacks.First().LastAmazonReportProcessingStatus);
			_reportRequestServiceMock.Verify(x => x.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
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
				GeneratedReportId = "GeneratedIdTest1",
				MerchantId = merchantId
			};

			_reportRequestCallbacks.Add(reportRequestCallback);

			// Act
			var testData = _reportRequestCallbacks.Find(x => x.GeneratedReportId == "GeneratedIdTest1");
			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestServiceMock.Object, testData);

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
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null,
					GeneratedReportId = "generatedReportId",
					MerchantId = _merchantId
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
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
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
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null,
					GeneratedReportId = "generatedReportId",
					MerchantId = _merchantId
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportDownloadRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
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
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null,
					GeneratedReportId = "generatedReportId",
					MerchantId = _merchantId
				});

			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[TestCase("ReportNotReady")]
		[TestCase("InvalidScheduleFrequency")]
		[TestCase("SomeUnhandledNewErrorCode")]
		public void DownloadGeneratedReportFromAmazon_WithAmazonReturningNonFatalExceptionResponse_RetryCountIncremented(string nonFatalErrorCode)
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new MarketplaceWebServiceException("message", HttpStatusCode.BadRequest, nonFatalErrorCode, "errorType", "123", "xml", new ResponseHeaderMetadata()));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null,
					GeneratedReportId = "generatedReportId",
					MerchantId = _merchantId
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportDownloadRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
		}

		[Test]
		public void DownloadGeneratedReportFromAmazon_WithArbitraryExceptionThrownWhenCallingAmazon_RetryCountIncremented()
		{
			_marketplaceWebServiceClientMock.Setup(mws => mws.GetReport(It.IsAny<GetReportRequest>()))
				.Throws(new Exception("Random exception thrown during download attempt"));

			var reportRequestEntryBeingUpdated = (ReportRequestEntry)null;
			_reportRequestServiceMock.Setup(rrp => rrp.Update(It.IsAny<ReportRequestEntry>())).Callback<ReportRequestEntry>(((entry) =>
			{
				reportRequestEntryBeingUpdated = entry;
			}));

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(_reportRequestServiceMock.Object,
				new ReportRequestEntry
				{
					LastAmazonRequestDate = DateTime.MinValue,
					ReportType = "testReportType",
					Details = null,
					GeneratedReportId = "genReportId",
					MerchantId = _merchantId
				});

			Assert.IsNull(reportRequestEntryBeingUpdated.Details);
			Assert.AreEqual(1, reportRequestEntryBeingUpdated.ReportDownloadRetryCount);
			Assert.AreEqual(DateTime.UtcNow.Day, reportRequestEntryBeingUpdated.LastAmazonRequestDate.Day);
			_reportRequestServiceMock.Verify(rrp => rrp.Update(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(rrp => rrp.Delete(It.IsAny<ReportRequestEntry>()), Times.Never);
			_reportRequestServiceMock.Verify(rrp => rrp.SaveChanges(), Times.Once);
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
			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(testReportRequestCallbacks);

			_requestReportProcessor.CleanupReportRequests(_reportRequestServiceMock.Object);

			// Id=6 deleted - ReportRequestMaxRetryCount. Id=1,2 deleted ReportDownloadRequestEntryExpirationPeriod=1day exceeded.
			_reportRequestServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Exactly(3));
		}

		private const int RetryCountIsStrictlyBelowConfiguredRetryCountLimit = -2;
		private const int RetryCountIsBelowConfiguredRetryCountLimitEdge = -1;
		private const int RetryCountIsEqualToConfiguredRetryCountLimit = 0;
		private const int RetryCountIsAboveConfiguredRetryCountLimitEdge = 1;
		private const int RetryCountIsStrictlyAboveConfiguredRetryCountLimit = 2;

		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, 0)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, 0)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, 1)]
		public void CleanupReportRequests_OneEntryWithRequestRetryCountExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, int numberOfDeleteCalls)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -1 ? _easyMwsOptions.ReportRequestMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.ReportRequestMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.ReportRequestMaxRetryCount + 1 : 0;

			var firstEntryToDelete = new ReportRequestEntry
			{
				Id = 1,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow,
				ReportDownloadRetryCount = 0,
				ReportProcessRetryCount = 0,
				InvokeCallbackRetryCount = 0,
				ReportRequestRetryCount = retryCount
			};
			var entryToLeaveIntact1 = new ReportRequestEntry
			{
				Id = 2, ReportRequestData = data, AmazonRegion = _region, MerchantId = _merchantId, DateCreated = DateTime.UtcNow,
				ReportDownloadRetryCount = 0, ReportProcessRetryCount = 0, InvokeCallbackRetryCount = 0, ReportRequestRetryCount = 0
			};
			var entriesList = new List<ReportRequestEntry> { firstEntryToDelete, entryToLeaveIntact1 };
			var entriesQueryable = entriesList.AsQueryable();
			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			_reportRequestServiceMock.Setup(x => x.Delete(It.IsAny<ReportRequestEntry>()));


			_requestReportProcessor.CleanupReportRequests(_reportRequestServiceMock.Object);
			_reportRequestServiceMock.Verify(x => x.Delete(It.Is<ReportRequestEntry>(e => e.Id == firstEntryToDelete.Id)), Times.Exactly(numberOfDeleteCalls));
			_reportRequestServiceMock.Verify(x => x.SaveChanges(), Times.Exactly(numberOfDeleteCalls));
		}

		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, 0)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, 0)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, 1)]
		public void CleanupReportRequests_OneEntryWithDownloadRetryCountExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, int numberOfDeleteCalls)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -1 ? _easyMwsOptions.ReportDownloadMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.ReportDownloadMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.ReportDownloadMaxRetryCount + 1 : 0;

			var firstEntryToDelete = new ReportRequestEntry
			{
				Id = 1,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow,
				ReportDownloadRetryCount = retryCount,
				ReportProcessRetryCount = 0,
				InvokeCallbackRetryCount = 0,
				ReportRequestRetryCount = 0
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
			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			_reportRequestServiceMock.Setup(x => x.Delete(It.IsAny<ReportRequestEntry>()));


			_requestReportProcessor.CleanupReportRequests(_reportRequestServiceMock.Object);
			_reportRequestServiceMock.Verify(x => x.Delete(It.Is<ReportRequestEntry>(e => e.Id == firstEntryToDelete.Id)), Times.Exactly(numberOfDeleteCalls));
			_reportRequestServiceMock.Verify(x => x.SaveChanges(), Times.Exactly(numberOfDeleteCalls));
		}

		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, 0)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, 0)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, 1)]
		public void CleanupReportRequests_OneEntryWithProcessRetryCountExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, int numberOfDeleteCalls)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -1 ? _easyMwsOptions.ReportProcessingMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.ReportProcessingMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.ReportProcessingMaxRetryCount + 1 : 0;

			var firstEntryToDelete = new ReportRequestEntry
			{
				Id = 1,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow,
				ReportDownloadRetryCount = 0,
				ReportProcessRetryCount = retryCount,
				InvokeCallbackRetryCount = 0,
				ReportRequestRetryCount = 0
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
			var entriesList = new List<ReportRequestEntry> { firstEntryToDelete, entryToLeaveIntact};
			var entriesQueryable = entriesList.AsQueryable();
			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			_reportRequestServiceMock.Setup(x => x.Delete(It.IsAny<ReportRequestEntry>()));


			_requestReportProcessor.CleanupReportRequests(_reportRequestServiceMock.Object);
			_reportRequestServiceMock.Verify(x => x.Delete(It.Is<ReportRequestEntry>(e => e.Id == firstEntryToDelete.Id)), Times.Exactly(numberOfDeleteCalls));
			_reportRequestServiceMock.Verify(x => x.SaveChanges(), Times.Exactly(numberOfDeleteCalls));
		}

		[TestCase(RetryCountIsBelowConfiguredRetryCountLimitEdge, 0)]
		[TestCase(RetryCountIsEqualToConfiguredRetryCountLimit, 0)]
		[TestCase(RetryCountIsAboveConfiguredRetryCountLimitEdge, 1)]
		public void CleanupReportRequests_OneEntryWithInvokeCallbackRetryCountExceeded_DeletesOnlyTheCorrectEntry(int retryCountType, int numberOfDeleteCalls)
		{
			var propertiesContainer = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Unknown);
			var data = JsonConvert.SerializeObject(propertiesContainer);

			var retryCount =
				retryCountType == -1 ? _easyMwsOptions.InvokeCallbackMaxRetryCount - 1 :
				retryCountType == 0 ? _easyMwsOptions.InvokeCallbackMaxRetryCount :
				retryCountType == 1 ? _easyMwsOptions.InvokeCallbackMaxRetryCount + 1 : 0;

			var firstEntryToDelete = new ReportRequestEntry
			{
				Id = 1,
				ReportRequestData = data,
				AmazonRegion = _region,
				MerchantId = _merchantId,
				DateCreated = DateTime.UtcNow,
				ReportDownloadRetryCount = 0,
				ReportProcessRetryCount = 0,
				InvokeCallbackRetryCount = retryCount,
				ReportRequestRetryCount = 0
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
			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);
			_reportRequestServiceMock.Setup(x => x.Delete(It.IsAny<ReportRequestEntry>()));


			_requestReportProcessor.CleanupReportRequests(_reportRequestServiceMock.Object);
			_reportRequestServiceMock.Verify(x => x.Delete(It.Is<ReportRequestEntry>(e => e.Id == firstEntryToDelete.Id)), Times.Exactly(numberOfDeleteCalls));
			_reportRequestServiceMock.Verify(x => x.SaveChanges(), Times.Exactly(numberOfDeleteCalls));
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
			_reportRequestServiceMock.Setup(x => x.GetAll()).Returns(entriesQueryable);

			_requestReportProcessor.CleanupReportRequests(_reportRequestServiceMock.Object);

			_reportRequestServiceMock.Verify(x => x.Delete(It.IsAny<ReportRequestEntry>()), Times.Once);
			_reportRequestServiceMock.Verify(x => x.SaveChanges(), Times.Once);
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
