using System;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public static class RetryIntervalHelper
    {
		/// <summary>
		/// Calculates if the expected period of time has passed since the last retry.
		/// </summary>
		/// <param name="timeOfLastRetry"></param>
		/// <param name="currentRetryCount"></param>
		/// <param name="retryInitialDelay"></param>
		/// <param name="retryInterval"></param>
		/// <param name="retryType"></param>
		/// <returns></returns>
	    public static bool IsRetryPeriodAwaited(DateTime timeOfLastRetry, int currentRetryCount,
		    TimeSpan retryInitialDelay, TimeSpan retryInterval, RetryPeriodType retryType)
	    {
		    DateTime timeOfNextRetry = DateTime.MinValue;

		    if (currentRetryCount <= 0) return true;
		    if (currentRetryCount == 1)
		    {
			    timeOfNextRetry = timeOfLastRetry.Add(retryInitialDelay);
		    }
		    if (currentRetryCount > 1)
		    {
			    switch (retryType)
			    {
				    case RetryPeriodType.ArithmeticProgression:
				    {
					    timeOfNextRetry = timeOfLastRetry.Add(retryInterval);
					    break;
				    }

				    case RetryPeriodType.GeometricProgression:
				    {
					    timeOfNextRetry = timeOfLastRetry.Add(TimeSpan.FromTicks(retryInterval.Ticks * (currentRetryCount - 1)));
					    break;
				    }
				    default:
					    throw new ArgumentOutOfRangeException("Retry period type not supported!");
			    }
		    }

		    if (DateTime.Compare(timeOfNextRetry, DateTime.UtcNow) < 0) return true;

		    return false;
	    }
    }
}
