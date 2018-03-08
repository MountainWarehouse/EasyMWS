using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Services
{
    internal interface IReportRequestCallbackService
    {
	    void Create(ReportRequestCallback callback);
	    Task CreateAsync(ReportRequestCallback callback);
	    void Update(ReportRequestCallback callback);
	    void Delete(ReportRequestCallback callback);
	    void SaveChanges();
	    Task SaveChangesAsync();

    }
}
