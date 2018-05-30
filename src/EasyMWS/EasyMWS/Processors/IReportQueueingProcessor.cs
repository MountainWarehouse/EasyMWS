using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IReportQueueingProcessor
    {
	    void PollReports(IReportRequestCallbackService reportRequestService);
	    void QueueReport(IReportRequestCallbackService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);
	    void QueueReport(IReportRequestCallbackService reportRequestService, ReportRequestPropertiesContainer propertiesContainer);
	}
}
