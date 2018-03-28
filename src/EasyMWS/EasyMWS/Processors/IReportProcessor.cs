using System.IO;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IReportProcessor
	{
		void CleanUpReportRequestQueue();
		void RequestNextReportInQueueFromAmazon();
		void RequestReportStatusesFromAmazon();
		(ReportRequestCallback reportRequestCallback, Stream stream) DownloadNextReportInQueueFromAmazon();
		void ExecuteCallback(ReportRequestCallback reportRequestCallback, Stream stream);
	}
}
