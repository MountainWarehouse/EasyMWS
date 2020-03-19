using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.DTO;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		void RequestReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		Task<IEnumerable<SettlementReportDetails>> ListSettlementReports(List<string> reportsToQuery, DateTime? availableFromDate = null, DateTime? availableToDate = null, bool? isAcknowledged = null);
		void QueueReportsAccordingToProcessingStatus(IReportRequestEntryService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
		void DownloadGeneratedReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry);
		void CleanupReportRequests(IReportRequestEntryService reportRequestService);


		event EventHandler<ReportRequestFailedEventArgs> ReportEntryWasMarkedForDelete;
	}
}
		