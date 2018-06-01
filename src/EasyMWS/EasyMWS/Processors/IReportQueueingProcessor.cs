using System;
using System.IO;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IReportQueueingProcessor
    {
	    void PollReports(IReportRequestCallbackService reportRequestService);
	    void QueueReport(IReportRequestCallbackService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);

	    void PurgeQueue(IReportRequestCallbackService reportRequestService);
    }
}
