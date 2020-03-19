using Moq;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Factories.Reports;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Processors;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace EasyMWS.Tests.EndToEnd
{
    public class SettlementReportDownloadingTests
    {
        private EasyMwsContext _dbContext;

        private IEasyMwsClient _easyMwsClient;
        private readonly AmazonRegion _region = AmazonRegion.Europe;
        private readonly string _merchantId = "testMerchantId";
        private readonly string _mwsAuthToken = "testMwsAuthToken";
        private EasyMwsOptions _options = new EasyMwsOptions();

        private Mock<IEasyMwsLogger> _loggerMock;
        private Mock<IMarketplaceWebServiceClient> _mwsClientMock;

		[SetUp]
		public void SetUp()
		{
            _options = new EasyMwsOptions();
			_dbContext = new EasyMwsContext();
			_loggerMock = new Mock<IEasyMwsLogger>();
			_mwsClientMock = new Mock<IMarketplaceWebServiceClient>();
			var feedProcessorMock = new Mock<IFeedQueueingProcessor>();

			var reportProcessor = new ReportProcessor(_region, _merchantId, _mwsAuthToken, _options, _mwsClientMock.Object, _loggerMock.Object);

			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, _merchantId, _mwsAuthToken, "test", "test", reportProcessor,
				feedProcessorMock.Object, _loggerMock.Object, _options);
        }

		[TearDown]
		public void TearDown()
		{
			var testReportEntries = _dbContext.ReportRequestEntries.Where(rre => rre.ReportType.StartsWith("_GET_V2_SETTLEMENT_REPORT_DATA_"));
			_dbContext.ReportRequestEntries.RemoveRange(testReportEntries);
			_dbContext.SaveChanges();

			_dbContext.Dispose();
		}

		private ReportRequestPropertiesContainer GenerateSettlementReportContainer()
			=> new SettlementReportsFactory().SettlementReport("testSettlementReportId");

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
				.Callback<GetReportRequest>(request => { stream.CopyTo(request.Report); })
				.Returns(response);
		}

        [Test]
        public void
            GivenQueuingSettlementReportAndPolling_WhenTargetHandlerArgsDataIsNotNull_TargetHandlerArgsIsSerializedAndDeserializedSuccessfully_SkipsFlowUntilGetReportAndDownloadedEventIsPublished()
        {
            // arrange
            var expectedReportProcessingStatus = "_DONE_";
            var reportRequestContainer = GenerateSettlementReportContainer();
            var reportType = reportRequestContainer.ReportType;
            var expectedGeneratedReportId = reportRequestContainer.ReportId;
            Setup_GetReport_Returns_ReportContentStream(expectedGeneratedReportId, "testReportContent");
            Stream actualReportContent = null;
            var targetEventArgs = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
            var reportDownloadedEventPublishedCount = 0;

            _easyMwsClient.ReportDownloaded += (s, e) =>
            {
                reportDownloadedEventPublishedCount++;
                actualReportContent = e.ReportContent;
                targetEventArgs = e.TargetHandlerArgs;
            };

            // act - queue report
            _easyMwsClient.QueueReport(reportRequestContainer, "targetHandlerId", new Dictionary<string, object> { { "key1", "value1" } });

            var dbEntry = _dbContext.ReportRequestEntries.Where(rre => rre.ReportType == reportType);

            // assert - null callback data serialization step does not crash
            Assert.NotNull(dbEntry);
            Assert.AreEqual(1, dbEntry.Count());
            var reportRequestEntry = dbEntry.Single();

            Assert.AreEqual("{\"key1\":\"value1\"}", reportRequestEntry.TargetHandlerArgs);

            // act - Poll report request process, in order for the queuedReport delegate to be invoked.
            _easyMwsClient.Poll();

            // assert - null callback data deserialization step does not crash, and callback was invoked successfully
            Assert.AreEqual(1, targetEventArgs.Count());
            Assert.IsNotNull(targetEventArgs.Where(kvp => kvp.Key == "key1"));
            Assert.IsNotNull(targetEventArgs.Where(kvp => kvp.Value == "value1"));
            Assert.NotNull(actualReportContent);
            Assert.AreEqual(1, reportDownloadedEventPublishedCount);

            // when queuing a settlement report, the queue entry is placed in a state so that all the flow for Requesting a Report from Amazon is skipped
            // up to the point of calling the GetReport() operation
            _mwsClientMock.Verify(mws => mws.RequestReport(It.IsAny<RequestReportRequest>()), Times.Never);
            _mwsClientMock.Verify(mws => mws.GetReportRequestList(It.IsAny<GetReportRequestListRequest>()), Times.Never);
            _mwsClientMock.Verify(mws => mws.GetReport(It.IsAny<GetReportRequest>()), Times.Once);
        }

    }
}
