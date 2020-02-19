using System;
using System.Collections.Generic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;

namespace MountainWarehouse.EasyMWS.Processors
{
    internal interface IReportQueueingProcessor
    {
	    void PollReports(IReportRequestEntryService reportRequestService);
	    void QueueReport(IReportRequestEntryService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, string targetEventId = null, Dictionary<string, object> targetEventArgs = null);

        void PurgeQueue(IReportRequestEntryService reportRequestService);

        event EventHandler<ReportDownloadedEventArgs> ReportDownloadedInternal;
        event EventHandler<ReportRequestFailedEventArgs> ReportRequestFailedInternal;
    }
}
