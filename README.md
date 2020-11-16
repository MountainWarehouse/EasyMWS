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
The ReportDownloaded event args contains a stream property named ReportContent which provides access to the report content.  

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

An EasyMws client instance needs to be initialised for the desired Amazon region, e.g. Europe or NorthAmerica.  
A separate EasyMws client instance will be needed for each different Amazon region to be worked with.  
Note : Only one EasyMws client instance per region needs to be running at any given time for that region (in order to avoid EasyMws internal concurrency issues).  
Note : Multiple EasyMws client instances can be running at the same time on the same database, provided they are corresponding to different regions.  


A user can create requests to submit feeds to amazon, and add the requests to an internal EasyMWS queue.  
Note : EasyMws does not currently provide support for building amazon data feeds.  
Note : To add a report request to the EasyMws internal queue, the EasyMwsClient.QueueFeed method needs to be called.  

In order to gain access to the Amazon feed processing report content, an event handler needs to be registered for the IEasyMwsClient.FeedUploaded dot net event.  
The FeedUploaded event args contains a stream property named ProcessingReportContent which provides access to the amazon feed processing report content. 

The EasyMwsClient.Poll method needs to be called recurrently in order to drive the internal lifecycle of EasyMws for requesting reports from amazon.  
Warning : Do not call the Poll method too often, otherwise there is a high risk of causing amazon request throttling.  
Note : A call every ~1 minute is recommended to minimize the risk of amazon request throttling.  
Note : amazon request throttling basically means that too many requests were made to the same amazon API endpoint within a short time window which ends up in the amazon MWS API temporarily refusing to process anymore requests - if this happens the best thing to do is to wait for a while before making another request)  
Note : for more information can be found in the Amazon MWS API documentation, https://docs.developer.amazonservices.com/en_US/dev_guide/DG_Throttling.html.  

Once the entire lifecycle for a feed request has been completed, by repeatedly invoking the Poll method, the feed will be uploaded to amazon, the feed processing report will be downloaded from amazon and EasyMws will fire the FeedUploaded event to provide access to the processing report content.


```
public static void Main(string[] args)
		{
			var euClient = new EasyMwsClient(AmazonRegion.Europe, "EUSellerId", "EUSellerAccessKey", "EUSellerSecretAccessKey");

			// REQUIRED: attach action event handlers for easyMws actions
			euClient.FeedUploaded += EasyMws_FeedUploaded;


			// Optional: attach error event handlers for easyMws actions
			euClient.FeedRequestFailed += EasyMws_FeedRequestFailed;


			/// Queuing a feed for upload to amazon ************
			var productFeedMarketplaces = new MwsMarketplaceGroup(MwsMarketplace.Italy)
					.AddMarketplace(MwsMarketplace.Spain);

			// documentation for available feed types and formats : https://docs.developer.amazonservices.com/en_US/feeds/Feeds_FeedType.html
			var myProductFeedContent = "product listings in xml format as required by Amazon";
			var productFeedContainer = new FeedSubmissionPropertiesContainer(
				feedContent: myProductFeedContent,
				feedType: "_POST_PRODUCT_DATA_",
				marketplaceIdList: productFeedMarketplaces.GetMarketplacesIdList.ToList());

			var uploadFileToHubCommand = new UploadFileToToFileStorageCommandArgs
			{
				// Example property pointing to a specific file cloud location
				FileStorageLocation = "myCloudLocation>Amazon/Feeds/ProcessingReports/",
				Filename = $"ProducFeed_EU_{DateTime.UtcNow.ToFileTimeUtc()}"
			};
			var uploadFileToHubEventArgs = new Dictionary<string, object> { { "CommandArguments", uploadFileToHubCommand } };

			euClient.QueueFeed(productFeedContainer, uploadFileToHubCommand.GetCommandName, uploadFileToHubEventArgs);

			/// REQUIRED: repeatedly call the Poll action, so that EasyMws can perform it's processing lifecycle.
			var timer = new System.Threading.Timer(e => { euClient.Poll(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
		}
```
```
		private static void EasyMws_FeedUploaded(object sender, FeedUploadedEventArgs e)
		{
			var commandName = e.TargetHandlerId;

			if (commandName == UploadFileToToFileStorageCommandArgs.CommandName)
			{
				var commandArgs = (UploadFileToToFileStorageCommandArgs)e.TargetHandlerArgs["CommandArguments"];

				// upload file to remote storage location e.g. :
				// _myRemoteStorageService.UploadFile(
				//			fileContentStream: e.ProcessingReportContent, 
				//			filename: commandArgs.Filename
				//			location: commandArgs.FileStorageLocation);
			}

			// additional processing commands can handled here
			// the actual processing code does not have to live here though, it can live inside dedicated classes associated to different commands
		}
```
```
		private static void EasyMws_FeedRequestFailed(object sender, FeedRequestFailedEventArgs e)
		{
			var errorDetails = $@"
FeedType: '{e.FeedType}', 
Region: '{e.AmazonRegion}', 
Reason: '{e.RequestFailureReason}', 
LastAmazonProcessingStatus: '{e.LastAmazonStatus ?? "none"}', 
Submission ID: '{e.FeedSubmissionId}'";

			switch (e.RequestFailureReason)
			{
				case FeedRequestFailureReasonType.FeedSubmissionMaxRetryCountExceeded:
				case FeedRequestFailureReasonType.FeedProcessingMaxRetryCountExceeded:
					//_logger.Error($"Amazon feed upload failed.{errorDetails}");
					break;
				case FeedRequestFailureReasonType.ProcessingReportDownloadMaxRetryCountExceeded:
					//_logger.Error($"Amazon feed processing report download failed. The feed was successfully uploaded to amazon. Callback was not invoked.{errorDetails}");
					break;
				case FeedRequestFailureReasonType.InvokeCallbackMaxRetryCountExceeded:
					//_logger.Error($"Post feed-upload callback invocation failed. The feed was successfully uploaded to amazon. The processing report was downloaded from amazon.{errorDetails}");
					break;
				case FeedRequestFailureReasonType.FeedSubmissionEntryExpirationPeriodExceeded:
				default:
					//_logger.Error($"Amazon feed upload failed.{errorDetails}");
					break;
			}
		}
```
```
		public class UploadFileToToFileStorageCommandArgs
		{
			public static string CommandName => typeof(UploadFileToToFileStorageCommandArgs).Name;
			public string GetCommandName => CommandName;


			public string FileStorageLocation { get; set; }
			public string Filename { get; set; }
		}
```


