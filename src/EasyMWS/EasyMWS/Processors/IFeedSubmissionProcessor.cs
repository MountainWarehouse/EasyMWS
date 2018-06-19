using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedSubmissionProcessor
	{
		void SubmitFeedToAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmission);
		List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(
			IEnumerable<string> feedSubmissionIdList, string merchant);
		void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionEntryService feedSubmissionService,
			List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses);
		(MemoryStream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionEntry feedSubmissionEntry);
		void RemoveFromQueue(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry entry);
		void CleanUpFeedSubmissionQueue(IFeedSubmissionEntryService feedSubmissionService);
	}
}
