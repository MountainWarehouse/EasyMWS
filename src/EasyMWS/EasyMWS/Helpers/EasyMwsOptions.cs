using System;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public class EasyMwsOptions
    {
	    public int RequestRetryCount { get; set; }

	    public RetryPeriodType RetryPeriodType { get; set; }

	    public TimeSpan TimeToWaitBeforeFirstRetry { get; set; }
	    public TimeSpan TimeToWaitBetweenRetries { get; set; }

		public static EasyMwsOptions Defaults = new EasyMwsOptions
		{
			RequestRetryCount = 3,
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
