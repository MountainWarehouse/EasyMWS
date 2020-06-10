using System;
using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedSubmissionProcessor
	{
		void SubmitFeedToAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmission);
		List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(IFeedSubmissionEntryService feedSubmissionService,
			IEnumerable<string> feedSubmissionIdList, string merchant);
		void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionEntryService feedSubmissionService,
			List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses);
		void DownloadFeedSubmissionResultFromAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmissionEntry);
		void CleanUpFeedSubmissionQueue(IFeedSubmissionEntryService feedSubmissionService);

		event EventHandler<FeedRequestFailedEventArgs> FeedEntryWasMarkedForDelete;
	}
}
