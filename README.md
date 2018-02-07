# EasyMWS

MWS Client (initially for reports) that abstracts away the request/check/download cycle and can run across multiple nodes.

## Usage

Create report requests using the factory class - this ensures everything is set correctly:

```
var reportRequest = ReportFactory.RequestFbaStockReport();
```

You can then submit this report to MWS along with code that is called when the report is available:

This callback code must be a static method and it must fit the delegate of `void (byte[], params object[])`

```
easyMwsClient.RequestReport(reportRequest, FbaStockProcessor.Process
```