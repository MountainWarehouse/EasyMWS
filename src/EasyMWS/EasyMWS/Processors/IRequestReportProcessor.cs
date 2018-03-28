﻿using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IRequestReportProcessor
	{
		ReportRequestCallback GetNextFromQueueOfReportsToRequest(AmazonRegion region, string merchantId);
		string RequestReportFromAmazon(ReportRequestCallback reportRequestCallback);
		void GetNextFromQueueOfReportsToGenerate(ReportRequestCallback reportRequestCallback, string reportRequestId);
		IEnumerable<ReportRequestCallback> GetAllPendingReportFromQueue(AmazonRegion region, string merchantId);
		List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant);
		ReportRequestCallback GetNextFromQueueOfReportsToDownload(AmazonRegion region, string merchantId);
		Stream DownloadGeneratedReportFromAmazon(ReportRequestCallback reportRequestCallback);
		void RemoveFromQueue(ReportRequestCallback reportRequestCallback);
		void MoveToRetryQueue(ReportRequestCallback reportRequestCallback);
		void QueueReportsAccordingToProcessingStatus(
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses);
	}
}
		