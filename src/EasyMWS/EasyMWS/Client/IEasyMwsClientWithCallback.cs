using System;
using System.IO;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Client
{
    public interface IEasyMwsClientWithCallback
    {
	    void Poll();

	    void QueueReport(ReportRequestPropertiesContainer reportRequestContainer, Action<Stream, object> callbackMethod, object callbackData);

	    void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer, Action<Stream, object> callbackMethod, object callbackData);
    }
}
