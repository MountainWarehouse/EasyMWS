using System;
using System.IO;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IReportQueueingProcessor
    {
	    void PollReports(IReportRequestEntryService reportRequestService);
	    void QueueReport(IReportRequestEntryService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);
	    void PurgeQueue(IReportRequestEntryService reportRequestService);
    }
}
