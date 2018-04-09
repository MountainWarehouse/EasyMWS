using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Client
{
    public interface IEasyMwsClientWithEvents
    {
	    event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;
	    event EventHandler<FeedSubmittedEventArgs> FeedSubmitted;
		void Poll();

	    void QueueReport(ReportRequestPropertiesContainer reportRequestContainer);

	    void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer);
	}
}
