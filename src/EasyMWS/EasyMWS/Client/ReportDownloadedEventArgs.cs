using System.Collections.Generic;
using System.IO;

namespace MountainWarehouse.EasyMWS.Client
{
    public class ReportDownloadedEventArgs
    {
        public MemoryStream ReportContent { get; set; } = null;
        public string ReportType { get; set; } = null;
        public string TargetHandlerId { get; set; } = null;
        public Dictionary<string, object> TargetHandlerArgs { get; set; } = new Dictionary<string, object>();
    }
}
