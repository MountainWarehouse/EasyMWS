using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestCallback GetNextFromQueueOfReportsToRequest();
		string RequestReportFromAmazon(ReportRequestCallback reportRequestCallback);
		void GetNextFromQueueOfReportsToGenerate(ReportRequestCallback reportRequestCallback, string reportRequestId);
		IEnumerable<ReportRequestCallback> GetAllPendingReportFromQueue();
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		ReportRequestCallback GetNextFromQueueOfReportsToDownload();
		Stream DownloadGeneratedReportFromAmazon(ReportRequestCallback reportRequestCallback);
		void RemoveFromQueue(int reportRequestCallbackId);
		void MoveToRetryQueue(ReportRequestCallback reportRequestCallback);
		void QueueReportsAccordingToProcessingStatus(
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void CleanupReportRequests();
	}
}
		