# EasyMWS

## Description

EasyMWS is a .NET library that intends to simplify the interaction with the Amazon Marketplace Web Services API.
It tries to do that by abstracting away the request/check/download cycle for downloading reports / submitting feeds.
Detailed logs describing the state of either lifecycle can be accessed.


## Downloading reports from Amazon MWS using EasyMWS

EasyMWS provides factories that can be used to generate requests for downloading reports from Amazon MWS. One has to specify the report type, any required/optional arguments needed by Amazon to process the report and a set of amazon seller account credentials.
(Adding support for more reports is still ongoing).

A user can create requests to download reports from amazon and can add these requests to an internal EasyMWS queue.

When queuing a request the user also needs to provide a static method reference. This method will be invoked once the request has been completed, in order to provide access to the report content.

All that is left to do is making periodic calls to the Poll() method. This method handles all the lifecycle of requesting reports from amazon. A call every 2 to 5 minutes is recommended in order to make sure request throttling won't happen.

Once a report has been downloaded, the callback method will be invoked and will provide access to the report content.

```
public void Main(object[] arguments)
{
	var euClient = new EasyMwsClient(AmazonRegion.Europe, "EUSellerId", "EUSellerAccessKey", "EUSellerSecretAccessKey");
	var marketplaces = new MwsMarketplaceGroup(marketplace: MwsMarketplace.UK)
			.AddMarketplace(MwsMarketplace.Germany).AddMarketplace(MwsMarketplace.France);
	IReportRequestFactoryInventory reportRequestFactory = new ReportRequestFactoryInventory();
	// if the marketplaces argument is not provided, the report is generated for the region(s) used to initialize the client.
	var propertiesContainer = reportRequestFactory.AllListingsReport(requestedMarketplacesGroup: marketplaces,
			startDate: DateTime.UtcNow.AddMonths(-1), endDate: DateTime.UtcNow);
	var reportFilename = $"AllListingsReport_{DateTime.UtcNow.ToFileTimeUtc()}";
	(string some, int data, string reportFileName) someData = ("C#7 named tuples are supported", 123, reportFilename);
	euClient.QueueReport(propertiesContainer, DoSomethingWithDownloadedReport, someData);

	// A better solution to call Poll repeatedly is recommended. example: https://www.hangfire.io/.
	var timer = new System.Threading.Timer(e => { euClient.Poll(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
}

// This method will be invoked when the report is downloaded.
public static void DoSomethingWithDownloadedReport(Stream reportContent, object someData)
{
	var parameters = ((string some, string data, int reportFilename))someData;
	using (var streamReader = new StreamReader(reportContent))
	{
		File.WriteAllText($@"C:\AmazonReports\{parameters.reportFilename}", streamReader.ReadToEnd());
	}
}
```

## Submitting feeds to Amazon MWS using EasyMWS

A user can also create requests to submit feeds to amazon (feed content has to be provided separately), and add the requests to an internal queue.

When queuing a request the user also needs to provide a static method reference. This method will be invoked once the request has been completed, in order to provide access to the feed submission result report content.

All that is left to do is making periodic calls to the Poll() method. This method also handles all the lifecycle of submitting feeds to amazon. A call every 2 to 5 minutes is recommended in order to make sure request throttling won't happen.

Once a feed has been submitted to amazon and a feed processing result report has been downloaded, the callback method will be invoked and will provide access to the feed processing result report.

```
public void Main(object[] arguments)
{
	var usClient = new EasyMwsClient(AmazonRegion.NorthAmerica, "USSellerId", "USSellerAccessKey", "USSellerSecretAccessKey");
	string productsFeedContent = "This should be the actual products feed content in XML format or some other format accepted by the Amazon MWS SubmitFeed endpoint";
	// if the marketplaces argument is not provided, the feed will be submitted to the region(s) used to initialize the client.
	var propertiesContainer = new FeedSubmissionPropertiesContainer(productsFeedContent, feedType: "_POST_PRODUCT_DATA_", purgeAndReplace: false);
	var feedSubmissionReportFilename = $"ProductsFeed_SubmissionReport_{DateTime.UtcNow.ToFileTimeUtc()}";
	(string some, int data, string reportFileName) someData = ("C#7 named tuples are supported", 123, feedSubmissionReportFilename);

	usClient.QueueFeed(propertiesContainer, DoSomethingWithDownloadedReport, someData);

	// A better solution to call Poll repeatedly is recommended. example: https://www.hangfire.io/.
	var timer = new System.Threading.Timer(e => { usClient.Poll(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
}

// This method will be invoked when the report is downloaded.
// reportContent : This stream contains the report content as received from Amazon.
// someData : This is additional callback data that might be necessary for this method.
public static void DoSomethingWithDownloadedReport(Stream reportContent, object someData)
{
	var parameters = ((string some, string data, int reportFilename))someData;
	using (var streamReader = new StreamReader(reportContent))
	{
		File.WriteAllText($@"C:\AmazonReports\{parameters.reportFilename}", streamReader.ReadToEnd());
	}
}
```


## Sample - getting logs from EasyMWS and overriding EasyMWS default settings

