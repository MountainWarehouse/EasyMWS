using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Services
{
    internal interface IFeedSubmissionEntryService
    {
	    void Create(FeedSubmissionEntry entry);
	    void Update(FeedSubmissionEntry entry);
	    void Delete(FeedSubmissionEntry entry);
	    void DeleteRange(IEnumerable<FeedSubmissionEntry> entries);
		void SaveChanges();
		IEnumerable<FeedSubmissionEntry> GetAll();
		IEnumerable<FeedSubmissionEntry> Where(Func<FeedSubmissionEntry, bool> predicate);
	    FeedSubmissionEntry First();
	    FeedSubmissionEntry FirstOrDefault();
	    FeedSubmissionEntry FirstOrDefault(Func<FeedSubmissionEntry, bool> predicate);
	    FeedSubmissionEntry GetNextFromQueueOfFeedsToSubmit(string merchantId, AmazonRegion region, bool markEntryAsLocked = true);

	    IEnumerable<string> GetIdsForSubmittedFeedsFromQueue(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true);

	    FeedSubmissionEntry GetNextFromQueueOfProcessingCompleteFeeds(string merchantId, AmazonRegion region, bool markEntryAsLocked = true);
	    IEnumerable<FeedSubmissionEntry> GetAllFromQueueOfFeedsReadyForCallback(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true);
	}
}
