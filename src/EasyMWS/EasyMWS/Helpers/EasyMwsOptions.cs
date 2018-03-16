using System;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public class EasyMwsOptions
    {
	    public int ReportRequestMaxRetryCount { get; set; }
	    public RetryPeriodType ReportRequestRetryType { get; set; }
	    public TimeSpan ReportRequestRetryInitialDelay { get; set; }
	    public TimeSpan ReportRequestRetryInterval { get; set; }

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
