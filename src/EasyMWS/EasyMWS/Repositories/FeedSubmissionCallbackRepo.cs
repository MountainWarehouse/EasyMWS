using System;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class FeedSubmissionCallbackRepo : IFeedSubmissionCallbackRepo
	{
		private EasyMwsContext _dbContext;
		internal FeedSubmissionCallbackRepo(string connectionString = null) => (_dbContext) = (new EasyMwsContext(connectionString));
		public void Create(FeedSubmissionCallback callback) => _dbContext.FeedSubmissionCallbacks.Add(callback);
		public void Update(FeedSubmissionCallback callback) => _dbContext.Update(callback);
		public void Delete(FeedSubmissionCallback callback) => _dbContext.Remove(callback);
		public IQueryable<FeedSubmissionCallback> GetAll() => _dbContext.FeedSubmissionCallbacks.AsQueryable();
		public void SaveChanges() => _dbContext.SaveChanges();
	}
}
