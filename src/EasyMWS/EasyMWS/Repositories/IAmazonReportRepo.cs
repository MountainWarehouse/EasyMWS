using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal interface IAmazonReportRepo
    {
	    void Create(AmazonReport report);
	    void Delete(AmazonReport report);
	    void DeleteRange(IEnumerable<AmazonReport> reports);
	    IQueryable<AmazonReport> GetAll();
	    void SaveChanges();
    }
}
