using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
    interface IFeedQueueingProcessor
	{
		void PollFeeds(IFeedSubmissionCallbackService feedSubmissionService);
		void QueueFeed(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);
		void QueueFeed(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer);
	}
}
