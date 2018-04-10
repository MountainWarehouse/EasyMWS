using System;
using MountainWarehouse.EasyMWS.Client;

namespace MountainWarehouse.EasyMWS.Processors
{
    internal interface IReportProcessor
    {
	    event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;
	}
}
