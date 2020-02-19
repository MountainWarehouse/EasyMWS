using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;
using System;

namespace MountainWarehouse.EasyMWS.Client
{
    public class ReportRequestFailedEventArgs
    {
        public ReportRequestFailedEventArgs(ReportRequestFailureReasonType requestFailureReason, AmazonRegion amazonRegion, DateTime lastAmazonRequestTimestamp, string lastAmazonStatus, string reportRequestId, string generatedReportId, ReportRequestPropertiesContainer reportRequestPropertiesContainer, string targetHandlerId, string targetHandlerArgs)
        {
            RequestFailureReason = requestFailureReason;
            AmazonRegion = amazonRegion;
            LastAmazonRequestTimestamp = lastAmazonRequestTimestamp;
            LastAmazonStatus = lastAmazonStatus;
            ReportRequestId = reportRequestId;
            GeneratedReportId = generatedReportId;
            ReportRequestPropertiesContainer = reportRequestPropertiesContainer;
            TargetHandlerId = targetHandlerId;
            TargetHandlerArgs = targetHandlerArgs;
        }

        public ReportRequestFailureReasonType RequestFailureReason { get; }
        public AmazonRegion AmazonRegion { get; }
        public DateTime LastAmazonRequestTimestamp { get; }
        public string LastAmazonStatus { get; }
        public string ReportRequestId { get; }
        public string GeneratedReportId { get; }
        public ReportRequestPropertiesContainer ReportRequestPropertiesContainer { get; }
        public string TargetHandlerId { get; }
        public string TargetHandlerArgs { get; }
    }
}
