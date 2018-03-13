using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Services
{
	internal interface IReportRequestCallbackService
	{
		void Create(ReportRequestCallback callback);
		Task CreateAsync(ReportRequestCallback callback);
		void Update(ReportRequestCallback callback);
		void Delete(ReportRequestCallback callback);
		void SaveChanges();
		Task SaveChangesAsync();
		IQueryable<ReportRequestCallback> GetAll();
		IQueryable<ReportRequestCallback> Where(Expression<Func<ReportRequestCallback, bool>> predicate);
		ReportRequestCallback First();
		ReportRequestCallback FirstOrDefault();
		ReportRequestCallback FirstOrDefault(Expression<Func<ReportRequestCallback, bool>> predicate);
		ReportRequestCallback Last();
		ReportRequestCallback LastOrDefault();
		ReportRequestCallback LastOrDefault(Expression<Func<ReportRequestCallback, bool>> predicate);
	}
}