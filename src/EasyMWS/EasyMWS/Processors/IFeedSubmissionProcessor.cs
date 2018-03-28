﻿using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedSubmissionProcessor
	{
		FeedSubmissionCallback GetNextFromQueueOfFeedsToSubmit(AmazonRegion region, string merchantId);
		string SubmitFeedToAmazon(FeedSubmissionCallback feedSubmission, string merchantId);
		void MoveToQueueOfSubmittedFeeds(FeedSubmissionCallback feedSubmission, string feedSubmissionId);
		IEnumerable<FeedSubmissionCallback> GetAllSubmittedFeedsFromQueue(AmazonRegion region, string merchantId);

		List<(string FeedSubmissionId, string FeedProcessingStatus)> GetFeedSubmissionResults(
			IEnumerable<string> feedSubmissionIdList, string merchant);

		void MoveFeedsToQueuesAccordingToProcessingStatus(
			List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses);

		FeedSubmissionCallback GetNextFeedFromProcessingCompleteQueue(AmazonRegion region, string merchant);
		(Stream processingReport, string md5hash) QueryFeedProcessingReport(FeedSubmissionCallback feedSubmissionCallback, string merchant);
		void DequeueFeedSubmissionCallback(FeedSubmissionCallback feedSubmissionCallback);
		void MoveToRetryQueue(FeedSubmissionCallback feedSubmission);
	}
}
