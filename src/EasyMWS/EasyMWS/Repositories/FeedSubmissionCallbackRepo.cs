using System;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class FeedSubmissionCallbackRepo : IFeedSubmissionCallbackRepo
	{
		private EasyMwsContext _dbContext;
		internal FeedSubmissionCallbackRepo(string connectionString = null) => (_dbContext) = (new EasyMwsContext(connectionString));
		public void Create(FeedSubmissionEntry entry) => _dbContext.FeedSubmissionEntries.Add(entry);
		public void Update(FeedSubmissionEntry entry) => _dbContext.Update(entry);
		public void Delete(int id)
		{
			// it might be expected for an entity to be already removed, if dealing with multiple similar clients instances e.g. using hangfire for creating tasks.
			// if this happens let the exception be thrown, as it will be caught and logged anyway
			_dbContext.FeedSubmissionEntries.Remove(new FeedSubmissionEntry { Id = id });
		}

		public IQueryable<FeedSubmissionEntry> GetAll() => _dbContext.FeedSubmissionEntries.AsQueryable();
		public void SaveChanges() => _dbContext.SaveChanges();
	}
}
