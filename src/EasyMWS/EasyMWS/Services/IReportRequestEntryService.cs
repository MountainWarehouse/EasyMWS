using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Services
{
	internal interface IReportRequestEntryService
	{
		void Create(ReportRequestEntry entry);
		void Update(ReportRequestEntry entry);
		void Delete(ReportRequestEntry entry);
		void DeleteRange(IEnumerable<ReportRequestEntry> entries);
		void SaveChanges();
		IEnumerable<ReportRequestEntry> GetAll();
		IEnumerable<ReportRequestEntry> Where(Func<ReportRequestEntry, bool> predicate);
		ReportRequestEntry First();
		ReportRequestEntry FirstOrDefault();
		ReportRequestEntry FirstOrDefault(Func<ReportRequestEntry, bool> predicate);
		ReportRequestEntry GetNextFromQueueOfReportsToRequest(string merchantId, AmazonRegion region, bool markEntryAsLocked = true);
		ReportRequestEntry GetNextFromQueueOfReportsToDownload(string merchantId, AmazonRegion region, bool markEntryAsLocked = true);
		IEnumerable<string> GetAllPendingReportFromQueue(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true);
		IEnumerable<ReportRequestEntry> GetAllFromQueueOfReportsReadyForCallback(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true);
	}
}