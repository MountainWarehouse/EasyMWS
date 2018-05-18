using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
	internal class ReportRequestCallbackService : IReportRequestCallbackService
	{
		private readonly IReportRequestCallbackRepo _reportRequestCallbackRepo;
		private readonly IEasyMwsLogger _logger;

		internal ReportRequestCallbackService(IReportRequestCallbackRepo reportRequestCallbackRepo, EasyMwsOptions options = null, IEasyMwsLogger logger = null) : this(options, logger)
			=> (_reportRequestCallbackRepo) = (reportRequestCallbackRepo);

		internal ReportRequestCallbackService(EasyMwsOptions options = null, IEasyMwsLogger logger = null) => (_reportRequestCallbackRepo, _logger) =
			(_reportRequestCallbackRepo ?? new ReportRequestCallbackRepo(options?.LocalDbConnectionStringOverride), logger);

		public void Create(ReportRequestCallback callback) => _reportRequestCallbackRepo.Create(callback);
		public async Task CreateAsync(ReportRequestCallback callback) => await _reportRequestCallbackRepo.CreateAsync(callback);
		public void Update(ReportRequestCallback callback) => _reportRequestCallbackRepo.Update(callback);
		public void Delete(int id)
		{
			try
			{
				_reportRequestCallbackRepo.Delete(id);
			}
			catch (Exception e)
			{
				_logger.Error(!_reportRequestCallbackRepo.GetAll().Where(rr => rr.Id == id).Select(r => r.Id).Any()
						? $"Delete ReportRequestCallback entity with ID: {id} failed. It is likely the entity has already been deleted."
						: $"Delete ReportRequestCallback entity with ID: {id} failed. See exception info for more details", e);
			}

		}

		public void SaveChanges() => _reportRequestCallbackRepo.SaveChanges();
		public async Task SaveChangesAsync() => await _reportRequestCallbackRepo.SaveChangesAsync();
		public IQueryable<ReportRequestCallback> GetAll() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id);

		public IQueryable<ReportRequestCallback> Where(Expression<Func<ReportRequestCallback, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).Where(predicate);

		public ReportRequestCallback First() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).First();
		public ReportRequestCallback FirstOrDefault() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public ReportRequestCallback FirstOrDefault(Expression<Func<ReportRequestCallback, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);

		public ReportRequestCallback Last() => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).First();
		public ReportRequestCallback LastOrDefault() => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault();
		public ReportRequestCallback LastOrDefault(Expression<Func<ReportRequestCallback, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault(predicate);
	}
}
