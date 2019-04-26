using System;

namespace MountainWarehouse.EasyMWS.Model
{
	/// <summary>
	/// A collection of settings that can be used to configure an EasyMwsClient instance.
	/// </summary>
    public class EasyMwsOptions
    {
        internal const int WorstCaseScenarioRetryInitialDelay = 5;
        internal const int WorstCaseScenarioRetryInterval = 10;
        internal const RetryPeriodType ExponentialRetryPeriodIncrease = RetryPeriodType.GeometricProgression;
        internal const RetryPeriodType ConstantRetryPeriod = RetryPeriodType.ArithmeticProgression;
        internal const int AmazonServiceRequestRetryCount = 4;
        internal const int AmazonRequestRequeueLimit = 2;

        public CallbackInvocationOptions CallbackInvocationOptions { get; set; }
        public ReportRequestOptions ReportRequestOptions { get; set; }
        public FeedSubmissionOptions FeedSubmissionOptions { get; set; }

		/// <summary>
		/// Default=null. If a connection string is specified using this option, the EasyMws local db will run against this connection string, ignoring any connection string contained in a configuration file.<para/>
		/// ConnectionString default location : .Net Framework / .Net Core projects referencing EasyMws need to provide an app.config file containing a connection string with the "EasyMwsContext" key in the calling executable project.																																																																																																																																								
		/// </summary>
		public string LocalDbConnectionStringOverride { get; set; }

        /// <summary>
        /// Returns a new instance of EasyMwsOptions populated with the default values. The configuration is identical to that returned by public the Defaults() static method.<para/>
        /// LocalDbConnectionStringOverride = null
        /// </summary>
        public EasyMwsOptions(bool useDefaultValues = true)
        {
            CallbackInvocationOptions = new CallbackInvocationOptions(useDefaultValues);
            ReportRequestOptions = new ReportRequestOptions(useDefaultValues);
            FeedSubmissionOptions = new FeedSubmissionOptions(useDefaultValues);

            if (useDefaultValues)
            {
                LocalDbConnectionStringOverride = null;
            }
        }
    }

    /// <summary>
    /// InvokeCallbackMaxRetryCount = 5,<para/>
    /// InvokeCallbackRetryPeriodType = RetryPeriodType.ArithmeticProgression,<para/>
    /// InvokeCallbackRetryInterval = TimeSpan.FromMinutes(30),<para/>
    /// <para/>
    /// InvokeCallbackForReportStatusDoneNoData = false<para/>
    /// </summary>
    public class CallbackInvocationOptions
    {
        /// <summary>
        /// Default=5. When attempting to invoke a callback method after a report has been downloaded or after a feed has been submitted, and the callback invocation fails, specify how many times to retry the invocation.
        /// </summary>
        public int InvokeCallbackMaxRetryCount { get; set; }

        /// <summary>
        ///  Default=ArithmeticProgression. When attempting to invoke a callback method after a report has been downloaded or after a feed has been submitted, and the callback invocation fails, specify the time series type for invocation retries.
        /// </summary>
        public RetryPeriodType InvokeCallbackRetryPeriodType { get; set; }

        /// <summary>
        /// Default=2minutes. When attempting to invoke a callback method after a report has been downloaded or after a feed has been submitted, and the callback invocation fails, specify the interval between retries. 
        /// </summary>
        public TimeSpan InvokeCallbackRetryInterval { get; set; }

        /// <summary>
		/// Default=false. Invoke the callback method after a report has been downloaded or after a feed has been submitted, even if the report status received from Amazon is DoneNoData, with the report stream argument being null.
		/// </summary>
        public bool InvokeCallbackForReportStatusDoneNoData { get; set; }

        public RestrictInvocationToOriginatingInstance RestrictInvocationToOriginatingInstance { get; set; }

        public CallbackInvocationOptions(bool useDefaultValues = true)
        {
            RestrictInvocationToOriginatingInstance = new RestrictInvocationToOriginatingInstance(useDefaultValues);

            if (useDefaultValues)
            {
                InvokeCallbackMaxRetryCount = 5;
                InvokeCallbackRetryPeriodType = EasyMwsOptions.ConstantRetryPeriod;
                InvokeCallbackRetryInterval = TimeSpan.FromMinutes(2);

                InvokeCallbackForReportStatusDoneNoData = false;
            }
        }
    }

