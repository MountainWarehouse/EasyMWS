using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		void RequestReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IReportRequestEntryService reportRequestService, IEnumerable<string> requestIdList, string merchant);
		void QueueReportsAccordingToProcessingStatus(IReportRequestEntryService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void DownloadGeneratedReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry);
		void CleanupReportRequests(IReportRequestEntryService reportRequestService);


		event EventHandler<ReportRequestFailedEventArgs> ReportEntryWasMarkedForDelete;
		void UnlockReportRequestEntries(IReportRequestEntryService reportRequestService, IEnumerable<string> reportRequestIds);
	}
}
		