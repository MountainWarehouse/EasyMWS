using System;
using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
    interface IFeedQueueingProcessor
	{
		void PollFeeds(IFeedSubmissionEntryService feedSubmissionService);
        void QueueFeed(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionPropertiesContainer propertiesContainer, string targetEventId = null, Dictionary<string, object> targetEventArgs = null);

        void PurgeQueue(IFeedSubmissionEntryService feedSubmissionService);

        event EventHandler<FeedUploadedEventArgs> FeedUploadedInternal;
    }
}
