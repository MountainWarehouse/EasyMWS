using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Moq;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.EndToEnd
{
    public class ReportDownloadingTests
    {
	    private EasyMwsContext _dbContext;
	    private const string _testEntriesIdentifier = "TEST";

		private IEasyMwsClient _easyMwsClient;
	    private readonly AmazonRegion _region = AmazonRegion.Europe;
	    private readonly string _merchantId = "testMerchantId";
	    private readonly EasyMwsOptions _options = EasyMwsOptions.Defaults();

		private Mock<IEasyMwsLogger> _loggerMock;
	    private Mock<IMarketplaceWebServiceClient> _mwsClientMock;

		[SetUp]
	    public void SetUp()
	    {
		    _dbContext = new EasyMwsContext();
		    _actualCallbackObject = null;

			_loggerMock = new Mock<IEasyMwsLogger>();
		    _mwsClientMock = new Mock<IMarketplaceWebServiceClient>();
		    var feedProcessorMock = new Mock<IFeedQueueingProcessor>();

			var reportProcessor =  new ReportProcessor(_region, _merchantId, _options, _mwsClientMock.Object, _loggerMock.Object);

			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "MerchantId", "test", "test", reportProcessor,
				feedProcessorMock.Object, _loggerMock.Object, EasyMwsOptions.Defaults());
		}

		[TearDown]
	    public void TearDown()
		{
			var testReportEntries = _dbContext.ReportRequestEntries.Where(rre => rre.ReportType.StartsWith(_testEntriesIdentifier));
			_dbContext.ReportRequestEntries.RemoveRange(testReportEntries);
			_dbContext.SaveChanges();

			_dbContext.Dispose();
		}

		private static object _actualCallbackObject;
	    public static void ReportDownloadCallback(Stream stream, object o)
	    {
		    _actualCallbackObject = o;
	    }

	    [Test]
	    public void
		    GivenQueuingReport_WhenCallbackDataIsNull_NullCallbackDataIsSerializedAndDeserializedSuccessfully_AndCallbackIsInvoked()
	    {
			// arrange
		    var validReportType = $"{_testEntriesIdentifier}_VALID_REPORT_TYPE_";
		    var expectedReportRequestId = "test report request Id";
		    var expectedGeneratedReportId = "test generated report Id";
		    var expectedReportProcessingStatus = "_DONE_";
		    var reportRequestContainer = GenerateReportContainer(validReportType);
			Setup_RequestReport_Returns_ReportRequestWasGenerated(validReportType, expectedReportRequestId);
		    Setup_GetReportRequestList_Returns_ReportsGeneratedSuccessfully(expectedReportRequestId, expectedGeneratedReportId, expectedReportProcessingStatus);
		    Setup_GetReport_Returns_ReportContentStream(expectedGeneratedReportId);
		    _actualCallbackObject = new object();

			// act - queue report
			_easyMwsClient.QueueReport(reportRequestContainer, ReportDownloadCallback, null);

		    var dbEntry = _dbContext.ReportRequestEntries.Where(rre => rre.ReportType == validReportType);

			// assert - null callback data serialization step does not crash
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("The following report was queued for download")),
			    It.IsAny<RequestInfo>()));
			Assert.NotNull(dbEntry);
			Assert.AreEqual(1, dbEntry.Count());
		    var reportRequestEntry = dbEntry.Single();

			Assert.Null(reportRequestEntry.Data);

			// act - Poll report request process, in order for the queuedReport delegate to be invoked.
			_easyMwsClient.Poll();

			// assert - null callback data deserialization step does not crash, and callback was invoked successfully
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.EndsWith("Executing cleanup of report requests queue.")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("Request to MWS.RequestReport was successful!")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("Attempting to request report processing statuses")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("Report download from Amazon has succeeded")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("Attempting to perform method callback")),
			    It.IsAny<RequestInfo>()));

			Assert.IsNull(_actualCallbackObject);
		    _mwsClientMock.Verify(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);
		    _mwsClientMock.Verify(mws => mws.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()), Times.Once);
		    _mwsClientMock.Verify(mws => mws.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
		}

	    [Test]
	    public void GivenReportWithInvalidType_WhenTryingToDownloadReport_ThenReportRequestIsDeletedFromDatabase()
	    {
			// arrange
		    var invalidReportType = $"{_testEntriesIdentifier}_INVALID_REPORT_TYPE_";
		    var expectedRequestId = "ExpectedRequestId_999";
		    var expectedExceptionMessage = "invalid report type";
		    var expectedExceptionStatusCode = HttpStatusCode.BadRequest;
		    var expectedExceptionErrorCode = "error code";
		    var expectedExceptionErrorType = "error type";

		    Setup_RequestReport_ThrowsInvalidReportTypeException(invalidReportType, expectedRequestId,
			    expectedExceptionStatusCode, expectedExceptionMessage, expectedExceptionErrorCode, expectedExceptionErrorType);
		    var reportRequestContainer = GenerateReportContainer(invalidReportType);

			// act
			_easyMwsClient.QueueReport(reportRequestContainer, ReportDownloadCallback, null);
		    _easyMwsClient.Poll();

			// assert
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.EndsWith("Executing cleanup of report requests queue.")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("Attempting to request the next report in queue")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Error(It.Is<string>(msg => msg.StartsWith($"Request to MWS.RequestReport failed! [HttpStatusCode:'{expectedExceptionStatusCode}'")),
			    It.IsAny<Exception>()));
			_loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.EndsWith("The report request was removed from queue.")),
			    It.IsAny<RequestInfo>()));
	    }


		#region Non testing code

		private ReportRequestPropertiesContainer GenerateReportContainer(string reportType)
		{
			return new ReportRequestPropertiesContainer(reportType, ContentUpdateFrequency.Unknown);
		}

		private void Setup_RequestReport_ThrowsInvalidReportTypeException(string reportType, string expectedRequestId, HttpStatusCode statusCode, string errorMessage, string errorCode, string errorType)
		{
			_mwsClientMock
				.Setup(mws => mws.RequestReport(It.Is<RequestReportRequest>(rrr => rrr.ReportType == reportType && rrr.Merchant == _merchantId)))
				.Throws(new MarketplaceWebServiceException(errorMessage, statusCode, errorCode, errorType,
					expectedRequestId, "xml", new ResponseHeaderMetadata()));
		}

		private void Setup_RequestReport_Returns_ReportRequestNotGeneratedYet(string reportType)
		{
			var response = new RequestReportResponse
			{
				ResponseHeaderMetadata = new ResponseHeaderMetadata("requestId", "responseContext", "timestamp"),
				RequestReportResult = new RequestReportResult { ReportRequestInfo = new ReportRequestInfo { ReportRequestId = null } }
			};

			_mwsClientMock
				.Setup(mws => mws.RequestReport(It.Is<RequestReportRequest>(rrr => rrr.ReportType == reportType && rrr.Merchant == _merchantId)))
				.Returns(response);
		}

		private void Setup_RequestReport_Returns_ReportRequestWasGenerated(string reportType, string expectedReportRequestId)
		{
			var response = new RequestReportResponse
			{
				ResponseHeaderMetadata = new ResponseHeaderMetadata("requestId", "responseContext", "timestamp"),
				RequestReportResult = new RequestReportResult { ReportRequestInfo = new ReportRequestInfo { ReportRequestId = expectedReportRequestId } }
			};

			_mwsClientMock
				.Setup(mws => mws.RequestReport(It.Is<RequestReportRequest>(rrr => rrr.ReportType == reportType && rrr.Merchant == _merchantId)))
				.Returns(response);
		}

		private void Setup_GetReportRequestList_Returns_ReportsGeneratedSuccessfully(string reportRequestId, string expectedGeneratedReportId, string expectedReportProcessingStatus)
		{
			var response = new GetReportRequestListResponse
			{
				ResponseHeaderMetadata = new ResponseHeaderMetadata("requestId", "responseContext", "timestamp"),
				GetReportRequestListResult = new GetReportRequestListResult
				{
					ReportRequestInfo = new List<ReportRequestInfo>
					{
						new ReportRequestInfo
						{
							ReportRequestId = reportRequestId,
							GeneratedReportId = expectedGeneratedReportId,
							ReportProcessingStatus = expectedReportProcessingStatus
						}
					}
				}
			};

			_mwsClientMock
				.Setup(mws => mws.GetReportRequestList(It.Is<GetReportRequestListRequest>(rrlr => rrlr.ReportRequestIdList.Id.Contains(reportRequestId) && rrlr.Merchant == _merchantId)))
				.Returns(response);
		}

		private void Setup_GetReport_Returns_ReportContentStream(string expectedGeneratedReportId)
		{
			var stream = StreamHelper.CreateNewMemoryStream("testReportContent");
			var response = new GetReportResponse
			{
				ResponseHeaderMetadata = new ResponseHeaderMetadata("requestId", "responseContext", "timestamp")
			};

			_mwsClientMock
				.Setup(mws => mws.GetReport(It.Is<GetReportRequest>(grr => grr.ReportId == expectedGeneratedReportId && grr.Merchant == _merchantId)))
				.Callback<GetReportRequest>(request => { request.Report = stream; })
				.Returns(response);
		}

		#endregion
	}
}