```
public void Main(object[] arguments)
{
	var customEasyMwsOptions = EasyMwsOptions.Defaults();
	customEasyMwsOptions.FeedSubmissionMaxRetryCount = 5;
	customEasyMwsOptions.ReportRequestRetryInterval = TimeSpan.FromMinutes(2);

	log4net.ILog log = log4net.LogManager.GetLogger(GetType());
	var easyMwsLogger = new EasyMwsLogger();
	easyMwsLogger.LogAvailable += (sender, args) => { args.PlugInLog4Net(log); };

	var euClient = new EasyMwsClient(AmazonRegion.Europe, "EUSellerId", "EUSellerAccessKey", "EUSellerSecretAccessKey", easyMwsLogger: easyMwsLogger, options: customEasyMwsOptions);
	var propertiesContainer = new FeedSubmissionPropertiesContainer("feed content", "feed type");
	euClient.QueueFeed(propertiesContainer, DoSomethingWithDownloadedReport, null);
	var timer = new System.Threading.Timer(e => { euClient.Poll(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
}

// Example class containing an extension method that can access EasyMws logs using Log4net.
// Any other logging framework can access EasyMws logs in a similar fashion.
public static class EasyMwsLoggingHelper
{
	public static void PlugInLog4Net(this LogAvailableEventArgs logArgs, ILog log4NetInstance)
	{
		switch (logArgs.Level)
		{
			case EasyMWS.Enums.LogLevel.Info:
				log4NetInstance.Info(logArgs.Message);
				break;
			case EasyMWS.Enums.LogLevel.Warn:
				log4NetInstance.Warn(logArgs.Message);
				break;
			case EasyMWS.Enums.LogLevel.Error:
				{
					log4NetInstance.Error(logArgs.Message, logArgs.Exception);
					if (logArgs.HasRequestInfo) HandleMarketplaceWebServiceException(logArgs.RequestInfo);
					break;
				}
		}
	}

	private static void HandleMarketplaceWebServiceException(RequestInfo requestInfo)
	{
		HttpStatusCode? statusCode = requestInfo.StatusCode;
		string errorCode = requestInfo.ErrorCode;
		string errorType = requestInfo.ErrorType;
		string requestId = requestInfo.RequestId;
		string requestTimestamp = requestInfo.Timestamp;

		// EasyMws deletes queued entries from it's internal queue, if a request from Amazon throws a MarketplaceWebServiceException with a fatal error code.
		// A fatal MarketplaceWebServiceException error code is considered by EasyMws to correspond to a scenario that cannot be retried.
		// If the error code is considered to be a fatal one, an automatic requeueing logic could be triggered from this place.
		// If the error code is considered to be non-fatal, EasyMWS will automatically retry the failing step internally.
		// For more details about amazon error codes see : https://docs.developer.amazonservices.com/en_US/reports/Reports_ErrorCodes.html or the equivalent page for feeds.
		// The following error codes are considered to be fatal by EasyMws : 
		// Report request related error codes : AccessToReportDenied, InvalidReportId, InvalidReportType, InvalidRequest, ReportNoLongerAvailable.
		// Feed request related error codes : AccessToFeedProcessingResultDenied, FeedCanceled, FeedProcessingResultNoLongerAvailable, InputDataError, InvalidFeedType, InvalidRequest.
		// The following error codes are considered to be non-fatal by EasyMws : 
		// Reports related : ReportNotReady, InvalidScheduleFrequency. Feeds related : ContentMD5Missing, ContentMD5DoesNotMatch, FeedProcessingResultNotReady, InvalidFeedSubmissionId. 
	}
}
```

## Details / Requirements

The library is a wrapper for the Amazon MarketplaceWebService .NET implementation.
The library was build using [.NET Standard 2.0](https://blogs.msdn.microsoft.com/dotnet/2017/08/14/announcing-net-standard-2-0/) and as such, it inherits the following minimum requirements :
- Supporting platforms : .NET Framework 4.6.1, .NET Core 2.0 or later.
- Visual Studio 2017 15.3 or later. If you only need to consume the library, you can do that even in Visual Studio 2015 but you'll need NuGet client 3.6 or higher (download from [Nuget.org/downloads](https://www.nuget.org/downloads)). More information [here](https://github.com/dotnet/announcements/issues/24).

Library dependencies  (all the dependencies are available on the NuGet platform):
- .NETStandard 2.0.0
- Microsoft.EntityFrameworkCore 2.1.0
- Microsoft.EntityFrameworkCore.SqlServer 2.1.0
- Microsoft.EntityFrameworkCore.Proxies 2.1.0
- Microsoft.EntityFrameworkCore.Tools 2.1.0
- Microsoft.Extensions.Configuration.Json 2.1.0
- Microsoft.Extensions.Configuration.Xml 2.1.0
- System.Configuration.ConfigurationManager 4.5.0


Package available on NuGet.org at [this location](https://www.nuget.org/packages/MountainWarehouse.EasyMWS/). tags : Amazon, MWS, MarketplaceWebService.

**EasyMws does not currently manage distributed locks. The calling code should either take that into consideration or avoid calling EasyMws in a distributed manner altogether.**
