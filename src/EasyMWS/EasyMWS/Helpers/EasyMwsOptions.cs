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
		public int FeedInitialSubmissionMaxRetryCount { get; set; }
		/// <summary>
		/// Retry period type calculation when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public RetryPeriodType FeedInitialSubmissionRetryType { get; set; }
		/// <summary>
		/// Initial delay before the first retry done when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public TimeSpan FeedInitialSubmissionRetryInitialDelay { get; set; }
		/// <summary>
		/// Time interval used in the calculation of subsequent retry periods when trying to call Amazon SubmitFeed endpoint but a FeedSubmissionId is not generated.
		/// </summary>
		public TimeSpan FeedInitialSubmissionRetryInterval { get; set; }

		public int FeedSubmissionMaxRetryCount { get; set; }
	    public RetryPeriodType FeedSubmissionRetryType { get; set; }
	    public TimeSpan FeedSubmissionRetryInitialDelay { get; set; }
	    public TimeSpan FeedSubmissionRetryInterval { get; set; }

		public static EasyMwsOptions Defaults = new EasyMwsOptions
		{
			ReportRequestMaxRetryCount = 4,
			ReportRequestRetryType = RetryPeriodType.GeometricProgression,
			ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(15),
			ReportRequestRetryInterval = TimeSpan.FromHours(1),

			FeedInitialSubmissionMaxRetryCount = 3,
			FeedInitialSubmissionRetryType = RetryPeriodType.ArithmeticProgression,
			FeedInitialSubmissionRetryInitialDelay = TimeSpan.FromMinutes(1),
			FeedInitialSubmissionRetryInterval = TimeSpan.FromMinutes(5),

			FeedSubmissionMaxRetryCount = 4,
			FeedSubmissionRetryType = RetryPeriodType.GeometricProgression,
			FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(15),
			FeedSubmissionRetryInterval = TimeSpan.FromHours(1)
		};
	}

	public enum RetryPeriodType
	{
		ArithmeticProgression,
		GeometricProgression
	}

}
