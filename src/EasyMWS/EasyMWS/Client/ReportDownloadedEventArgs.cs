using System.Collections.ObjectModel;
using System.IO;

namespace MountainWarehouse.EasyMWS.Client
{
    public class ReportDownloadedEventArgs
    {
        public ReportDownloadedEventArgs(MemoryStream reportContent, string reportType, string targetHandlerId, ReadOnlyDictionary<string, object> targetHandlerArgs)
            => (ReportContent, ReportType, TargetHandlerId, TargetHandlerArgs)
            = (reportContent, reportType, targetHandlerId, targetHandlerArgs);

        public MemoryStream ReportContent { get; set; } = null;
        public string ReportType { get; set; } = null;
        public string TargetHandlerId { get; set; } = null;
        public ReadOnlyDictionary<string, object> TargetHandlerArgs { get; set; }
    }
}
