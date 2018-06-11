using System.Collections.Generic;
using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal interface IFeedSubmissionCallbackRepo
    {
	    void Create(FeedSubmissionEntry entry);
	    void Update(FeedSubmissionEntry entry);
	    void Delete(FeedSubmissionEntry entry);
	    void DeleteRange(IEnumerable<FeedSubmissionEntry> entries);
		IQueryable<FeedSubmissionEntry> GetAll();
	    void SaveChanges();
	    void Dispose();
	}
}
