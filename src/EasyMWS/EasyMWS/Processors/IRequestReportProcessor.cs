using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		void RequestReportFromAmazon(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		void QueueReportsAccordingToProcessingStatus(IReportRequestCallbackService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void DownloadGeneratedReportFromAmazon(IReportRequestCallbackService reportRequestService, ReportRequestEntry reportRequestEntry);
		void CleanupReportRequests(IReportRequestCallbackService reportRequestService);
	}
}
		