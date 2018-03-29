using System.Linq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
    internal class AmazonReportService : IAmazonReportService
    {
	    private readonly IAmazonReportRepo _amazonReportRepo;

	    internal AmazonReportService(IAmazonReportRepo reportRepo) : this() => (_amazonReportRepo) = (reportRepo);
	    internal AmazonReportService() => (_amazonReportRepo) = (_amazonReportRepo ?? new AmazonReportRepo());

	    public void Create(AmazonReport report) => _amazonReportRepo.Create(report);
		public void Delete(AmazonReport report) => _amazonReportRepo.Delete(report);
		public IQueryable<AmazonReport> GetAll() => _amazonReportRepo.GetAll();
		public void SaveChanges() => _amazonReportRepo.SaveChanges();
	}
}
