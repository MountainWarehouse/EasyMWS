using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedSubmissionProcessor
	{
		FeedSubmissionEntry GetNextFromQueueOfFeedsToSubmit(IFeedSubmissionCallbackService feedSubmissionService);
		string SubmitFeedToAmazon(FeedSubmissionEntry feedSubmission);
		void MoveToQueueOfSubmittedFeeds(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission, string feedSubmissionId);
		IEnumerable<string> GetIdsForSubmittedFeedsFromQueue(IFeedSubmissionCallbackService feedSubmissionService);

		List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(
			IEnumerable<string> feedSubmissionIdList, string merchant);

		void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionCallbackService feedSubmissionService,
			List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses);

		FeedSubmissionEntry GetNextFromQueueOfProcessingCompleteFeeds(IFeedSubmissionCallbackService feedSubmissionService);
		(MemoryStream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionEntry feedSubmissionEntry);
		void RemoveFromQueue(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry entry);
		void MoveToRetryQueue(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission);
		void CleanUpFeedSubmissionQueue(IFeedSubmissionCallbackService feedSubmissionService);
	}
}
