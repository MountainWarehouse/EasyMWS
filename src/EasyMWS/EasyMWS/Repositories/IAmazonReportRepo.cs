using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal interface IAmazonReportRepo
    {
	    void Create(AmazonReport report);
	    void Delete(AmazonReport report);
	    IQueryable<AmazonReport> GetAll();
	    void SaveChanges();
    }
}
