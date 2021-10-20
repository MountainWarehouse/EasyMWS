﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.DTO;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Client
{
    /// <summary>
    /// An amazon MWS client which can be used to queue requests for downloading reports from amazon or for submitting feeds to amazon.<para/>
    /// Once a request to download a report from amazon has been completed, the ReportDownloaded event will be fired providing access to all relevant information and the report itself.<br/>
    /// Once a request to upload a feed to amazon has been completed and the corresponding feed processing report has been downloaded from amazon, <br/>
    /// the FeedUploaded event will be fired providing access to all relevant information and the feed processing report itself.<br/>
    /// </summary>
    public interface IEasyMwsClient
    {
		/// <summary>
		/// The amazon region used to initialize the client
		/// </summary>
		AmazonRegion AmazonRegion { get; }

		/// <summary>
		/// The merchant id used to initialize the client (also known as seller Id).
		/// </summary>
		string MerchantId { get; }

		/// <summary>
		/// The set of options used to initialize the client.
		/// </summary>
		EasyMwsOptions Options { get; }

        /// <summary>
        /// Polling operation which drives the life-cycle of processing any requests queued to amazon with QueueReport() or QueueFeed().<br/>
        /// Once a report has been downloaded the ReportDownloaded event will be fired providing access to the report content.<br/>
        /// Once a feed has bee uploaded and it's processing report has been downloaded the FeedUploaded event will be fired providing access to the processing report content.<para/>
        /// A) Handling requests to download reports from amazon (more info https://docs.developer.amazonservices.com/en_US/reports/Reports_Overview.html)<br/>
        /// 1. Requests the next report from report request queue from Amazon, if a ReportRequestId is successfully generated by amazon then the ReportRequest is moved in a queue of reports awaiting Amazon generation.<br/>
        ///    - If a ReportRequestId is not generated by amazon, a configurable retry policy will be applied.<br/>
        /// 2. Query amazon if any of the reports that are pending generation, were generated.<br/>
        ///    - If any reports were successfully generated (returned report processing status is "_DONE_"), those reports are moved to a queue of reports that await downloading.<br/>
        ///    - If any reports requests were canceled by amazon (returned report processing status is "_CANCELLED_"), then those ReportRequests are moved back to the report request queue.<br/>
        ///    - If amazon returns a processing status any other than "_DONE_" or "_CANCELLED_" for any report requests, those ReportRequests are moved back to the report request queue.<br/>
        /// 3. Downloads the next report from amazon (which is the next report ReportRequest in the queue of reports awaiting download).<br/>
        /// 4. Perform a callback of the handler method provided as argument when QueueReport was called. The report content can be obtained by reading the stream argument of the callback method.<para/>
        /// B) Perform a roughly similar life-cycle to handle requests to upload reports to amazon (more info https://docs.developer.amazonservices.com/en_US/feeds/Feeds_Overview.html)<para/>
        /// Note: the report download request life-cycle and the feed upload request life-cycle are being handled on child threads which run in parallel.
        /// </summary>
        void Poll();

        /// <summary>
        ///  Create a new request to download a report from amazon, and place it in the EasyMws internal queue.<br/>
        ///  In order to process the request, it is necessary to call the Poll() method repeatedly until the ReportDownloaded event is fired, providing access to the downloaded report in it's arguments.<br/>
        ///  Given that each report might be processed in a different manner, the "targetHandlerId" argument can be specified to target a specific process to handle each report at the time the ReportDownloaded event handled is invoked.<br/>
        /// </summary>
        /// <param name="reportRequestContainer">An object that contains the arguments required to request the report from Amazon. This object is meant to be obtained by calling a ReportRequestFactory, ex: IReportRequestFactoryFba.</param>
        /// <param name="targetHandlerId">Optional. A unique arbitrary ID which can be used to associate the specific process calling this method, with its corresponding "ReportDownloaded" event handler.<br/>This can be used to differentiate between different processes targeting different report types.</param>
        /// <param name="targetHandlerArgs">Optional. A dictionary which can be used to specify any data needed at the time the report has been downloaded and is about to be processed, as part of the event handler(s) which will be invoked.</param>
        void QueueReport(ReportRequestPropertiesContainer reportRequestContainer, string targetHandlerId = null, Dictionary<string, object> targetHandlerArgs = null);

        /// <summary>
        /// Create a new request to upload a feed to amazon, and place it in the EasyMws internal queue.<br/>
        /// In order to process the request, it is necessary to call the Poll() method repeatedly until the FeedUploaded event is fired, providing access to the downloaded feed processing report in it's arguments.<br/>
        /// Given that each feed processing report might be processed in a different manner, the "targetHandlerId" argument can be specified to target a specific process to handle each report at the time the FeedUploaded event handled is invoked.<br/>
        /// </summary>
        /// <param name="feedSubmissionContainer">An object that contains the arguments required to submit a feed to Amazon.</param>
        /// <param name="targetEventId">Optional. A unique arbitrary ID which can be used to associate the specific process calling this method, with its corresponding "FeedUploaded" event handler.<br/>This can be used to differentiate between different processes targeting different feed processing report types.</param>
        /// <param name="targetEventArgs">Optional. A dictionary which can be used to specify any data needed at the time the feed processing report has been downloaded and is about to be processed, as part of the event handler(s) which will be invoked.</param>
        void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer, string targetEventId = null, Dictionary<string, object> targetEventArgs = null);

		/// <summary>
		/// Purges the queue of report request entries for the current client instance corresponding to a pair of AmazonRegion and MerchantId
		/// </summary>
	    void PurgeReportRequestEntriesQueue();

		/// <summary>
		/// Purges the queue of feed submission entries for the current client instance corresponding to a pair of AmazonRegion and MerchantId
		/// </summary>
		void PurgeFeedSubmissionEntriesQueue();

        /// <summary>
        /// Get a list of settlement reports which have been automatically generated by Amazon, and are available for download.<br/>
        /// The listed reports are assigned to the AmazonRegion which was used to initialize the client.
        /// Calls the Amazon MWS GetReportList operation, which is subject to request throttling. For more details see https://docs.developer.amazonservices.com/en_US/reports/Reports_GetReportList.html <br/>
        /// Hourly request quota: 60 requests/hour. Maximum request quota: 10 requests. Restore rate: 1 request/minute.<br/>
        /// </summary>
        /// <param name="reportTypeList">The settlement report type. Can be obtained using SettlementReportTypes class.<br/>More info available here : https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html#ReportTypeCategories__SettlementReports</param>
        /// <param name="availableFromDate"></param>
        /// <param name="availableToDate"></param>
        /// <param name="isAcknowledged">To keep track of which reports you have already received, it is a good practice to acknowledge reports after you have received and stored them successfully. <br/>Then, when you submit a GetReportList request, you can specify to receive only reports that have not yet been acknowledged.<br/> To retrieve reports that have been lost, set the Acknowledged to false - This action returns a list of all reports within the previous 90 days that match the query parameters. <br/> More info: https://docs.developer.amazonservices.com/en_UK/reports/Reports_UpdateReportAcknowledgements.html</param>
        /// <returns></returns>
        Task<IEnumerable<SettlementReportDetails>> ListSettlementReportsAsync(List<string> reportTypeList, DateTime? availableFromDate = null, DateTime? availableToDate = null, bool? isAcknowledged = null);

        /// <summary>
        /// Get a list of settlement reports which have been automatically generated by Amazon, and are available for download.<br/>
        /// The listed reports are assigned to the AmazonRegion which was used to initialize the client.
        /// Calls the Amazon MWS GetReportList operation, which is subject to request throttling. For more details see https://docs.developer.amazonservices.com/en_US/reports/Reports_GetReportList.html <br/>
        /// Hourly request quota: 60 requests/hour. Maximum request quota: 10 requests. Restore rate: 1 request/minute.<br/>
        /// </summary>
        /// <param name="reportTypeList">The settlement report type. Can be obtained using SettlementReportTypes class.<br/>More info available here : https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html#ReportTypeCategories__SettlementReports</param>
        /// <param name="availableFromDate"></param>
        /// <param name="availableToDate"></param>
        /// <param name="isAcknowledged">To keep track of which reports you have already received, it is a good practice to acknowledge reports after you have received and stored them successfully. <br/>Then, when you submit a GetReportList request, you can specify to receive only reports that have not yet been acknowledged.<br/> To retrieve reports that have been lost, set the Acknowledged to false - This action returns a list of all reports within the previous 90 days that match the query parameters. <br/> More info: https://docs.developer.amazonservices.com/en_UK/reports/Reports_UpdateReportAcknowledgements.html</param>
        IEnumerable<SettlementReportDetails> ListSettlementReports(List<string> reportTypeList, DateTime? availableFromDate = null, DateTime? availableToDate = null, bool? isAcknowledged = null);

        /// <summary>
        /// Event which is invoked as soon as a requested report has been downloaded from Amazon<br/>
        /// The ReportDownloadedEventArgs event parameter provides access to the following data : <br/>
        ///     -   ReportContent provides access to the report as a stream, as downloaded from amazon.<br/>
        ///     -   ReportType can be used to identify the initially queued report type therefore allows processing the report in a specific manner. <br/>
        ///     -   TargetHandlerId can be provided at the time the report is Queued to specify an identifier for a specific process to be used/invoked once the report is downloaded from amazon.<br/>
        ///     -   argetHandlerArgs contains a dictionary of arbitrary key-value-pairs which can be provided at the time the report is Queued, in order for them to be used at the time the report is downloaded from amazon.
        /// </summary>
        event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;

        /// <summary>
        /// Event which is invoked as soon as a feed processing report has been downloaded from Amazon<br/>
        /// The FeedUploadedEventArgs event parameter provides access to the following data : <br/>
        ///     -   ProcessingReportContent provides access to the feed processing report, as downloaded from amazon.<br/>
        ///     -   FeedType can be used to identify the initially queued feed type therefore allows processing the feed processing report in a specific manner. <br/>
        ///     -   TargetHandlerId can be provided at the time the feed is Queued to specify an identifier for a specific process to be used/invoked once the feed processing report is downloaded from amazon.<br/>
        ///     -   TargetHandlerArgs contains a dictionary of arbitrary key-value-pairs which can be provided at the time the feed is Queued, in order for them to be used at the time the processing report is downloaded from amazon .
        /// </summary>
        event EventHandler<FeedUploadedEventArgs> FeedUploaded;

        /// <summary>
        /// Event which is invoked as soon as a previously queued report download request encounters a problem which will cause it to be deleted from the internal EasyMws queue.<br/>
        /// The ReportRequestFailedEventArgs event parameter provides access to the following data :<br/>
        ///     -   RequestFailureReason : the reason for which the report request has failed. For example the maximum retry limit might has been reached while trying to perform a certain step from the lifecycle of getting the report from amazon.<br/>
        ///     -   LastAmazonStatus : the last report processing status received from amazon regarding the failed request.<br/>
        ///     -   LastAmazonRequestTimestamp : the timestamp associated to the last request of any kind to the Amazon MWS API regarding the failed request.<para/>
        ///     -   AmazonRegion : the amazon region associated to the EasyMws client instance used to queue the report (this can be used to re-queue the report if necessary).<br/>
        ///     -   ReportType : The report type associated to the affected report download request entry<br/> https://docs.developer.amazonservices.com/en_US/reports/Reports_ReportType.html
        ///     -   ReportRequestPropertiesContainer : the properties container parameter used to queue the report.<br/>
        ///     -   TargetHandlerId and TargetHandlerArgs : parameters that might have potentially been used to queue the report (these can be used to re-queue the report if necessary).<para/>
        ///     -   ReportRequestId and GeneratedReportId : parameters related to the report request from amazon; These could be used in the amazon scratchpad or an external tool to query the status of the report request.<br/>
        ///     -   Amazon scratchpad url : https://mws.amazonservices.co.uk/scratchpad/index.html<br/>
        ///     -   More information about the amazon report request lifecycle : https://docs.developer.amazonservices.com/en_US/reports/Reports_Overview.html<br/>
        /// </summary>
        event EventHandler<ReportRequestFailedEventArgs> ReportRequestFailed;

        /// <summary>
        /// Event which is invoked as soon as a previously queued feed upload request encounters a problem which will cause it to be deleted from the internal EasyMws queue.<br/>
        /// The FeedRequestFailedEventArgs event parameter provides access to the following data :<br/>
        ///     -   RequestFailureReason : the reason for which the feed request has failed. For example the maximum retry limit might has been reached while trying to perform a certain step from the lifecycle of uploading a feed to amazon and getting its submission report from amazon.<br/>
        ///     -   LastAmazonStatus : the last report processing status received from amazon regarding the failed request.<br/>
        ///     -   LastAmazonRequestTimestamp : the timestamp associated to the last request of any kind to the Amazon MWS API regarding the failed request.<para/>
        ///     -   AmazonRegion : the amazon region associated to the EasyMws client instance used to queue the feed (this can be used to re-queue the feed if necessary).<br/>
        ///     -   FeedType : The feed type associated to the affected feed upload request entry<br/> https://docs.developer.amazonservices.com/en_US/feeds/Feeds_FeedType.html
        ///     -   FeedContent :  The feed content associated to the affected feed upload request entry.
        ///     -   FeedSubmissionPropertiesContainer : the properties container parameter used to queue the feed.
        ///     -   TargetHandlerId and TargetHandlerArgs : parameters that might have potentially been used to queue the feed (these can be used to re-queue the feed if necessary).<para/>
        ///     -   FeedSubmissionId : parameter related to the report request from amazon; This could be manually used in the amazon scratchpad or an external tool to query the status of the feed submission.<br/>
        ///     -   Amazon scratchpad url : https://mws.amazonservices.co.uk/scratchpad/index.html<br/>
        ///     -   More information about the amazon report request lifecycle : https://docs.developer.amazonservices.com/en_US/feeds/Feeds_Overview.html<br/>
        /// </summary>
        event EventHandler<FeedRequestFailedEventArgs> FeedRequestFailed;
    }
}
