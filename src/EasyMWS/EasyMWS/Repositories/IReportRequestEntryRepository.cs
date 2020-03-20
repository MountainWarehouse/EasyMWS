using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MountainWarehouse.EasyMWS.Data;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Repositories
{
	internal interface IReportRequestEntryRepository
    {
	    void Create(ReportRequestEntry entry);
	    void Update(ReportRequestEntry entry);
	    void Delete(ReportRequestEntry entry);
	    void DeleteRange(IEnumerable<ReportRequestEntry> entries);

		IQueryable<ReportRequestEntry> GetAll();
		void SaveChanges();
	    void Dispose();

    }
}
