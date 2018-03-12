using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
	internal class ReportRequestCallbackService : IReportRequestCallbackService
	{
		private readonly IReportRequestCallbackRepo _reportRequestCallbackRepo;

		internal ReportRequestCallbackService(IReportRequestCallbackRepo reportRequestCallbackRepo = null)
		{
			_reportRequestCallbackRepo = reportRequestCallbackRepo ?? new ReportRequestCallbackRepo();
		}

		public void Create(ReportRequestCallback callback) => _reportRequestCallbackRepo.Create(callback);
		public async Task CreateAsync(ReportRequestCallback callback) => await _reportRequestCallbackRepo.CreateAsync(callback);
		public void Update(ReportRequestCallback callback) => _reportRequestCallbackRepo.Update(callback);
		public void Delete(ReportRequestCallback callback) => _reportRequestCallbackRepo.Delete(callback);
		public void SaveChanges() => _reportRequestCallbackRepo.SaveChanges();
		public async Task SaveChangesAsync() => await _reportRequestCallbackRepo.SaveChangesAsync();
		public IQueryable<ReportRequestCallback> GetAll() => _reportRequestCallbackRepo.GetAll();

		public IQueryable<ReportRequestCallback> Where(Expression<Func<ReportRequestCallback, bool>> predicate) => _reportRequestCallbackRepo.GetAll().Where(predicate);

		public ReportRequestCallback First() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).First();
		public ReportRequestCallback FirstOrDefault() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public ReportRequestCallback FirstOrDefault(Expression<Func<ReportRequestCallback, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);

		public ReportRequestCallback Last() => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).First();
		public ReportRequestCallback LastOrDefault() => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault();
		public ReportRequestCallback LastOrDefault(Expression<Func<ReportRequestCallback, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault(predicate);
	}
}
