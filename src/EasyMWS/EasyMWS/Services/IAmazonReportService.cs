using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Services
{
    internal interface IAmazonReportService
    {
	    void Create(AmazonReport report);
	    void Delete(AmazonReport report);
	    IQueryable<AmazonReport> GetAll();
	    void SaveChanges();
    }
}
