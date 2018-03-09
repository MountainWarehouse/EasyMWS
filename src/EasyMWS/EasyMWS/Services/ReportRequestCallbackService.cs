using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Services
{
    internal class ReportRequestCallbackService : IReportRequestCallbackService
	{
	    private EasyMwsContext _dbContext;

	    internal ReportRequestCallbackService()
	    {
			_dbContext = new EasyMwsContext();
	    }

	    public void Create(ReportRequestCallback callback) => _dbContext.ReportRequestCallbacks.Add(callback);
	    public async Task CreateAsync(ReportRequestCallback callback) => await _dbContext.ReportRequestCallbacks.AddAsync(callback);
		public void Update(ReportRequestCallback callback) => _dbContext.Update(callback);
		public void Delete(ReportRequestCallback callback) => _dbContext.Remove(callback);
	    public void SaveChanges() => _dbContext.SaveChanges();
	    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
		public IQueryable<ReportRequestCallback> GetAll() => _dbContext.ReportRequestCallbacks.AsQueryable();
	}
}
