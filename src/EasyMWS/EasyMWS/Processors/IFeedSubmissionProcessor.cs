using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedSubmissionProcessor
	{
		FeedSubmissionCallback GetNextFromQueueOfFeedsToSubmit(AmazonRegion region, string merchantId);
		string SubmitFeedToAmazon(FeedSubmissionCallback feedSubmission);
		void MoveToQueueOfSubmittedFeeds(FeedSubmissionCallback feedSubmission, string feedSubmissionId);
		IEnumerable<FeedSubmissionCallback> GetAllSubmittedFeedsFromQueue(AmazonRegion region, string merchantId);

		List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(
			IEnumerable<string> feedSubmissionIdList, string merchant);

		void QueueFeedsAccordingToProcessingStatus(
			List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses);

		FeedSubmissionCallback GetNextFromQueueOfProcessingCompleteFeeds(AmazonRegion region, string merchant);
		(Stream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionCallback feedSubmissionCallback);
		void RemoveFromQueue(FeedSubmissionCallback feedSubmissionCallback);
		void MoveToRetryQueue(FeedSubmissionCallback feedSubmission);
	}
}
