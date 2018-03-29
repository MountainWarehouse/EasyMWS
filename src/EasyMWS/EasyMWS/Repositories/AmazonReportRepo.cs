using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class AmazonReportRepo : IAmazonReportRepo
    {
	    private EasyMwsContext _dbContext;

	    internal AmazonReportRepo()
	    {
		    _dbContext = new EasyMwsContext();
	    }

	    public void Create(AmazonReport report) => _dbContext.AmazonReports.Add(report);

		public void Delete(AmazonReport report) => _dbContext.AmazonReports.Remove(report);

	    public IQueryable<AmazonReport> GetAll() => _dbContext.AmazonReports.AsQueryable();

		public void SaveChanges() => _dbContext.SaveChanges();
	}
}
