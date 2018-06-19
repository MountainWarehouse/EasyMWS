using System;
using System.IO;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
    interface IFeedQueueingProcessor
	{
		void PollFeeds(IFeedSubmissionEntryService feedSubmissionService);
		void QueueFeed(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);
		void PurgeQueue(IFeedSubmissionEntryService feedSubmissionService);
	}
}
