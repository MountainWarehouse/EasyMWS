using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestCallback GetNonRequestedReportFromQueue(AmazonRegion region);
		string RequestSingleQueuedReport(ReportRequestCallback reportRequestCallback, string merchantId);
		void MoveToNonGeneratedReportsQueue(ReportRequestCallback reportRequestCallback, string reportRequestId);
		Task MoveToNonGeneratedReportsQueueAsync(ReportRequestCallback reportRequestCallback, string reportRequestId);
		IEnumerable<ReportRequestCallback> GetAllPendingReport(AmazonRegion region);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportRequestListResponse(IEnumerable<string> requestIdList, string merchant);

		void MoveReportsToGeneratedQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> generatedReports);

		void MoveReportsBackToRequestQueue(List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> generatedReports);
		ReportRequestCallback GetReadyForDownloadReports(AmazonRegion region);
		void DownloadGeneratedReport(ReportRequestCallback reportRequestCallback, string merchantId);
	}
}