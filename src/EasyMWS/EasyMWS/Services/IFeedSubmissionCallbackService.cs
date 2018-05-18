using System;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Services
{
    internal interface IFeedSubmissionCallbackService
    {
	    void Create(FeedSubmissionCallback callback);
	    void Update(FeedSubmissionCallback callback);
	    void Delete(int id);
	    void SaveChanges();
	    IQueryable<FeedSubmissionCallback> GetAll();
	    IQueryable<FeedSubmissionCallback> Where(Expression<Func<FeedSubmissionCallback, bool>> predicate);
	    FeedSubmissionCallback First();
	    FeedSubmissionCallback FirstOrDefault();
	    FeedSubmissionCallback FirstOrDefault(Expression<Func<FeedSubmissionCallback, bool>> predicate);
	    FeedSubmissionCallback Last();
	    FeedSubmissionCallback LastOrDefault();
	    FeedSubmissionCallback LastOrDefault(Expression<Func<FeedSubmissionCallback, bool>> predicate);
	}
}
