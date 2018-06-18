using System;

namespace MountainWarehouse.EasyMWS.Model
{
	/// <summary>
	/// A collection of settings that can be used to configure an EasyMwsClient instance.
	/// </summary>
    public class EasyMwsOptions
    {
		/// <summary>
		/// Default=3. When receiving a _CANCELLED_ or unhandled processing status from amazon for a report queued for request, specify how many times to retry requesting the report from amazon. 
		/// </summary>
		public int ReportProcessingMaxRetryCount { get; set; }

	    /// <summary>
		/// Default=5. When attempting to invoke a callback method after a report has been downloaded or after a feed has been submitted, and the callback invocation fails, specify how many times to retry the invocation.
		/// </summary>
		public int InvokeCallbackMaxRetryCount { get; set; }

		/// <summary>
		///  Default=ArithmeticProgression. When attempting to invoke a callback method after a report has been downloaded or after a feed has been submitted, and the callback invocation fails, specify the time series type for invocation retries.
		/// </summary>
		public RetryPeriodType InvokeCallbackRetryPeriodType { get; set; }

		/// <summary>
		/// Default=30minutes. When attempting to invoke a callback method after a report has been downloaded or after a feed has been submitted, and the callback invocation fails, specify the interval between retries. 
		/// </summary>
		public TimeSpan InvokeCallbackRetryInterval { get; set; }

	    /// <summary>
		/// Default=4. When attempting to download a report from amazon but the attempt fails, specify how many times to retry the download.
		/// </summary>
		public int ReportDownloadMaxRetryCount { get; set; }

		/// <summary>
		/// Default=GeometricProgression. When attempting to download a report from amazon but the attempt fails, specify the time series type for download retries.
		/// </summary>
		public RetryPeriodType ReportDownloadRetryType { get; set; }

		/// <summary>
		/// Default=30minutes.When attempting to download a report from amazon but the attempt fails, specify the initial delay awaited before the first download retry is performed.
		/// </summary>
		public TimeSpan ReportDownloadRetryInitialDelay { get; set; }

		/// <summary>
		///  Default=1hour.When attempting to download a report from amazon but the attempt fails, specify the time-step used to calculate how often download retries will be performed. 
		/// </summary>
		public TimeSpan ReportDownloadRetryInterval { get; set; }

	    /// <summary>
		/// Default=4. When requesting a report from amazon fails, specify how many times to retry the same request.
		/// </summary>
	    public int ReportRequestMaxRetryCount { get; set; }

	    /// <summary>
		/// Default=GeometricProgression. When requesting a report from amazon fails, specify the time series type for request retries.
		/// </summary>
		public RetryPeriodType ReportRequestRetryType { get; set; }

		/// <summary>
		/// Default=30minutes. When requesting a report from amazon fails, specify the initial delay awaited before the first request retry is performed.
		/// </summary>
		public TimeSpan ReportRequestRetryInitialDelay { get; set; }

		/// <summary>
		/// Default=1hour. When requesting a report from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
		/// </summary>
		public TimeSpan ReportRequestRetryInterval { get; set; }

		/// <summary>
		/// Default=4. When requesting a FeedSubmission from amazon fails, specify how many times to retry the same request.
		/// </summary>
		public int FeedSubmissionMaxRetryCount { get; set; }

		/// <summary>
		/// Default=GeometricProgression. When requesting a FeedSubmission from amazon fails, specify the time series type for request retries.
		/// </summary>
		public RetryPeriodType FeedSubmissionRetryType { get; set; }

		/// <summary>
		/// Default=30minutes. When requesting a FeedSubmission from amazon fails, specify the initial delay awaited before the first request retry is performed.
		/// </summary>
		public TimeSpan FeedSubmissionRetryInitialDelay { get; set; }

		/// <summary>
		/// Default=1hour. When requesting a FeedSubmission from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
		/// </summary>
		public TimeSpan FeedSubmissionRetryInterval { get; set; }

		/// <summary>
		/// Default=null. If a connection string is specified using this option, the EasyMws local db will run against this connection string, ignoring any connection string contained in a configuration file.<para/>
		/// ConnectionString default location : .Net Framework / .Net Core projects referencing EasyMws need to provide an app.config file containing a connection string with the "EasyMwsContext" key in the calling executable project.																																																																																																																																								
		/// </summary>
		public string LocalDbConnectionStringOverride { get; set; }

