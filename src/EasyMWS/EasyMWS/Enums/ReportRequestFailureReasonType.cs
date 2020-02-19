namespace MountainWarehouse.EasyMWS.Enums
{
	public enum ReportRequestFailureReasonType
	{
		ReportRequestMaxRetryCountExceeded,
		ReportDownloadMaxRetryCountExceeded,
		ReportProcessingMaxRetryCountExceeded,
		InvokeCallbackMaxRetryCountExceeded,
		ReportRequestEntryExpirationPeriodExceeded
	}
}
