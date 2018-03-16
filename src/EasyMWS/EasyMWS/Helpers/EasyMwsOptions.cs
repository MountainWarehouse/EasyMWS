using System;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public class EasyMwsOptions
    {
	    public int MaxRequestRetryCount { get; set; }

	    public RetryPeriodType RetryPeriodType { get; set; }

	    public TimeSpan TimeToWaitBeforeFirstRetry { get; set; }
	    public TimeSpan TimeToWaitBetweenRetries { get; set; }

		public static EasyMwsOptions Defaults = new EasyMwsOptions
		{
			MaxRequestRetryCount = 4,
			RetryPeriodType = RetryPeriodType.GeometricProgression,
			TimeToWaitBeforeFirstRetry = TimeSpan.FromMinutes(15),
			TimeToWaitBetweenRetries = TimeSpan.FromHours(1)
		};
	}

	public enum RetryPeriodType
	{
		ArithmeticProgression,
		GeometricProgression
	}

}
