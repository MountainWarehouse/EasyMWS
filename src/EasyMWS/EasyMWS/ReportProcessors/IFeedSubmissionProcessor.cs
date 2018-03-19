﻿using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
    internal interface IFeedSubmissionProcessor
    {
	    FeedSubmissionCallback GetNextFeedToSubmitFromQueue(AmazonRegion region, string merchantId);
	    string SubmitSingleQueuedFeedToAmazon(FeedSubmissionCallback feedSubmission, string merchantId);
	    void AllocateFeedSubmissionForRetry(FeedSubmissionCallback feedSubmission);
	    void MoveToQueueOfSubmittedFeeds(FeedSubmissionCallback feedSubmission, string feedSubmissionId);
    }
}
