using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestEntry GetNextFromQueueOfReportsToRequest();
		string RequestReportFromAmazon(ReportRequestEntry reportRequestEntry);
		void MoveToQueueOfReportsToGenerate(ReportRequestEntry reportRequestEntry, string reportRequestId);
		IEnumerable<string> GetAllPendingReportFromQueue();
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		ReportRequestEntry GetNextFromQueueOfReportsToDownload();
		Stream DownloadGeneratedReportFromAmazon(ReportRequestEntry reportRequestEntry);
		void RemoveFromQueue(int reportRequestCallbackId);
		void MoveToRetryQueue(ReportRequestEntry reportRequestEntry);
		void QueueReportsAccordingToProcessingStatus(
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void CleanupReportRequests();
	}
}
		