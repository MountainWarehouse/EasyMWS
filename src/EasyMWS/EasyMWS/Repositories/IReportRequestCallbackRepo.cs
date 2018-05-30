using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Repositories
{
    internal interface IReportRequestCallbackRepo
    {
	    void Create(ReportRequestEntry entry);
	    Task CreateAsync(ReportRequestEntry entry);
	    void Update(ReportRequestEntry entry);
	    void Delete(ReportRequestEntry entry);
	    IQueryable<ReportRequestEntry> GetAll();
		void SaveChanges();
	    Task SaveChangesAsync();
	    void Dispose();

    }
}
