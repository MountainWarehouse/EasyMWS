using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestCallback GetNonRequestedReportFromQueue(AmazonRegion region, string merchantId);
		string RequestSingleQueuedReport(ReportRequestCallback reportRequestCallback, string merchantId);
		void MoveToNonGeneratedReportsQueue(ReportRequestCallback reportRequestCallback, string reportRequestId);
		IEnumerable<ReportRequestCallback> GetAllPendingReportFromQueue(AmazonRegion region, string merchantId);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportRequestListResponse(IEnumerable<string> requestIdList, string merchant);
		ReportRequestCallback GetReadyForDownloadReportsFromQueue(AmazonRegion region, string merchantId);
		Stream DownloadGeneratedReportFromAmazon(ReportRequestCallback reportRequestCallback, string merchantId);
		void DequeueReportRequestCallback(ReportRequestCallback reportRequestCallback);
		void MoveToRetryQueue(ReportRequestCallback reportRequestCallback);
		void MoveReportsToQueuesAccordingToProcessingStatus(
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
	}
}