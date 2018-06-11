# EasyMWS

## Description

EasyMWS is a .NET library that intends to simplify the interaction with the Amazon Marketplace Web Services API.
It tries to do that by abstracting away the request/check/download cycle for downloading reports / submitting feeds.
Detailed logs describing the state of either lifecycle can be accessed.


## Downloading reports from Amazon MWS

EasyMws provides factories that can be used to generate requests for downloading reports from Amazon MWS. One has to specify the report type, any required/optional arguments needed by Amazon to process the report and a set of amazon seller account credentials.
(Adding support for more reports is still ongoing).

A user can create requests to download reports from amazon and can add these requests to an internal EasyMws queue.

When queuing a request the user also needs to provide a static method reference. This method will be invoked once the request has been completed, in order to provide access to the report content.

All that is left to do is making periodic calls to the Poll() method. This method handles all the lifecycle of requesting reports from amazon. A call every 2 to 5 minutes is recommended in order to make sure request throttling won't happen.

Once a report has been downloaded, the callback method will be invoked and will provide access to the report content.


## Submitting feeds to Amazon MWS

A user can also create requests to submit feeds to amazon (feed content has to be provided separately), and add the requests to an internal queue.

When queuing a request the user also needs to provide a static method reference. This method will be invoked once the request has been completed, in order to provide access to the feed submission result report content.

All that is left to do is making periodic calls to the Poll() method. This method also handles all the lifecycle of submitting feeds to amazon. A call every 2 to 5 minutes is recommended in order to make sure request throttling won't happen.

Once a feed has been submitted to amazon and a feed processing result report has been downloaded, the callback method will be invoked and will provide access to the feed processing result report.

## Code usage - (this is only to demonstrate how to use the client.)

```
public void Main(object[] arguments) {
	var euClient = new EasyMwsClient(AmazonRegion.Europe, "euSellerId", "accessKey", "secretAccessKey");
	var marketplaces = new MwsMarketplaceGroup(marketplace: MwsMarketplace.UK)
					.AddMarketplace(MwsMarketplace.Germany)
					.AddMarketplace(MwsMarketplace.France);
	IReportRequestFactoryInventory reportRequestFactory = new ReportRequestFactoryInventory();
	var allListingsReportProperties = reportRequestFactory.AllListingsReport(marketplaces);
	var reportFilename = $"AllListingsReport_{DateTime.UtcNow.ToFileTimeUtc()}";
	euClient.QueueReport(allListingsReportProperties, DoSomethingWithDownloadedReport, reportFilename);

	while(true)
	{
		Tread.Sleep(Timespan.FromMinutes(2));
		euClient.Poll();
	}
}

public static void DoSomethingWithDownloadedReport(Stream reportContent, object callbackData)
{
	var filename = (string) callbackData;
	var path = @"C:\AmazonReports";
	using (var streamReader = new StreamReader(reportContent))
	{
		File.WriteAllText($"{path}/{filename}", streamReader.ReadToEnd());
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
