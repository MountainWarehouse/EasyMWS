using System;
using System.IO;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
    interface IFeedQueueingProcessor
	{
		void PollFeeds(IFeedSubmissionCallbackService feedSubmissionService);
		void QueueFeed(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);
		void PurgeQueue(IFeedSubmissionCallbackService feedSubmissionService);
	}
}
