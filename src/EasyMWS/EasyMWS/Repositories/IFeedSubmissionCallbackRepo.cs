using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal interface IFeedSubmissionCallbackRepo
    {
	    void Create(FeedSubmissionEntry entry);
	    void Update(FeedSubmissionEntry entry);
	    void Delete(int id);
	    IQueryable<FeedSubmissionEntry> GetAll();
	    void SaveChanges();
	}
}
