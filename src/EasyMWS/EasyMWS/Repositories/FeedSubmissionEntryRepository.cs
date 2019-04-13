using MountainWarehouse.EasyMWS.Data;
using System.Collections.Generic;
using System.Linq;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal class FeedSubmissionEntryRepository : IFeedSubmissionEntryRepository
	{
		private Dictionary<int, FeedSubmissionEntry> _feeds = new Dictionary<int, FeedSubmissionEntry>();
		private int _idCounter = 1;
		internal FeedSubmissionEntryRepository(string connectionString = null)
		{
			//no-op, leaving for posterity
		}

		public void Create(FeedSubmissionEntry entry)
		{
			if (entry.Id == 0)
				entry.Id = _idCounter++;
			_feeds.Add(entry.Id, entry);
		}

		public void Update(FeedSubmissionEntry entry)
		{
			_feeds.Remove(entry.Id);
			_feeds.Add(entry.Id, entry);
		}

		public void Delete(FeedSubmissionEntry entry)
		{
			_feeds.Remove(entry.Id);
		}

		public void DeleteRange(IEnumerable<FeedSubmissionEntry> entries)
		{
			foreach (var entry in entries)
			{
				Delete(entry);
			}
		}

		public IQueryable<FeedSubmissionEntry> GetAll() => _feeds.Values.AsQueryable();

		public void SaveChanges()
		{
			//no-op
		}

		public void Dispose()
		{
			//no-op
		}
	}
}