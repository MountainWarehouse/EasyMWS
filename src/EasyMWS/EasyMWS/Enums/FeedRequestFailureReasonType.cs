namespace MountainWarehouse.EasyMWS.Enums
{
    /// <summary>
    /// The reason for which a request to upload a feed to amazon / or downloading its processing report has failed, and for which reason the request is being deleted from the EasyMws internal queue<br/>
    /// Consult the EasyMws logs for more details about the actual cause (to access the EasyMws logs, pass in an instance of IEasyMwsLogger when initializing the EasyMwsClient, subscribe to the IEasyMwsLogger.LogAvailable event, and in the eventHandler redirect the logs to an actual logger e.g. Log4net)
    /// </summary>
    public enum FeedRequestFailureReasonType
    {
        /// <summary>
        /// Calling the SubmitFeed MWS API endpoint has failed more than [EasyMwsOptions.FeedSubmissionOptions.FeedSubmissionMaxRetryCount] times. (Default 4 times)
        /// </summary>
        FeedSubmissionMaxRetryCountExceeded,

        /// <summary>
        /// Calling the GetFeedSubmissionResult MWS API endpoint has failed more than [EasyMwsOptions.ReportRequestOptions.ReportDownloadMaxRetryCount] times. (Default 4 times)
        /// </summary>
        ProcessingReportDownloadMaxRetryCountExceeded,

        /// <summary>
        /// Calling the GetFeedSubmissionList MWS API endpoint results in a feed processing status being assigned to the current feed which is NOT one of the following: Done, InProgress, InSafetyNet, Submitted, Unconfirmed; and this has happened more than [EasyMwsOptions.ReportRequestOptions.FeedProcessingMaxRetryCount] times. (Default 2 times)
        /// </summary>
        FeedProcessingMaxRetryCountExceeded,

        /// <summary>
        /// Invoking the FeedUploaded event has failed more than [EasyMwsOptions.EventPublishingOptions.EventPublishingMaxRetryCount] times. (Default 5 times)<br/>
		/// OR an exception was thrown within a FeedUploaded event handler the same number of times.
        /// </summary>
        InvokeCallbackMaxRetryCountExceeded,

        /// <summary>
        ///  The request to upload a feed has persisted in the EasyMws internal queue for more than the expiration period [EasyMwsOptions.FeedSubmissionOptions.FeedSubmissionRequestEntryExpirationPeriod]. (Default 2 days)
        /// </summary>
        FeedSubmissionEntryExpirationPeriodExceeded,
    }
}
