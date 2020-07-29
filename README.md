# EasyMWS

## Description

EasyMWS is a .NET library that intends to simplify the interaction with the Amazon Marketplace Web Services API.

It tries to do that by abstracting away the request/check/download cycle for downloading reports / submitting feeds.

EasyMws also provides access to logs describing the internal processing it is doing.


## Downloading reports from Amazon MWS using EasyMWS

An EasyMws client instance needs to be initialised for the desired Amazon region, e.g. Europe or NorthAmerica.
A separate EasyMws client instance will be needed for each different Amazon region to be worked with.
Note : Only one EasyMws client instance per region needs to be running at any given time for that region (in order to avoid EasyMws internal concurrency issues).
Note : Multiple EasyMws client instances can be running at the same time on the same database, provided they are corresponding to different regions.

EasyMws currently contains a reference to the Microsoft.EntityFrameworkCore package which is being used to maintain a persistent internal state.
This means that EasyMws requires a valid connection string to be configured in the entry-point project which calls the EasyMwsClient, the name of the connection string should be "EasyMwsContext".
EasyMws will create 4 tables in the Database specified in the "EasyMwsContext" connection string, needed in order to handle the report and feed requests.
Note : It is not recommended to edit these the contents of these tables, due to a high risk of putting the data in an inconsistent state.

EasyMWS provides factories that can be used to generate requests for downloading reports from Amazon MWS. 
Note : the EasyMws report request factories are defined in the following namespace : MountainWarehouse.EasyMWS.Factories.Reports
Example : The MountainWarehouse.EasyMWS.Factories.Reports.IInventoryReportsFactory class provides access to ready to use report request objects for Amazon inventory reports.

A user can create requests to download reports from amazon and can add these requests to an internal EasyMWS queue.
Note : The report request objects can either be obtained using the EasyMws report request factories mentioned above, or they can be created manually.
Note : To add a report request to the EasyMws internal queue, the EasyMwsClient.QueueReport method needs to be called.

In order to gain access to the report content, an event handler needs to be registered for the IEasyMwsClient.ReportDownloaded dot net event.
The ReportDownloaded event args contains a stream property named ReportContent whcih provides access to the report content.

The EasyMwsClient.Poll method needs to be called recurrently in order to drive the internal lifecycle of EasyMws for requesting reports from amazon.
Warning : Do not call the Poll method too often, otherwise there is a high risk of causing amazon request throttling.
Note : A call every ~1 minute is recommended to minimize the risk of amazon request throttling.
Note : amazon request throttling basically means that too many requests were made to the same amazon API endpoint within a short time window which ends up in the amazon MWS API temporarily refusing to process anymore requests - if this happens the best thing to do is to wait for a while before making another request)
Note : for more information can be found in the Amazon MWS API documentation, https://docs.developer.amazonservices.com/en_US/dev_guide/DG_Throttling.html.

Once the entire lifecycle for a report request has been completed, by repeatedly invoking the Poll method, the report will be downloaded from amazon and EasyMws will fire the ReportDownloaded event to provide access to the report content.

```
public static void Main(string[] args)
		{
			var euClient = new EasyMwsClient(AmazonRegion.Europe, "EUSellerId", "EUSellerAccessKey", "EUSellerSecretAccessKey");

			// REQUIRED: attach action event handlers for easyMws actions
			euClient.ReportDownloaded += EasyMws_ReportDownloaded;


			// Optional: attach error event handlers for easyMws actions
			euClient.ReportRequestFailed += EasyMws_ReportRequestFailed;

			/// Queuing a report for download from amazon ******
			// this variable can be used to filter specific countries for which to download a report
			var inventoryReportMarketplaces = new MwsMarketplaceGroup(MwsMarketplace.UK)
					.AddMarketplace(MwsMarketplace.Germany)
					.AddMarketplace(MwsMarketplace.France);

			IInventoryReportsFactory reportRequestFactory = new InventoryReportsFactory();

			// if the requestedMarketplaces argument is not provided, the report is generated for the region used to initialize the client.
			var inventoryReportContainer = reportRequestFactory.AllListingsReport(
				startDate: DateTime.UtcNow.AddMonths(-1),
				endDate: DateTime.UtcNow,
				requestedMarketplaces: inventoryReportMarketplaces.GetMarketplaces);

			var genericReportProcessingCommand = new GenericReportProcessingCommandArgs
			{
				ReportFilename = $"AllListingsReport_EU_{DateTime.UtcNow.ToFileTimeUtc()}.csv"
			};
			var genericReportProcessingEventArgs = new Dictionary<string, object> { { "CommandArguments", genericReportProcessingCommand } };

			euClient.QueueReport(inventoryReportContainer,
				targetEventId: genericReportProcessingCommand.GetCommandName,
				targetEventArgs: genericReportProcessingEventArgs);


			// REQUIRED: repeatedly call the Poll action, so that EasyMws can perform it's processing lifecycle.
			var timer = new System.Threading.Timer(e => { euClient.Poll(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
		}
```
```
		private static void EasyMws_ReportDownloaded(object sender, ReportDownloadedEventArgs e)
		{
			var commandName = e.TargetHandlerId;

			if (commandName == GenericReportProcessingCommandArgs.CommandName)
			{
				var commandArgs = (GenericReportProcessingCommandArgs)e.TargetHandlerArgs["CommandArguments"];

				using (var fileStream = File.Create($"{commandArgs.TargetFileSavePath}\\{commandArgs.ReportFilename}"))
				using (e.ReportContent)
				{
					e.ReportContent.Seek(0, SeekOrigin.Begin);
					e.ReportContent.CopyTo(fileStream);
				}
			}

			// additional processing commands can handled here
			// the actual processing code does not have to live here though, it can live inside dedicated classes associated to different commands
		}
```
```
		private static void EasyMws_ReportRequestFailed(object sender, ReportRequestFailedEventArgs e)
		{
			var errorDetails = $@"
ReportType: '{e.ReportType}', 
Region: '{e.AmazonRegion}', 
Reason: '{e.RequestFailureReason}', 
LastAmazonProcessingStatus: '{e.LastAmazonStatus ?? "none"}'";

			switch (e.RequestFailureReason)
			{
				case ReportRequestFailureReasonType.ReportRequestMaxRetryCountExceeded:
				case ReportRequestFailureReasonType.ReportDownloadMaxRetryCountExceeded:
				case ReportRequestFailureReasonType.ReportProcessingMaxRetryCountExceeded:
					//_myLogger.Error($"Amazon report download failed. Retry count exceeded. {errorDetails}");
					break;
				case ReportRequestFailureReasonType.InvokeCallbackMaxRetryCountExceeded:
					//_myLogger.Error($"The callback invocation of the ReportDownloaded event has failed. The report was downloaded from amazon.{errorDetails}");
					break;
				default:
					//_myLogger.Error($"Amazon report download failed.{errorDetails}");
					break;
			}
		}
```
```
		public class GenericReportProcessingCommandArgs
		{
			public static string CommandName => typeof(GenericReportProcessingCommandArgs).Name;
			public string GetCommandName => CommandName;

			public string ReportFilenameKey { get; set; }
			public string TargetFileSavePath { get; set; } = "..\\AmazonReports";
			public string ReportFilename { get; set; }
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
