using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedSubmissionProcessor
	{
		FeedSubmissionCallback GetNextFromQueueOfFeedsToSubmit();
		string SubmitFeedToAmazon(FeedSubmissionCallback feedSubmission);
		void MoveToQueueOfSubmittedFeeds(FeedSubmissionCallback feedSubmission, string feedSubmissionId);
		IEnumerable<string> GetIdsForSubmittedFeedsFromQueue();

		List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(
			IEnumerable<string> feedSubmissionIdList, string merchant);

		void QueueFeedsAccordingToProcessingStatus(
			List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses);

		FeedSubmissionCallback GetNextFromQueueOfProcessingCompleteFeeds();
		(Stream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionCallback feedSubmissionCallback);
		void RemoveFromQueue(int feedSubmissionId);
		void MoveToRetryQueue(FeedSubmissionCallback feedSubmission);
		void CleanUpFeedSubmissionQueue();
	}
}
