using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestEntry GetNextFromQueueOfReportsToRequest(IReportRequestCallbackService reportRequestService);
		void RequestReportFromAmazon(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry);
		IEnumerable<string> GetAllPendingReportFromQueue(IReportRequestCallbackService reportRequestService);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		ReportRequestEntry GetNextFromQueueOfReportsToDownload(IReportRequestCallbackService reportRequestService);
		(MemoryStream report, string md5Hash) DownloadGeneratedReportFromAmazon(ReportRequestEntry reportRequestEntry);
		void MoveToRetryQueue(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry);
		void QueueReportsAccordingToProcessingStatus(IReportRequestCallbackService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void CleanupReportRequests(IReportRequestCallbackService reportRequestService);
	}
}
		