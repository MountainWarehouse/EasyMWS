using System.Runtime.InteropServices.ComTypes;
using MarketplaceWebService.Model;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Factories.Reports;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace EasyMWS.Tests.ReportProcessors
{
	[TestFixture]
    public class RequestReportProcessorTests
	{
		private EasyMwsClient _easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "AccessKeyTest", "SecretAccessKeyTest");
		private ReportRequestFactoryFba _reportRequestFactoryFba = new ReportRequestFactoryFba();

	    [Test]
	    public void RequestSingleQueueReport_OneInQueue_SerializesCorrectMerchantId()
	    {
		 //   var request = _reportRequestFactoryFba.GenerateRequestForReportGetAfnInventoryData();

			//var pendingReport = _easyMwsClient.QueueReport();
		 //   _mwsRepositoryFake.Add(pendingReport);
		 //   RequestReportRequest requestReportRequest = null;
		 //   _mwsClientMock.Setup(mc => mc.RequestReport(It.IsAny<RequestReportRequest>()))
			//    .Callback<RequestReportRequest>(rrr => requestReportRequest = rrr);

		 //   _reportManager.RequestSingleQueuedReport(AccountSettings.Europe);

		 //   Assert.IsNotNull(requestReportRequest);
		 //   Assert.AreEqual(AccountSettings.Europe.MerchantId, requestReportRequest.Merchant);
	    }

	}
}
