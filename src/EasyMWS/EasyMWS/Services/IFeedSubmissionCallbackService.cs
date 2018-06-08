using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Services
{
    internal interface IFeedSubmissionCallbackService
    {
	    void Create(FeedSubmissionEntry entry);
	    void Update(FeedSubmissionEntry entry);
	    void Delete(FeedSubmissionEntry entry);
	    void DeleteRange(IEnumerable<FeedSubmissionEntry> entries);
		void SaveChanges();
	    IQueryable<FeedSubmissionEntry> GetAll();
	    IQueryable<FeedSubmissionEntry> Where(Expression<Func<FeedSubmissionEntry, bool>> predicate);
	    FeedSubmissionEntry First();
	    FeedSubmissionEntry FirstOrDefault();
	    FeedSubmissionEntry FirstOrDefault(Expression<Func<FeedSubmissionEntry, bool>> predicate);
	    FeedSubmissionEntry Last();
	    FeedSubmissionEntry LastOrDefault();
	    FeedSubmissionEntry LastOrDefault(Expression<Func<FeedSubmissionEntry, bool>> predicate);
	}
}
