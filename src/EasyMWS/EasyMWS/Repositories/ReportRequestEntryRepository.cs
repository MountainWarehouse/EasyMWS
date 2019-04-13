using MountainWarehouse.EasyMWS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;

[assembly: InternalsVisibleTo("EasyMWS.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class ReportRequestEntryRepository : IReportRequestEntryRepository, IDisposable
    {
		private Dictionary<int, ReportRequestEntry> _data = new Dictionary<int, ReportRequestEntry>();
		private int _idCounter = 1;
		internal ReportRequestEntryRepository(string _connectionString)
		{
			//no-op
		}

		public void Create(ReportRequestEntry entry)
		{
			if (entry.Id == 0)
				entry.Id = _idCounter++;
			if (!_data.ContainsKey(entry.Id))
			{
				_data.Add(entry.Id, entry);
			}
		}

		public async Task CreateAsync(ReportRequestEntry entry) => Create(entry);

        public void Update(ReportRequestEntry entry)
        {
	        _data.Remove(entry.Id);
		    _data.Add(entry.Id, entry);
        }
        // it might be expected for an entity to be already removed, if dealing with multiple similar clients instances e.g. using hangfire for creating tasks. 
        // if this happens let the exception be thrown, as it will be caught and logged anyway 
        public void Delete(ReportRequestEntry entry) => _data.Remove(entry.Id);

        public void DeleteRange(IEnumerable<ReportRequestEntry> entries)
        {
	        foreach (var entry in entries)
	        {
				Delete(entry);
	        }
        }

        public void SaveChanges()
        {
			//no-op
        }
        public async Task SaveChangesAsync()
        {
			//no-op
        }

        public IQueryable<ReportRequestEntry> GetAll()
        {
	        return _data.Values.AsQueryable();
        }
        public void Dispose()
        {
			//no-op
        }
    }
}
