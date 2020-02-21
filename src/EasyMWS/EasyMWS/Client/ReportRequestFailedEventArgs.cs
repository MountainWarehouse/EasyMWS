using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;
using System;

namespace MountainWarehouse.EasyMWS.Client
{
    public class ReportRequestFailedEventArgs
    {
        public ReportRequestFailedEventArgs(ReportRequestFailureReasonType requestFailureReason, AmazonRegion amazonRegion, DateTime lastAmazonRequestTimestamp, string lastAmazonStatus, string reportRequestId, string generatedReportId, ReportRequestPropertiesContainer reportRequestPropertiesContainer, string targetHandlerId, string targetHandlerArgs, string reportType)
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
            ReportType = reportType;
        }

        /// <summary>
        /// The reason for which the report request has failed. For example the maximum retry limit might has been reached while trying to perform a certain step from the lifecycle of getting the report from Amazon.<br/>
        /// </summary>
        public ReportRequestFailureReasonType RequestFailureReason { get; }

        /// <summary>
        /// The Amazon region associated to the EasyMws client instance used to queue the report (this can be used to re-queue the report if necessary).<br/>
        /// </summary>
        public AmazonRegion AmazonRegion { get; }

        /// <summary>
        /// The timestamp associated to the last request of any kind to the Amazon MWS API, regarding this specific report download request entry.<para/>
        /// </summary>
        public DateTime LastAmazonRequestTimestamp { get; }

        /// <summary>
        /// The last report processing status received from Amazon for this specific report download request entry.<br/>
        /// </summary>
        public string LastAmazonStatus { get; }

        /// <summary>
        /// Parameter related to the report request from Amazon; This could be manually used in the Amazon scratchpad or an external tool to query the status of the report request.<br/>
        /// Amazon scratchpad url : https://mws.amazonservices.co.uk/scratchpad/index.html<br/>
        /// More information about the Amazon report request lifecycle : https://docs.developer.amazonservices.com/en_US/reports/Reports_Overview.html<br/>
        /// </summary>
        public string ReportRequestId { get; }

        /// <summary>
        /// Parameter related to the report request from Amazon; This could be manually used in the Amazon scratchpad or an external tool to query the status of the generated report.<br/>
        /// Amazon scratchpad url : https://mws.amazonservices.co.uk/scratchpad/index.html<br/>
        /// More information about the amazon report request lifecycle : https://docs.developer.amazonservices.com/en_US/reports/Reports_Overview.html<br/>
        /// </summary>
        public string GeneratedReportId { get; }

        /// <summary>
        /// A container of properties containing data necessary to queue a request to download a report from Amazon<br/>
        /// </summary>
        public ReportRequestPropertiesContainer ReportRequestPropertiesContainer { get; }

        /// <summary>
        /// Parameter that might have potentially been used to queue the report (this can be used to re-queue the report if necessary).
        /// </summary>
        public string TargetHandlerId { get; }

        /// <summary>
        /// Parameter that might have potentially been used to queue the report (this can be used to re-queue the report if necessary).
        /// </summary>
        public string TargetHandlerArgs { get; }

        /// <summary>
        /// The report type associated to the affected report download request entry<br/> https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html
        /// </summary>
        public string ReportType { get; }
    }
}
