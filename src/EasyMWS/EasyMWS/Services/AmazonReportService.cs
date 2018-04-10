using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
    internal class AmazonReportService : IAmazonReportService, IInternalStorageReportService
	{
	    private readonly IAmazonReportRepo _amazonReportRepo;

	    internal AmazonReportService(IAmazonReportRepo reportRepo, EasyMwsOptions options = null) : this(options) => (_amazonReportRepo) = (reportRepo);
	    internal AmazonReportService(EasyMwsOptions options = null) => (_amazonReportRepo) = (_amazonReportRepo ?? new AmazonReportRepo(options?.LocalDbConnectionStringOverride));

	    public void Create(AmazonReport report) => _amazonReportRepo.Create(report);
		public void Delete(AmazonReport report) => _amazonReportRepo.Delete(report);
		public IQueryable<AmazonReport> GetAll() => _amazonReportRepo.GetAll();
		public void SaveChanges() => _amazonReportRepo.SaveChanges();
		public IEnumerable<AmazonReport> GetAllReports() => GetAll();
		public IQueryable<AmazonReport> GetReportsWhere(Expression<Func<AmazonReport, bool>> predicate) => GetAll().Where(predicate);

		public IEnumerable<AmazonReport> GetAllReportsWithType(string reportType) => GetAll().Where(r => r.ReportType == reportType);

		public void DeleteAllReportsFromInternalStorage()
		{
			var reportsToDelete = GetAll();
			_amazonReportRepo.DeleteRange(reportsToDelete);
			_amazonReportRepo.SaveChanges();
		}
	}
}
