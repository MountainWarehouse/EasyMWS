using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MountainWarehouse.EasyMWS.Client
{
    public class FeedUploadedEventArgs
    {
        public FeedUploadedEventArgs(MemoryStream processingReportContent, string feedType, string targetHandlerId, ReadOnlyDictionary<string, object> targetHandlerArgs)
            => (ProcessingReportContent, FeedType, TargetHandlerId, TargetHandlerArgs)
            = (processingReportContent, feedType, targetHandlerId, targetHandlerArgs);

        public MemoryStream ProcessingReportContent { get; } = null;
        public string FeedType { get; } = null;
        public string TargetHandlerId { get; } = null;
        public ReadOnlyDictionary<string, object> TargetHandlerArgs { get; }
    }
}