    /// <summary>
    /// Options regarding restricting callback invocations by originating clients only.
    /// </summary>
    public class RestrictInvocationToOriginatingInstance
    {
        public RestrictInvocationToOriginatingInstance(bool useDefaultValues = true)
        {
            ForceInvocationByOriginatingInstance = false;
            AllowInvocationByAnyInstanceIfInvocationFailedLimitReached = false;
            CustomInstanceId = null;
            InvocationFailedLimit = 2;
        }

        /// <summary>
        /// Default=null. A custom value identifying an EasyMwsClient instance. <para/>
        /// If set to true then only a client with a matching CustomInstanceId value will be able to invoke a callback for a request originating from a client with the same CustomInstanceId.<para/>
        /// If the Default value is set, then the instance id will automatically be set to System.Environment.MachineName.<para/>
        /// If the same CustomInstanceId value is specified for multiple clients sharing the same persistence location (e.g. database), then any of those clients will be able to invoke callbacks for requests submitted from clients with that CustomInstanceId.
        /// </summary>
        public string CustomInstanceId { get; set; }

        /// <summary>
        /// Default=false. After a report has been downloaded or after a feed has been submitted, try to invoke the callback method but restrict the source of the invocation to the instance from where the initial request originated.<para/>
        /// If AllowInvocationByAnyInstanceIfInvocationFailedLimitReached is false, then callbacks can always ONLY be invoked by the instance from where the request originated.<para/>
        /// </summary>
        public bool ForceInvocationByOriginatingInstance { get; set; }

        /// <summary>
        /// Default=false. After a report has been downloaded or after a feed has been submitted, if the callback invocation attempts fails for InvocationFailedLimit times, then allow the invocation to be made from any client instance.<para/>
        /// This is only being used if ForceInvocationByOriginatingInstance is true.
        /// </summary>
        public bool AllowInvocationByAnyInstanceIfInvocationFailedLimitReached { get; set; }

        /// <summary>
        /// Default=2. Specify the number of callback invocation failures allowed to happen before attempting to invoke a callback from Any instance.<para/>
        /// This is only being used if AllowInvocationByAnyInstanceIfInvocationFailedLimitReached is true.
        /// </summary>
        public int InvocationFailedLimit { get; set; }
    }

    /// <summary>
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
    /// ReportProcessingMaxRetryCount = 3<para/>
    /// ReportDownloadRequestEntryExpirationPeriod = TimeSpan.FromDays(1),<para/>
    /// </summary>
    public class ReportRequestOptions
    {
        /// <summary>
		/// Default=3. When receiving a _CANCELLED_ or unhandled processing status from amazon for a report queued for request, specify how many times to retry requesting the same report from amazon. 
		/// </summary>
		public int ReportProcessingMaxRetryCount { get; set; }


        /// <summary>
        /// Default=4. When attempting to download a report from amazon but the attempt fails, specify how many times to retry the download.
        /// </summary>
        public int ReportDownloadMaxRetryCount { get; set; }

        /// <summary>
        /// Default=GeometricProgression. When attempting to download a report from amazon but the attempt fails, specify the time series type for download retries.
        /// </summary>
        public RetryPeriodType ReportDownloadRetryType { get; set; }

        /// <summary>
        /// Default=5minutes.When attempting to download a report from amazon but the attempt fails, specify the initial delay awaited before the first download retry is performed.
        /// </summary>
        public TimeSpan ReportDownloadRetryInitialDelay { get; set; }

        /// <summary>
        ///  Default=10minutes.When attempting to download a report from amazon but the attempt fails, specify the time-step used to calculate how often download retries will be performed. 
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
        /// Default=5minutes. When requesting a report from amazon fails, specify the initial delay awaited before the first request retry is performed.
        /// </summary>
        public TimeSpan ReportRequestRetryInitialDelay { get; set; }

        /// <summary>
        /// Default=10minutes. When requesting a report from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
        /// </summary>
        public TimeSpan ReportRequestRetryInterval { get; set; }

        /// <summary>
		/// Default=1day. Sets the expiration period of report download request entries. If a request entry is not completed before the expiration period, then it is automatically removed from queue.
		/// </summary>
	    public TimeSpan ReportDownloadRequestEntryExpirationPeriod { get; set; }

