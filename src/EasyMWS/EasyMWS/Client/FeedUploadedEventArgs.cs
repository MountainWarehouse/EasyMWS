using System.Collections.Generic;
using System.IO;

namespace MountainWarehouse.EasyMWS.Client
{
    public class FeedUploadedEventArgs
    {
        public MemoryStream ProcessingReportContent { get; set; } = null;
        public string FeedType { get; set; } = null;
        public string TargetHandlerId { get; set; } = null;
        public Dictionary<string, object> TargetHandlerArgs { get; set; } = new Dictionary<string, object>();
    }
}