		/// <summary>
		/// Default=1day. Sets the expiration period of report download request entries. If a request entry is not completed before the expiration period, then it is automatically removed from queue.
		/// </summary>
	    public TimeSpan ReportDownloadRequestEntryExpirationPeriod { get; set; }

	    /// <summary>
	    /// Default=2days. Sets the expiration period of feed submission request entries. If a request entry is not completed before the expiration period, then it is automatically removed from queue.
	    /// </summary>
	    public TimeSpan FeedSubmissionRequestEntryExpirationPeriod { get; set; }


		/// <summary>
		/// The set of default settings that will be used if no custom settings are specified.<para/>
		/// InvokeCallbackMaxRetryCount = 5,<para/>
		/// InvokeCallbackRetryPeriodType = RetryPeriodType.ArithmeticProgression,<para/>
		/// InvokeCallbackRetryInterval = TimeSpan.FromMinutes(30),<para/>
		/// <para/>
		/// ReportRequestMaxRetryCount = 4,<para/>
		/// ReportRequestRetryType = RetryPeriodType.GeometricProgression,<para/>
		/// ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(30),<para/>
		/// ReportRequestRetryInterval = TimeSpan.FromHours(1),<para/>
		/// <para/>
		/// ReportDownloadMaxRetryCount = 4,<para/>
		/// ReportDownloadRetryType = RetryPeriodType.GeometricProgression,<para/>
		/// ReportDownloadRetryInitialDelay = TimeSpan.FromMinutes(30),<para/>
		/// ReportDownloadRetryInterval = TimeSpan.FromHours(1),<para/>
		/// <para/>
		/// FeedSubmissionMaxRetryCount = 3,<para/>
		/// FeedSubmissionRetryType = RetryPeriodType.GeometricProgression,<para/>
		/// FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(30),<para/>
		/// FeedSubmissionRetryInterval = TimeSpan.FromHours(1),<para/>
		/// <para/>
		/// ReportDownloadRequestEntryExpirationPeriod = TimeSpan.FromDays(1),<para/>
		/// FeedSubmissionRequestEntryExpirationPeriod = TimeSpan.FromDays(2)<para/>
		/// <para/>
		/// </summary>
		public static EasyMwsOptions Defaults()
	    {
		    return new EasyMwsOptions
		    {
			    InvokeCallbackMaxRetryCount = 5,
			    InvokeCallbackRetryPeriodType = RetryPeriodType.ArithmeticProgression,
			    InvokeCallbackRetryInterval = TimeSpan.FromMinutes(30),

				ReportRequestMaxRetryCount = 4,
				ReportRequestRetryType = RetryPeriodType.GeometricProgression,
			    ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(30),
			    ReportRequestRetryInterval = TimeSpan.FromHours(1),

			    ReportDownloadMaxRetryCount = 4,
			    ReportDownloadRetryType = RetryPeriodType.GeometricProgression,
			    ReportDownloadRetryInitialDelay = TimeSpan.FromMinutes(30),
			    ReportDownloadRetryInterval = TimeSpan.FromHours(1),

				ReportProcessingMaxRetryCount = 3,

				FeedSubmissionMaxRetryCount = 4,
			    FeedSubmissionRetryType = RetryPeriodType.GeometricProgression,
			    FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(30),
			    FeedSubmissionRetryInterval = TimeSpan.FromHours(1),

			    ReportDownloadRequestEntryExpirationPeriod = TimeSpan.FromDays(1),
				FeedSubmissionRequestEntryExpirationPeriod = TimeSpan.FromDays(2)
			};
		}
	}

	/// <summary>
	/// Specifies the time series type for the retry interval.
	/// </summary>
	public enum RetryPeriodType
	{
		/// <summary>
		/// T(k+1) = T(k) + RetryInterval. Example of retry time-steps with InitialDelay=3 and RetryInterval=2 : 3, 5, 7, 9, 11, ...
		/// </summary>
		ArithmeticProgression,

		/// <summary>
		/// T(k+1) = T(k) + [RetryInterval * (k-1)]. Example  of retry time-steps with InitialDelay=1 and RetryInterval=2 : 1, 3, 7, 13, ...
		/// </summary>
		GeometricProgression
	}

}
