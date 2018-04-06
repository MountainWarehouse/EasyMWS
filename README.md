# EasyMWS

## Description

EasyMWS is a library that intends to simplify the interaction with the Amazon Marketplace Web Services API,  for .NET.
It tries to do that by abstracting away the request/check/download cycle for downloading reports / submitting feeds.
The library is a wrapper for the Amazon MarketplaceWebService .NET implementation.

The package is also available on NuGet at [this location](https://www.nuget.org/packages/MountainWarehouse.EasyMWS/). tags : Amazon, MWS, MarketplaceWebService.

## Features

- A way to queue any number of Amazon MWS Reports for download from Amazon for a specified marketplaces, by only specifying the type of report, and the information required by that specific report type, along with the required seller credentials. _*only a small number of reports have been implemented / live tested so far, support for more reports will be added later on. But in theory any report type will be supported. This is still alpha version._
	
- Simplify the whole lifecycle of requesting reports from amazon to calling a single Poll() method from time to time. The recommended interval is 2 minutes to in order to avoid amazon request throttling.  _*there are still things to be improved around this area, the 2 minutes value is based on the average restore rate of the average request quota .This is still alpha version._

- When queuing a report for download, the user can provide a Callback method that will be invoked once the report has successfully been downloaded from Amazon.  _*at the moment Callback methods tare limited to being static methods. Unless otherwise specified, once a Callback method has been invoked, the corresponding report content / feed processing report content will be deleted from the local database._

- Submit already generated feed files to Amazon.  _*This library does not also handle the generation of feed content. The user has to provide the feed content to submit to Amazon using the EasyMWS client._

- All temporary information used by EasyMWS to run the lifecycles of getting reports from Amazon and submitting feeds to Amazon, is stored in a local SQL server database. _* the user has to provide a valid SQL server connection string pointing to the location where the  database should be created._

- Every relevant action from the lifecycle of requesting a report or submitting a feed can be logged if required, The  logging mechanism can be linked to any logging library of choice to extract the log information. _* for more information about how to access the logs and link EasyMWS to a logging library, follow indications in the library usage example below._


## Requirements

The library was build using [.NET Standard 2.0](https://blogs.msdn.microsoft.com/dotnet/2017/08/14/announcing-net-standard-2-0/) and as such, it inherits the following minimum requirements :
- Supporting platforms : .NET Framework 4.6.1, .NET Core 2.0 or later.
- Visual Studio 2017 15.3 or later. If you only need to consume the library, you can do that even in Visual Studio 2015 but you'll need NuGet client 3.6 or higher (download from [Nuget.org/downloads](https://www.nuget.org/downloads)). More information [here](https://github.com/dotnet/announcements/issues/24).

Library dependencies  (all the dependencies are available on the NuGet platform):
- _.NETStandard,Version=2.0._
- _Microsoft.EntityFrameworkCore.SqlServer (>= 2.0.1)_
- _Microsoft.Extensions.Configuration.Json (>= 2.0.0)_
- _Microsoft.EntityFrameworkCore.Tools (>= 2.0.1)_
- _Microsoft.Extensions.Configuration.Xml (>= 2.0.0)_
- _System.Configuration.ConfigurationManager (>= 4.4.1)_
- _Newtonsoft.Json (>= 11.0.1)_

# Usage

_Basic usage example_ : 
  ```
  public void Main(object[] arguments) {

			log4net.ILog log = log4net.LogManager.GetLogger(GetType());

			var easyMwsLogger = new EasyMwsLogger();
			easyMwsLogger.LogAvailable += (sender, args) => { args.PlugInLog4Net(log); };

			var euClient = new EasyMwsClient(AmazonRegion.Europe, "euSellerId", "accessKey", "secretAccessKey", easyMwsLogger);

			QueueAllListingsReportForEurope(euClient);

			PollEasyMws(euClient);
		}

		/// <summary>
		/// A method that can calls the client.Poll() method every 2 minutes.
		/// A better solution would be recommended, like using Hangfire for example. see https://www.hangfire.io/.
		/// </summary>
		public void PollEasyMws(EasyMwsClient client) {
			var startTimeSpan = TimeSpan.Zero;
			var periodTimeSpan = TimeSpan.FromMinutes(2);

			var timer = new System.Threading.Timer((e) =>
			{
				client.Poll();
			}, null, startTimeSpan, periodTimeSpan);

		}

		public void QueueAllListingsReportForEurope(EasyMwsClient client)
		{
			var marketplaces = new MwsMarketplaceGroup(marketplace: MwsMarketplace.UK)
				.AddMarketplace(MwsMarketplace.Germany)
				.AddMarketplace(MwsMarketplace.France);

			IReportRequestFactoryInventory reportRequestFactory = new ReportRequestFactoryInventory();
			var allListingsReportProperties = reportRequestFactory.AllListingsReport(marketplaces);

			var reportFilename = $"AllListingsReport_{DateTime.UtcNow.ToFileTimeUtc()}";
			(string fancy, int data) someFancyData = ("C#7 named tuples are fancy", 123);

			client.QueueReport(allListingsReportProperties, DoSomethingWithDownloadedReport, (reportFilename, someFancyData));
		}

		/// <summary>
		/// This method will be invoked when the report is downloaded
		/// </summary>
		/// <param name="reportContent">This stream contains the report content as received from Amazon</param>
		/// <param name="callbackData">This is additional callback data that might be necessary for this method</param>
		public static void DoSomethingWithDownloadedReport(Stream reportContent, object callbackData)
		{
			var parameters = ((string Filename, (string Fancy, int Data) FancyData))callbackData;

			var filename = parameters.Filename;
			File.WriteAllText(filename, GetStringFrom(reportContent));

			var fancy = parameters.FancyData.Fancy;
			var data = parameters.FancyData.Data;
			DoSomethingReallyFancy(fancy, data, reportContent);
		}

		/// <summary>
		/// Some generic method that can extract a string from a stream
		/// </summary>
		public static string GetStringFrom(Stream st) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Some method that can do further processing with the received report content
		/// </summary>
		public static void DoSomethingReallyFancy(string fancy, int data, Stream reportContent) {
			throw new NotImplementedException();
		}
  ```
  _Note:  in the example above a C#7 named tuple is sent over as callbackData, but callbackData can be any object._

## Client instantiation
The following parameters are required in order to create a new EasyMwsClient instance :

  _This data is required in order to request reports associated to an amazon seller account, or submit feeds associated to it._
- _AmazonRegion (enum, required)_ - The region of a seller account. This tells the client which MWS root URL to call when making any request.
- _MerchantId (string, required)_ - This is the seller ID provided by Amazon to any seller account.
- _AccessKeyId (string, required), MwsSecretAccessKey (string, required)_ - access keys associated with the seller account.

- IEasyMwsLogger (interface, optional) - An EasyMwsLogger instance can be provided, in order to gain access to logs.
  - EasyMwsLogger exposes the LogAvailable event that has to be subscribed in order to get the logs.
  - The LogAvailable event is fired every time a log entry becomes available, no matter the log level.
  - The event handler provided for the LogAvailable event should ideally contain the wiring to a logging framework like Log4Net.
  - Example - how to wire LogAvailable event handler to log4Net using an extension method: 
  ```
  public static class EasyMwsLoggingHelper
	{
		public static void PlugInLog4Net(this LogAvailableEventArgs logArgs, ILog log4NetInstance) {
			switch (logArgs.Level)
			{
				case EasyMWS.Enums.LogLevel.Info:
					log4NetInstance.Info(logArgs.Message);
					break;
				case EasyMWS.Enums.LogLevel.Warn:
					log4NetInstance.Warn(logArgs.Message);
					break;
				case EasyMWS.Enums.LogLevel.Error:
					log4NetInstance.Error(logArgs.Message, logArgs.Exception);
					break;
			}
		}
	}
  ```
  - Example - how to use the PlugInLog4Net extension method (Ilog is the interface exposed by log4net): 
  ```
  ILog _log = LogManager.GetLogger(GetType());
  
  var easyMwsLogger = new EasyMwsLogger();
  easyMwsLogger.LogAvailable += (sender, args) => { args.PlugInLog4Net(_log); };
  var easyMwsClient = new EasyMwsClient(region, merchantId, accessKey, secretAccessKey, easyMwsLogger);
  ```
  
- _EasyMwsOptions (object, optional)_ - An EasyMwsOptions instance can be provided in order to customize some aspects of the client.
							A static method .Defaults() is available on the object to provide access to the set of default settings,
							If this parameter is not provided, the default options will be used.

## Public behaviour exposed by the EasyMwsClient class :

- **_EasyMwsClient.QueueReport() method_** - a report request is added to an internal queue.
  - _Parameters_ :
  - _ReportRequestPropertiesContainer (required)_ - an object that encapsulates the information necessary to describe a report request.
    - Ideally this object should not be created manually, but instead be provided by a report request factory like
		_MountainWarehouse.EasyMWS.Factories.Reports.IReportRequestFactoryFba_. Such a factory will provide methods that can return the properties containers specific to various report types. _*only a few report request factories have been created so far, given the library is still in alpha version, with more to follow in the future._
    - Using an EasyMWS report request factory to get the properties container for a report type has the advantage of only requiring information that is truly relevant for each report type, for example StartDate, EndDate, MarketplaceIdList.

  - _callbackMethod (required)_ - the name of a static with the following signature should be provided for this argument: 
```public static void ExampleCallbackMethod(Stream stream, object callbackData)```
  - _callbackData (optional)_ - an object that could be a container of parameters that can be used to invoke the callbackMethod with.

- **_EasyMwsClient.QueueFeed() method_** - a feed submission request is added to an internal queue. Works in a similar fashion as QueueReport().

- **_EasyMwsClient.Poll() method_**
Each time the Poll() method is called the following sequence of steps is executed (roughly) :
  - Take the next report that awaits Amazon request permission from queue, and try get a ReportRequestId from Amazon for that report request.
  - Make a call to amazon to request the processing statuses for all reports in the internal queue.
  - Take the next report that has been processed by Amazon from queue, and try to download it from Amazon.
  - If a report is successfully downloaded from Amazon, invoke the method that was provided along with it’s request, and make the report Content available to that method through an parameter.
  
- **_AmazonRegion Enum_** - the possible values are : North America, Europe, India, China, Japan, Brazil, Australia.
This Enum is created in accordance to [amazon documentation](https://docs.developer.amazonservices.com/en_US/dev_guide/DG_Endpoints.html)

- **_IReportRequestFactoryFba interface_**
  - Is an interface that exposes behaviour for generating ReportRequestPropertiesContainer instances necessary to call client.QueueReports().
  - Exposes factory methods corresponding to all Fba Inventory report types, and each method only requires the parameters necessary for its corresponding report type (required and optional parameters).
  - For example the GenerateRequestForReportGetAfnInventoryData() method handles generating a properties container for a report of type _GET_AFN_INVENTORY_DATA_ (see Amazon MWS documentation for information about [report types](https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html)). Since this report type doesn't have any required parameters, it can only receive the optional MarketplaceIdList which is provided through the means of a MwsMarketplaceGroup.

- **_MwsMarketplaceGroup class_**
  - Some report types support the MarketplaceIdList optional parameter, which specifies what marketplaces to generate a report for.
  - But all the marketplaces included a MarketplaceIdList have to belong to the same region, a report cannot be requested for both Canada and France in the same request because they belong to different endpoints.
  - For this reason the MwsMarketplaceGroup class is used to generated the MarketplaceIdList parameter, and provides runtime safety preventing users from including incompatible marketplaces in the same MarketplaceIdList.

  _How to use the MwsMarketplaceGroup class_
  1. Initialize a new MwsMarketplaceGroup object with a MwsMarketplace. The MwsMarketplace object contains static members corresponding to all amazon marketplaces. One MwsMarketplaceGroup corresponds to a single MWS URL endpoint, and such can only contain marketplaces belonging to the same endpoint. The MWS URL endpoint of a MwsMarketplaceGroup is the endpoint of the MwsMarketplace used to initialize that group.
  2. Use the TryAddMarketplace method on a group to attempt adding additional marketplaces to that group. This method throws InvalidOperationException if attempting to add a marketplace with a different MWS endpoint than the one already associated to the group.






  