## Getting logs from EasyMWS and overriding EasyMWS default settings

```
public static void Main(string[] args)
        {
			/// Setting up custom EasyMwsClient options
			var customEasyMwsOptions = new EasyMwsOptions(useDefaultValues: true)
			{
				FeedSubmissionOptions = new FeedSubmissionOptions(useDefaultValues: true)
				{
					FeedProcessingMaxRetryCount = 10
				}
			};

			/// Wiring up EasyMws logging to an arbitrary logging framework, for example Log4Net
			var myLogger = LogManager.GetLogger("EasyMwsService");
			var easyMwsLogger = new EasyMwsLogger();
			easyMwsLogger.LogAvailable += (sender, logArgs) => { logArgs.PlugInLog4Net(myLogger); };

			var euClient = new EasyMwsClient(AmazonRegion.Europe, "EUSellerId", "EUSellerAccessKey", "EUSellerSecretAccessKey",
				easyMwsLogger: easyMwsLogger,
				options: customEasyMwsOptions);
		}
```
```
public static class EasyMwsLoggingHelper
	{
		public static void PlugInLog4Net(this LogAvailableEventArgs logArgs, ILog log4NetInstance)
		{
			switch (logArgs.Level)
			{
				case LogLevel.Debug:
					log4NetInstance.Debug(logArgs.Message);
					break;
				case LogLevel.Info:
					log4NetInstance.Info(logArgs.Message);
					break;
				case LogLevel.Warn:
					log4NetInstance.Warn(logArgs.Message);
					break;
				case LogLevel.Error:
					log4NetInstance.Error(logArgs.Message, logArgs.Exception);
					break;
				case LogLevel.Fatal:
					log4NetInstance.Fatal(logArgs.Message, logArgs.Exception);
					break;
			}
		}
	}
```

## Details / Requirements

The library is a wrapper for the Amazon MarketplaceWebService .NET implementation.
The library was build using [.NET Standard 2.0](https://blogs.msdn.microsoft.com/dotnet/2017/08/14/announcing-net-standard-2-0/) and as such, it inherits the following minimum requirements :
- Supporting platforms : .NET Framework 4.6.1, .NET Core 2.0 or later.
- Visual Studio 2017 15.3 or later. If you only need to consume the library, you can do that even in Visual Studio 2015 but you'll need NuGet client 3.6 or higher (download from [Nuget.org/downloads](https://www.nuget.org/downloads)). More information [here](https://github.com/dotnet/announcements/issues/24).


Package available on NuGet.org at [this location](https://www.nuget.org/packages/MountainWarehouse.EasyMWS/). tags : Amazon, MWS, MarketplaceWebService.

**EasyMws does not currently manage distributed locks. The calling code should either take that into consideration or avoid calling EasyMws in a distributed manner altogether.**

**Due to it's dependency on Microsoft.EntityFrameworkCore, there is a chance EasyMws will cause package version conflicts when used in a solution which already uses any of the packages listed in it's dependencies, but with different versions. This is a known issue and the reason why there are plans to remove the dependency on Microsoft.EntityFrameworkCore from EasyMws in the future.**