        public ReportRequestOptions(bool useDefaultValues = true)
        {
            if (useDefaultValues)
            {
                ReportRequestMaxRetryCount = EasyMwsOptions.AmazonServiceRequestRetryCount;
                ReportRequestRetryType = EasyMwsOptions.ExponentialRetryPeriodIncrease;
                ReportRequestRetryInitialDelay = TimeSpan.FromMinutes(EasyMwsOptions.WorstCaseScenarioRetryInitialDelay);
                ReportRequestRetryInterval = TimeSpan.FromMinutes(EasyMwsOptions.WorstCaseScenarioRetryInterval);

                ReportDownloadMaxRetryCount = EasyMwsOptions.AmazonServiceRequestRetryCount;
                ReportDownloadRetryType = EasyMwsOptions.ExponentialRetryPeriodIncrease;
                ReportDownloadRetryInitialDelay = TimeSpan.FromMinutes(EasyMwsOptions.WorstCaseScenarioRetryInitialDelay);
                ReportDownloadRetryInterval = TimeSpan.FromMinutes(EasyMwsOptions.WorstCaseScenarioRetryInterval);

                ReportProcessingMaxRetryCount = EasyMwsOptions.AmazonRequestRequeueLimit;
                ReportDownloadRequestEntryExpirationPeriod = TimeSpan.FromDays(1);
            }
        }
    }

    /// <summary>
    /// FeedProcessingMaxRetryCount = 3<para/>
    /// <para/>
    /// FeedSubmissionMaxRetryCount = 3,<para/>
    /// FeedSubmissionRetryType = RetryPeriodType.GeometricProgression,<para/>
    /// FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(30),<para/>
    /// FeedSubmissionRetryInterval = TimeSpan.FromHours(1),<para/>
    /// FeedSubmissionRequestEntryExpirationPeriod = TimeSpan.FromDays(2),<para/>
    /// </summary>
    public class FeedSubmissionOptions
    {
        /// <summary>
        /// Default=3. When receiving a _CANCELLED_ or unhandled processing status from amazon for a feed submitted for processing, specify how many times to retry submitting the same feed.
        /// </summary>
        public int FeedProcessingMaxRetryCount { get; set; }


        /// <summary>
        /// Default=4. When requesting a FeedSubmission from amazon fails, specify how many times to retry the same request.
        /// </summary>
        public int FeedSubmissionMaxRetryCount { get; set; }

        /// <summary>
        /// Default=GeometricProgression. When requesting a FeedSubmission from amazon fails, specify the time series type for request retries.
        /// </summary>
        public RetryPeriodType FeedSubmissionRetryType { get; set; }

        /// <summary>
        /// Default=5minutes. When requesting a FeedSubmission from amazon fails, specify the initial delay awaited before the first request retry is performed.
        /// </summary>
        public TimeSpan FeedSubmissionRetryInitialDelay { get; set; }

        /// <summary>
        /// Default=10minutes. When requesting a FeedSubmission from amazon fails, specify the time-step used to calculate how often request retries will be performed. 
        /// </summary>
        public TimeSpan FeedSubmissionRetryInterval { get; set; }
        /// <summary>
        /// Default=2days. Sets the expiration period of feed submission request entries. If a request entry is not completed before the expiration period, then it is automatically removed from queue.
        /// </summary>
        public TimeSpan FeedSubmissionRequestEntryExpirationPeriod { get; set; }

        public FeedSubmissionOptions(bool useDefaultValues = true)
        {
            
            if (useDefaultValues)
            {
                FeedProcessingMaxRetryCount = EasyMwsOptions.AmazonRequestRequeueLimit;

                FeedSubmissionMaxRetryCount = EasyMwsOptions.AmazonServiceRequestRetryCount;
                FeedSubmissionRetryType = EasyMwsOptions.ExponentialRetryPeriodIncrease;
                FeedSubmissionRetryInitialDelay = TimeSpan.FromMinutes(EasyMwsOptions.WorstCaseScenarioRetryInitialDelay);
                FeedSubmissionRetryInterval = TimeSpan.FromMinutes(EasyMwsOptions.WorstCaseScenarioRetryInterval);


                FeedSubmissionRequestEntryExpirationPeriod = TimeSpan.FromDays(2);
            }
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
