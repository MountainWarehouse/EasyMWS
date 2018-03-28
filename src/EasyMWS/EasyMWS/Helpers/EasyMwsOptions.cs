using System;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public class EasyMwsOptions
    {
	    public int ReportRequestMaxRetryCount { get; set; }
	    public RetryPeriodType ReportRequestRetryType { get; set; }
	    public TimeSpan ReportRequestRetryInitialDelay { get; set; }
	    public TimeSpan ReportRequestRetryInterval { get; set; }

		/// <summary>
		/// The number of maximum retries done when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public int FeedSubmissionMaxRetryCount { get; set; }
		/// <summary>
		/// Retry period type calculation when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public RetryPeriodType FeedSubmissionRetryType { get; set; }
		/// <summary>
		/// Initial delay before the first retry done when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public TimeSpan FeedSubmissionRetryInitialDelay { get; set; }
		/// <summary>
		/// Time interval used in the calculation of subsequent retry periods when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public TimeSpan FeedSubmissionRetryInterval { get; set; }

		/// <summary>
		/// When requesting a feed processing result from amazon, if the checksum of the report fails the MD5 value sent by amazon. The request will be retried.
		/// </summary>
	    public TimeSpan FeedResultFailedChecksumRetryInterval { get; set; }
	    public int FeedResultFailedChecksumMaxRetryCount { get; set; }


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

	public enum RetryPeriodType
	{
		ArithmeticProgression,
		GeometricProgression
	}

}
