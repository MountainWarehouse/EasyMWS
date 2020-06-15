using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Services
{
	internal interface IReportRequestEntryService
	{
		void Lock(ReportRequestEntry entry);
		void Unlock(ReportRequestEntry entry);
		void Create(ReportRequestEntry entry);
		Task CreateAsync(ReportRequestEntry entry);
		void Update(ReportRequestEntry entry);
		void Delete(ReportRequestEntry entry);
		void DeleteRange(IEnumerable<ReportRequestEntry> entries);
		void SaveChanges();
		Task SaveChangesAsync();
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