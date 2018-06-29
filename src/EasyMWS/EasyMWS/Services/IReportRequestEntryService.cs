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
		void Create(ReportRequestEntry entry);
		Task CreateAsync(ReportRequestEntry entry);
		void Update(ReportRequestEntry entry);
		void Delete(ReportRequestEntry entry);
		void DeleteRange(IEnumerable<ReportRequestEntry> entries);
		void SaveChanges();
		Task SaveChangesAsync();
		IQueryable<ReportRequestEntry> GetAll();
		IQueryable<ReportRequestEntry> Where(Expression<Func<ReportRequestEntry, bool>> predicate);
		ReportRequestEntry First();
		ReportRequestEntry FirstOrDefault();
		ReportRequestEntry FirstOrDefault(Expression<Func<ReportRequestEntry, bool>> predicate);
		ReportRequestEntry Last();
		ReportRequestEntry LastOrDefault();
		ReportRequestEntry LastOrDefault(Expression<Func<ReportRequestEntry, bool>> predicate);

		ReportRequestEntry GetNextFromQueueOfReportsToRequest(string merchantId, AmazonRegion region);
		ReportRequestEntry GetNextFromQueueOfReportsToDownload(string merchantId, AmazonRegion region);
		IEnumerable<string> GetAllPendingReportFromQueue(string merchantId, AmazonRegion region);
		IEnumerable<ReportRequestEntry> GetAllFromQueueOfReportsReadyForCallback(string merchantId, AmazonRegion region);
	}
}