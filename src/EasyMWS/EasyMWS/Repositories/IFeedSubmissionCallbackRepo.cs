using System.Linq;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Repositories
{
    internal interface IFeedSubmissionCallbackRepo
    {
	    void Create(FeedSubmissionCallback callback);
	    void Update(FeedSubmissionCallback callback);
	    void Delete(int id);
	    IQueryable<FeedSubmissionCallback> GetAll();
	    void SaveChanges();
	}
}
