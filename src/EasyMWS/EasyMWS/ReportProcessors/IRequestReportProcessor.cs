using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestCallback GetNonRequestedReportFromQueue(AmazonRegion region, string merchantId);
		string RequestSingleQueuedReport(ReportRequestCallback reportRequestCallback, string merchantId);
		void MoveToNonGeneratedReportsQueue(ReportRequestCallback reportRequestCallback, string reportRequestId);
		IEnumerable<ReportRequestCallback> GetAllPendingReport(AmazonRegion region, string merchantId);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportRequestListResponse(IEnumerable<string> requestIdList, string merchant);
		void MoveReportsToGeneratedQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void MoveReportsBackToRequestQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		ReportRequestCallback GetReadyForDownloadReports(AmazonRegion region, string merchantId);
		Stream DownloadGeneratedReport(ReportRequestCallback reportRequestCallback, string merchantId);
		void DequeueReportRequestCallback(ReportRequestCallback reportRequestCallback);
		void AllocateReportRequestForRetry(ReportRequestCallback reportRequestCallback);
	}
}