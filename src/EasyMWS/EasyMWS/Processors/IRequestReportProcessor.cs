using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		void RequestReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		void QueueReportsAccordingToProcessingStatus(IReportRequestEntryService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void DownloadGeneratedReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry);
		IEnumerable<ReportRequestFailedEventArgs> CleanupReportRequests(IReportRequestEntryService reportRequestService);
	}
}
		