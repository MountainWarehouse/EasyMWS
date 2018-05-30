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
	internal class ReportRequestCallbackService : IReportRequestCallbackService, IDisposable
	{
		private readonly IReportRequestCallbackRepo _reportRequestCallbackRepo;
		private readonly IEasyMwsLogger _logger;

		internal ReportRequestCallbackService(IReportRequestCallbackRepo reportRequestCallbackRepo, EasyMwsOptions options = null, IEasyMwsLogger logger = null) : this(options, logger)
			=> (_reportRequestCallbackRepo) = (reportRequestCallbackRepo);

		internal ReportRequestCallbackService(EasyMwsOptions options = null, IEasyMwsLogger logger = null) => (_reportRequestCallbackRepo, _logger) =
			(_reportRequestCallbackRepo ?? new ReportRequestCallbackRepo(options?.LocalDbConnectionStringOverride), logger);

		public void Create(ReportRequestEntry entry) => _reportRequestCallbackRepo.Create(entry);
		public async Task CreateAsync(ReportRequestEntry entry) => await _reportRequestCallbackRepo.CreateAsync(entry);
		public void Update(ReportRequestEntry entry) => _reportRequestCallbackRepo.Update(entry);
		public void Delete(ReportRequestEntry entry)
		{
			try
			{
				_reportRequestCallbackRepo.Delete(entry);
			}
			catch (Exception e)
			{
				_logger.Error(!_reportRequestCallbackRepo.GetAll().Where(rr => rr.Id == entry.Id).Select(r => r.Id).Any()
						? $"Delete ReportRequestCallback entity with ID: {entry.Id} failed. It is likely the entity has already been deleted."
						: $"Delete ReportRequestCallback entity with ID: {entry.Id} failed. See exception info for more details", e);
			}

		}

		public void SaveChanges() => _reportRequestCallbackRepo.SaveChanges();
		public async Task SaveChangesAsync() => await _reportRequestCallbackRepo.SaveChangesAsync();
		public IQueryable<ReportRequestEntry> GetAll() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id);

		public IQueryable<ReportRequestEntry> Where(Expression<Func<ReportRequestEntry, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).Where(predicate);

		public ReportRequestEntry First() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).First();
		public ReportRequestEntry FirstOrDefault() => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public ReportRequestEntry FirstOrDefault(Expression<Func<ReportRequestEntry, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);

		public ReportRequestEntry Last() => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).First();
		public ReportRequestEntry LastOrDefault() => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault();
		public ReportRequestEntry LastOrDefault(Expression<Func<ReportRequestEntry, bool>> predicate) => _reportRequestCallbackRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault(predicate);
		public void Dispose()
		{
			_reportRequestCallbackRepo.Dispose();
		}
	}
}
