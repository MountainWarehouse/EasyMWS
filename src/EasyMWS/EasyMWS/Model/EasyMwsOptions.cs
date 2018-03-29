using System;

namespace MountainWarehouse.EasyMWS.Model
{
	/// <summary>
	/// A collection of settings that can be used to configure an EasyMwsClient instance.
	/// </summary>
    public class EasyMwsOptions
    {
		/// <summary>
		/// When requesting a report from amazon fails, specify how many times to retry the same request.
		/// </summary>
	    public int ReportRequestMaxRetryCount { get; set; }

		/// <summary>
		/// When requesting a report from amazon fails, specify the time series type for request retries.
		/// </summary>
		public RetryPeriodType ReportRequestRetryType { get; set; }

		/// <summary>
		/// When requesting a report from amazon fails, specify the initial delay awaited before the first request retry is performed.
		/// </summary>
		public TimeSpan ReportRequestRetryInitialDelay { get; set; }

		/// <summary>
		/// When requesting a report from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
		/// </summary>
		public TimeSpan ReportRequestRetryInterval { get; set; }

		/// <summary>
		/// When requesting a FeedSubmission from amazon fails, specify how many times to retry the same request.
		/// </summary>
		public int FeedSubmissionMaxRetryCount { get; set; }

		/// <summary>
		/// When requesting a FeedSubmission from amazon fails, specify the time series type for request retries.
		/// </summary>
		public RetryPeriodType FeedSubmissionRetryType { get; set; }

		/// <summary>
		/// When requesting a FeedSubmission from amazon fails, specify the initial delay awaited before the first request retry is performed.
		/// </summary>
		public TimeSpan FeedSubmissionRetryInitialDelay { get; set; }

		/// <summary>
		/// When requesting a FeedSubmission from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
		/// </summary>
		public TimeSpan FeedSubmissionRetryInterval { get; set; }

		/// <summary>
		/// If the checksum verification fails for a feed submission report received from Amazon, specify the the time-step used to calculate how often the feed submission report request will be retried. 
		/// </summary>
		public TimeSpan FeedResultFailedChecksumRetryInterval { get; set; }

		/// <summary>
		/// If the checksum verification fails for a feed submission report received from Amazon, specify how many times the feed submission report request will be retried. 
		/// </summary>
		public int FeedResultFailedChecksumMaxRetryCount { get; set; }

		/// <summary>
		/// The set of default settings that will be used if no custom settings are specified.
		/// </summary>
		public static EasyMwsOptions Defaults = new EasyMwsOptions
		{
			ReportRequestMaxRetryCount = 4,
			ReportRequestRetryType = RetryPeriodType.GeometricProgression,
			ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(15),
			ReportRequestRetryInterval = TimeSpan.FromHours(1),

			FeedSubmissionMaxRetryCount = 3,
			FeedSubmissionRetryType = RetryPeriodType.ArithmeticProgression,
			FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(2),
			FeedSubmissionRetryInterval = TimeSpan.FromHours(5),

			FeedResultFailedChecksumRetryInterval = TimeSpan.FromMinutes(2),
			FeedResultFailedChecksumMaxRetryCount = 3
		};
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
