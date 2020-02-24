namespace MountainWarehouse.EasyMWS.Enums
{
	/// <summary>
	/// The reason for which a request to download a report from amazon has failed, and for which reason the request is being deleted from the EasyMws internal queue.<br/>
	/// Consult the EasyMws logs for more details about the actual cause (to access the EasyMws logs, pass in an instance of IEasyMwsLogger when initializing the EasyMwsClient, subscribe to the IEasyMwsLogger.LogAvailable event, and in the eventHandler redirect the logs to an actual logger e.g. Log4net)
	/// </summary>
	public enum ReportRequestFailureReasonType
	{
		/// <summary>
		/// Calling the RequestReport MWS API endpoint has failed more than [EasyMwsOptions.ReportRequestOptions.ReportRequestMaxRetryCount] times. (Default 4 times)
		/// </summary>
		ReportRequestMaxRetryCountExceeded,

		/// <summary>
		/// Calling the GetReport MWS API endpoint has failed more than [EasyMwsOptions.ReportRequestOptions.ReportDownloadMaxRetryCount] times. (Default 4 times)
		/// </summary>
		ReportDownloadMaxRetryCountExceeded,

		/// <summary>
		/// Calling the GetReportList MWS API endpoint results in a report processing status being assigned to the current report which is NOT one of the following: Done, DoneNoData, Submitted, InProgress; and this has happened more than [EasyMwsOptions.ReportRequestOptions.ReportProcessingMaxRetryCount] times. (Default 2 times)
		/// </summary>
		ReportProcessingMaxRetryCountExceeded,

		/// <summary>
		/// Invoking the ReportDownloaded event has failed more than [EasyMwsOptions.EventPublishingOptions.EventPublishingMaxRetryCount] times. (Default 5 times)<br/>
		/// OR an exception was thrown within a ReportDownloaded event handler the same number of times.
		/// </summary>
		InvokeCallbackMaxRetryCountExceeded,

		/// <summary>
		/// The request to download a report has persisted in the EasyMws internal queue for more than the expiration period [EasyMwsOptions.ReportRequestOptions.ReportDownloadRequestEntryExpirationPeriod]. (Default 1 day)
		/// </summary>
		ReportRequestEntryExpirationPeriodExceeded
	}
}
