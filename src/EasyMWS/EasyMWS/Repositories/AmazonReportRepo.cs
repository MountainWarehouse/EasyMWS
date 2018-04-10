using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class AmazonReportRepo : IAmazonReportRepo
    {
	    private EasyMwsContext _dbContext;

	    internal AmazonReportRepo(string connectionString = null) => (_dbContext) = (new EasyMwsContext(connectionString));

	    public void Create(AmazonReport report) => _dbContext.AmazonReports.Add(report);
		public void Delete(AmazonReport report) => _dbContext.AmazonReports.Remove(report);
	    public void DeleteRange(IEnumerable<AmazonReport> reports) => _dbContext.AmazonReports.RemoveRange(reports);
		public IQueryable<AmazonReport> GetAll() => _dbContext.AmazonReports.AsQueryable();
		public void SaveChanges() => _dbContext.SaveChanges();
	}
}
