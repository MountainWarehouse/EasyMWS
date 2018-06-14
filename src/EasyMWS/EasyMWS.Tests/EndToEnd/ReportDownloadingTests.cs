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
	    private EasyMwsOptions _options = EasyMwsOptions.Defaults();

		private Mock<IEasyMwsLogger> _loggerMock;
	    private Mock<IMarketplaceWebServiceClient> _mwsClientMock;

		[SetUp]
	    public void SetUp()
	    {
		    _options = EasyMwsOptions.Defaults();
			_dbContext = new EasyMwsContext();
		    _actualCallbackObject = null;
		    _actualReportContent = null;

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
	    private static Stream _actualReportContent;
	    public static void ReportDownloadCallback(Stream stream, object o)
	    {
		    _actualCallbackObject = o;
		    _actualReportContent = stream;
	    }

	    [Test]
	    public void
		    GivenQueuingReportAndPolling_WhenCallbackDataIsNull_NullCallbackDataIsSerializedAndDeserializedSuccessfully_AndCallbackIsInvoked()
	    {
			// arrange
		    var validReportType = $"{_testEntriesIdentifier}_VALID_REPORT_TYPE_";
		    var expectedGeneratedReportId = "test generated report Id";
		    var expectedReportProcessingStatus = "_DONE_";
		    var reportRequestContainer = GenerateReportContainer(validReportType);
			Setup_RequestReport_Returns_ReportRequestWasGenerated(validReportType);
		    Setup_GetReportRequestList_Returns_ReportsGeneratedSuccessfully(expectedGeneratedReportId, expectedReportProcessingStatus);
		    Setup_GetReport_Returns_ReportContentStream(expectedGeneratedReportId, "testReportContent");
		    _actualCallbackObject = new object();
		    _actualReportContent = null;

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
			Assert.IsNull(_actualCallbackObject);
			Assert.NotNull(_actualReportContent);
		    _mwsClientMock.Verify(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);
		    _mwsClientMock.Verify(mws => mws.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()), Times.Once);
		    _mwsClientMock.Verify(mws => mws.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
		}

		[Test]
		public void
			GivenQueuingReportAndPolling_WhenAllDataIsValid_TheReportIsDownloaded_AndIsReturnedInTheCallbackMethodAlongWithTheCallbackData()
		{
			// arrange
			var validReportType = $"{_testEntriesIdentifier}_VALID_REPORT_TYPE_";
			
			var expectedGeneratedReportId = "test generated report Id";
			var expectedReportProcessingStatus = "_DONE_";
			var reportRequestContainer = GenerateReportContainer(validReportType);
			var expectedCallbackData = ("test", "callback", "data");
			var expectedReportContent = "testReportContent";
			Setup_RequestReport_Returns_ReportRequestWasGenerated(validReportType);
			Setup_GetReportRequestList_Returns_ReportsGeneratedSuccessfully(expectedGeneratedReportId, expectedReportProcessingStatus);
			Setup_GetReport_Returns_ReportContentStream(expectedGeneratedReportId, expectedReportContent);
			_actualCallbackObject = null;
			_actualReportContent = null;
			

			// act - queue report
			_easyMwsClient.QueueReport(reportRequestContainer, ReportDownloadCallback, expectedCallbackData);

			// assert - null callback data serialization step does not crash
			_loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("The following report was queued for download")),
				It.IsAny<RequestInfo>()));

			// act - Poll report request process, in order for the queuedReport delegate to be invoked.
			_easyMwsClient.Poll();

			// assert - null callback data deserialization step does not crash, and callback was invoked successfully
			Assert.AreEqual(expectedCallbackData, _actualCallbackObject);
			Assert.AreEqual(expectedReportContent, _actualReportContent == null ? null : new StreamReader(_actualReportContent).ReadToEnd());
			_mwsClientMock.Verify(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()), Times.Once);
			_mwsClientMock.Verify(mws => mws.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()), Times.Once);
			_mwsClientMock.Verify(mws => mws.GetReport(It.IsAny<GetReportRequest>()), Times.Once);

			var dbEntry = _dbContext.ReportRequestEntries.FirstOrDefault(rre => rre.ReportType == validReportType);
			Assert.IsNull(dbEntry);
		}

		[Test]
	    public void GivenQueuingReportAndPolling_WhenReportHasAnInvalidType_AndRequestIsRejectedByAmazon_ThenReportRequestIsDeletedFromQueue()
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
			var dbEntry = _dbContext.ReportRequestEntries.FirstOrDefault(rre => rre.ReportType == invalidReportType);
		    Assert.IsNull(dbEntry);
		}

		[Test]
	    public void GivenRequestingAReportFromAmazon_WhenTheRequestFailsMaxRetryCountTimes_TheReportRequestEntryIsRemovedFromQueue()
	    {
		    var validReportType = $"{_testEntriesIdentifier}_VALID_REPORT_TYPE_";
		    Setup_RequestReport_Returns_ReportRequestNotGeneratedYet(validReportType);
		    var reportRequestContainer = GenerateReportContainer(validReportType);
			_options.ReportRequestRetryInitialDelay = TimeSpan.Zero;
			_options.ReportRequestRetryInterval = TimeSpan.Zero;

			_easyMwsClient.QueueReport(reportRequestContainer, ReportDownloadCallback, "callbackData");

		    var retryCount = _options.ReportRequestMaxRetryCount;

		    for (int i = 0; i <= retryCount; i++)
		    {
				_easyMwsClient.Poll();

			    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.EndsWith("Executing cleanup of report requests queue.")),
				    It.IsAny<RequestInfo>()));
			    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith("Attempting to request the next report in queue")),
				    It.IsAny<RequestInfo>()));
			    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.StartsWith($"Request to MWS.RequestReport was successful!")),
				    It.IsAny<RequestInfo>()));
			    _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains($"ReportRequestId not generated by Amazon. Placing report request in retry queue. Retry count : {i + 1}")),
				    It.IsAny<RequestInfo>()));

			    var dbEntry = _dbContext.ReportRequestEntries.Where(rre => rre.ReportType == validReportType);
			    Assert.NotNull(dbEntry);
			    Assert.AreEqual(1, dbEntry.Count());
			    var reportRequestEntry = dbEntry.Single();
			    Assert.IsNull(reportRequestEntry.RequestReportId);
			}

		    _easyMwsClient.Poll();

		    _loggerMock.Verify(l => l.Info(It.Is<string>(msg => msg.EndsWith("Executing cleanup of report requests queue.")),
			    It.IsAny<RequestInfo>()));
		    _loggerMock.Verify(l => l.Warn(It.Is<string>(msg => msg.Contains("deleted from queue. Reason: Failure while trying to request the report from Amazon. Retry count exceeded")),
			    It.IsAny<RequestInfo>()));

		    Assert.IsNull(_dbContext.ReportRequestEntries.FirstOrDefault(rre => rre.ReportType == validReportType));
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

		private void Setup_RequestReport_Returns_ReportRequestWasGenerated(string reportType)
		{
			var expectedReportRequestId = "test report request Id";
			var response = new RequestReportResponse
			{
				ResponseHeaderMetadata = new ResponseHeaderMetadata("requestId", "responseContext", "timestamp"),
				RequestReportResult = new RequestReportResult { ReportRequestInfo = new ReportRequestInfo { ReportRequestId = expectedReportRequestId } }
			};

			_mwsClientMock
				.Setup(mws => mws.RequestReport(It.Is<RequestReportRequest>(rrr => rrr.ReportType == reportType && rrr.Merchant == _merchantId)))
				.Returns(response);
		}

		private void Setup_GetReportRequestList_Returns_ReportsGeneratedSuccessfully(string expectedGeneratedReportId, string expectedReportProcessingStatus)
		{
			var reportRequestId = "test report request Id";
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

		private void Setup_GetReport_Returns_ReportContentStream(string expectedGeneratedReportId, string reportContent)
		{
			var stream = StreamHelper.CreateMemoryStream(reportContent);
			var validMd5Hash = MD5ChecksumHelper.ComputeHashForAmazon(stream);
			var response = new GetReportResponse
			{
				ResponseHeaderMetadata = new ResponseHeaderMetadata("requestId", "responseContext", "timestamp"),
				GetReportResult = new GetReportResult { ContentMD5 = validMd5Hash }
			};

			_mwsClientMock
				.Setup(mws => mws.GetReport(It.Is<GetReportRequest>(grr => grr.ReportId == expectedGeneratedReportId && grr.Merchant == _merchantId)))
				.Callback<GetReportRequest>(request => { request.Report = stream; })
				.Returns(response);
		}

		#endregion
	}
}
