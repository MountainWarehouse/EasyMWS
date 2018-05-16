using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class ReportRequestCallbackRepo : IReportRequestCallbackRepo
	{
	    private readonly EasyMwsContext _dbContext;

		internal ReportRequestCallbackRepo(string connectionString = null) => (_dbContext) = (new EasyMwsContext(connectionString));

	    public void Create(ReportRequestEntry entry) => _dbContext.ReportRequestEntries.Add(entry);
	    public async Task CreateAsync(ReportRequestEntry entry) => await _dbContext.ReportRequestEntries.AddAsync(entry);
		public void Update(ReportRequestEntry entry) => _dbContext.Update(entry);

		// it might be expected for an entity to be already removed, if dealing with multiple similar clients instances e.g. using hangfire for creating tasks. 
		// if this happens let the exception be thrown, as it will be caught and logged anyway 
		public void Delete(int id) => _dbContext.ReportRequestEntries.Remove(new ReportRequestEntry {Id = id});
	    public void SaveChanges() => _dbContext.SaveChanges();
	    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
		public IQueryable<ReportRequestEntry> GetAll() => _dbContext.ReportRequestEntries.AsQueryable();
	}
}
