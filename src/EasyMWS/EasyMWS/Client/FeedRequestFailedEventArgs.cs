using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;
using System;

namespace MountainWarehouse.EasyMWS.Client
{
    public class FeedRequestFailedEventArgs
    {
        public FeedRequestFailedEventArgs(FeedRequestFailureReasonType requestFailureReason, AmazonRegion amazonRegion, DateTime lastAmazonRequestTimestamp, string lastAmazonStatus, string feedSubmissionId, FeedSubmissionPropertiesContainer feedSubmissionPropertiesContainer, string targetHandlerId, string targetHandlerArgs, string feedContent, string feedType)
        {
            RequestFailureReason = requestFailureReason;
            AmazonRegion = amazonRegion;
            LastAmazonRequestTimestamp = lastAmazonRequestTimestamp;
            LastAmazonStatus = lastAmazonStatus;
            FeedSubmissionId = feedSubmissionId;
            FeedSubmissionPropertiesContainer = feedSubmissionPropertiesContainer;
            TargetHandlerId = targetHandlerId;
            TargetHandlerArgs = targetHandlerArgs;
            FeedContent = feedContent;
            FeedType = feedType;
        }

        /// <summary>
        /// The reason for which the feed request has failed. For example the maximum retry limit might has been reached while trying to perform a certain step from the lifecycle of uploading a feed to Amazon or downloading its corresponding feed submission report from Amazon.<br/>
        /// </summary>
        public FeedRequestFailureReasonType RequestFailureReason { get; }

        /// <summary>
        /// The Amazon region associated to the EasyMws client instance used to queue the feed (this can be used to re-queue the feed if necessary).<br/>
        /// </summary>
        public AmazonRegion AmazonRegion { get; }

        /// <summary>
        /// The timestamp associated to the last request of any kind to the Amazon MWS API, regarding this specific feed upload request entry.<para/>
        /// </summary>
        public DateTime LastAmazonRequestTimestamp { get; }

        /// <summary>
        /// The last report processing status received from Amazon for this specific feed upload request entry.<br/>
        /// </summary>
        public string LastAmazonStatus { get; }

        /// <summary>
        /// Parameter related to the feed upload request to Amazon; This could be manually used in the Amazon scratchpad or an external tool to query the status of the feed upload/processing on Amazon's side.<br/>
        /// Amazon scratchpad url : https://mws.amazonservices.co.uk/scratchpad/index.html<br/>
        /// More information about the Amazon report request lifecycle : https://docs.developer.amazonservices.com/en_US/feeds/Feeds_Overview.html<br/>
        /// </summary>
        public string FeedSubmissionId { get; set; }

        /// <summary>
        /// A container of properties containing data necessary to queue a request to upload a feed to Amazon<br/>
        /// </summary>
        public FeedSubmissionPropertiesContainer FeedSubmissionPropertiesContainer { get; }

        /// <summary>
        /// Parameter that might have potentially been used to queue the feed (this can be used to re-queue the feed if necessary).
        /// </summary>
        public string TargetHandlerId { get; }

        /// <summary>
        /// Parameter that might have potentially been used to queue the feed (this can be used to re-queue the feed if necessary).
        /// </summary>
        public string TargetHandlerArgs { get; }

        /// <summary>
        /// The feed content associated to the affected feed upload request entry.
        /// </summary>
        public string FeedContent { get; }

        /// <summary>
        /// The feed type associated to the affected feed upload request entry<br/> https://docs.developer.amazonservices.com/en_US/feeds/Feeds_FeedType.html
        /// </summary>
        public string FeedType { get; }
    }
}
