using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Services
{
	internal interface IReportRequestCallbackService
	{
		void Create(ReportRequestEntry entry);
		Task CreateAsync(ReportRequestEntry entry);
		void Update(ReportRequestEntry entry);
		void Delete(ReportRequestEntry entry);
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
	}
}