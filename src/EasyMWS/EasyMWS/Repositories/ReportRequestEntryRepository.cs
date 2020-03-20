using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MountainWarehouse.EasyMWS.Data;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Repositories
{
	internal class ReportRequestEntryRepository : IReportRequestEntryRepository, IDisposable
	{
	    private readonly EasyMwsContext _dbContext;

		internal ReportRequestEntryRepository(string connectionString = null) => (_dbContext) = (new EasyMwsContext(connectionString));

	    public void Create(ReportRequestEntry entry) => _dbContext.ReportRequestEntries.Add(entry);
		public void Update(ReportRequestEntry entry) => _dbContext.Update(entry);

		// it might be expected for an entity to be already removed, if dealing with multiple similar clients instances e.g. using hangfire for creating tasks. 
		// if this happens let the exception be thrown, as it will be caught and logged anyway 
		public void Delete(ReportRequestEntry entry) => _dbContext.ReportRequestEntries.Remove(entry);
		public void DeleteRange(IEnumerable<ReportRequestEntry> entries) => _dbContext.ReportRequestEntries.RemoveRange(entries);
		public void SaveChanges() => _dbContext.SaveChanges();
		public IQueryable<ReportRequestEntry> GetAll() => _dbContext.ReportRequestEntries.AsQueryable();
		public void Dispose()
		{
			_dbContext.Dispose();
		}
	}
}
