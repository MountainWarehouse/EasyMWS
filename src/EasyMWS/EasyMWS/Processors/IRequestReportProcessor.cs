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
		string RequestReportFromAmazon(ReportRequestEntry reportRequestEntry);
		void MoveToQueueOfReportsToGenerate(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry, string reportRequestId);
		IEnumerable<string> GetAllPendingReportFromQueue(IReportRequestCallbackService reportRequestService);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		ReportRequestEntry GetNextFromQueueOfReportsToDownload(IReportRequestCallbackService reportRequestService);
		Stream DownloadGeneratedReportFromAmazon(ReportRequestEntry reportRequestEntry);
		void RemoveFromQueue(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequest);
		void MoveToRetryQueue(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry);
		void QueueReportsAccordingToProcessingStatus(IReportRequestCallbackService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void CleanupReportRequests(IReportRequestCallbackService reportRequestService);
	}
}
		