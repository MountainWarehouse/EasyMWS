using System;

namespace MountainWarehouse.EasyMWS.Model
{
	/// <summary>
	/// A collection of settings that can be used to configure an EasyMwsClient instance.
	/// </summary>
    public class EasyMwsOptions
    {
		/// <summary>
		/// Default=4. When requesting a report from amazon fails, specify how many times to retry the same request.
		/// </summary>
	    public int ReportRequestMaxRetryCount { get; set; }

		/// <summary>
		/// Default=3. After a report has been downloaded from amazon, the delegate provided for the ReportQueue method is invoked. If the invocation fails, this option specifies how many times the invocation will be retried.
		/// </summary>
	    public int ReportReadyCallbackInvocationMaxRetryCount { get; set; }

		/// <summary>
		/// Default=3. After a feed submission result has been received from amazon, the delegate provided for the ReportFeed method is invoked. If the invocation fails, this option specifies how many times the invocation will be retried.
		/// </summary>
	    public int FeedSubmissionResponseCallbackInvocationMaxRetryCount { get; set; }

	    /// <summary>
		/// Default=GeometricProgression. When requesting a report from amazon fails, specify the time series type for request retries.
		/// </summary>
		public RetryPeriodType ReportRequestRetryType { get; set; }

		/// <summary>
		/// Default=15minutes. When requesting a report from amazon fails, specify the initial delay awaited before the first request retry is performed.
		/// </summary>
		public TimeSpan ReportRequestRetryInitialDelay { get; set; }

		/// <summary>
		/// Default=15minutes. When requesting a report from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
		/// </summary>
		public TimeSpan ReportRequestRetryInterval { get; set; }

		/// <summary>
		/// Default=3. When requesting a FeedSubmission from amazon fails, specify how many times to retry the same request.
		/// </summary>
		public int FeedSubmissionMaxRetryCount { get; set; }

		/// <summary>
		/// Default=ArithmeticProgression. When requesting a FeedSubmission from amazon fails, specify the time series type for request retries.
		/// </summary>
		public RetryPeriodType FeedSubmissionRetryType { get; set; }

		/// <summary>
		/// Default=15minutes. When requesting a FeedSubmission from amazon fails, specify the initial delay awaited before the first request retry is performed.
		/// </summary>
		public TimeSpan FeedSubmissionRetryInitialDelay { get; set; }

		/// <summary>
		/// Default=1hour. When requesting a FeedSubmission from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
		/// </summary>
		public TimeSpan FeedSubmissionRetryInterval { get; set; }

		/// <summary>
		/// Default=15minutes. If the checksum verification fails for a feed submission report received from Amazon, specify the the time-step used to calculate how often the feed submission report request will be retried.<para/>
		/// Retry interval is constant (arithmetic progression)
		/// </summary>
		public TimeSpan FeedResultFailedChecksumRetryInterval { get; set; }

		/// <summary>
		/// Default=3. If the checksum verification fails for a feed submission report received from Amazon, specify how many times the feed submission report request will be retried. 
		/// </summary>
		public int FeedResultFailedChecksumMaxRetryCount { get; set; }

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
		/// <para/>
		/// ReportRequestMaxRetryCount = 4,<para/>
		/// ReportReadyCallbackInvocationMaxRetryCount = 3,<para/>
		/// FeedSubmissionResponseCallbackInvocationMaxRetryCount = 3,<para/>
		/// ReportRequestRetryType = GeometricProgression,<para/>
		/// ReportRequestRetryInitialDelay = Minutes(15),<para/>
		/// ReportRequestRetryInterval = Minutes(15),<para/>
		/// <para/>
		/// FeedSubmissionMaxRetryCount = 3,<para/>
		/// FeedSubmissionRetryType = ArithmeticProgression,<para/>
		/// FeedSubmissionRetryInitialDelay = Minutes(15),<para/>
		/// FeedSubmissionRetryInterval = Hours(1),<para/>
		/// <para/>
		/// FeedResultFailedChecksumRetryInterval = Minutes(15),<para/>
		/// FeedResultFailedChecksumMaxRetryCount = 3,<para/>
		/// <para/>
		/// ReportDownloadRequestEntryExpirationPeriod = Days(1),<para/>
		/// FeedSubmissionRequestEntryExpirationPeriod = Days(2),<para/>
		/// </summary>
		public static EasyMwsOptions Defaults()
	    {
		    return new EasyMwsOptions
		    {
			    ReportReadyCallbackInvocationMaxRetryCount = 3,
			    FeedSubmissionResponseCallbackInvocationMaxRetryCount = 3,

			    ReportRequestMaxRetryCount = 4,
				ReportRequestRetryType = RetryPeriodType.GeometricProgression,
			    ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(15),
			    ReportRequestRetryInterval = TimeSpan.FromMinutes(15),

			    FeedSubmissionMaxRetryCount = 3,
			    FeedSubmissionRetryType = RetryPeriodType.ArithmeticProgression,
			    FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(15),
			    FeedSubmissionRetryInterval = TimeSpan.FromHours(1),

			    FeedResultFailedChecksumRetryInterval = TimeSpan.FromMinutes(15),
			    FeedResultFailedChecksumMaxRetryCount = 3,

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
