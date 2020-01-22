﻿using System;
using System.Collections.Generic;
using System.IO;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Client
{
	/// <summary>
	/// EasyMws client for downloading reports from Amazon / submitting feeds to Amazon.<para/>
	/// This client type expects callback method references when queuing reports or feeds, and invokes those callback methods when the respective actions have happened.
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
		/// Method that drives the life-cycle of requesting reports from Amazon or submitting feeds to Amazon.
		/// It is handling the following operations : 
		/// 1. Requests the next report from report request queue from Amazon, if a ReportRequestId is successfully generated by amazon then the ReportRequest is moved in a queue of reports awaiting Amazon generation.
		///    If a ReportRequestId is not generated by amazon, a configurable retry policy will be applied.
		/// 2. Query amazon if any of the reports that are pending generation, were generated.
		///    If any reports were successfully generated (returned report processing status is "_DONE_"), those reports are moved to a queue of reports that await downloading.
		///    If any reports requests were canceled by amazon (returned report processing status is "_CANCELLED_"), then those ReportRequests are moved back to the report request queue.
		///    If amazon returns a processing status any other than "_DONE_" or "_CANCELLED_" for any report requests, those ReportRequests are moved back to the report request queue.
		/// 3. Downloads the next report from amazon (which is the next report ReportRequest in the queue of reports awaiting download).
		/// 4. Perform a callback of the handler method provided as argument when QueueReport was called. The report content can be obtained by reading the stream argument of the callback method.
		/// 5. Perform a similar life-cycle for queued feeds.
		/// </summary>
		void Poll();

        /// <summary>
        ///  Add a new ReportRequest to a queue of requests that are going to be processed, with the final result of trying to download the respective report from Amazon.<para/>
        ///  Once the report has been downloaded from amazon, it can be obtained by subscribing to the IEasyMwsClient.ReportDownloaded event.<para/>
        ///  Given that each report might be processed in a different manner, the "targetHandlerId" argument can be specified to target a specific process to handle each report at the time the ReportDownloaded event handled is invoked.<para/>
        /// </summary>
        /// <param name="reportRequestContainer">An object that contains the arguments required to request the report from Amazon. This object is meant to be obtained by calling a ReportRequestFactory, ex: IReportRequestFactoryFba.</param>
        /// <param name="targetHandlerId">Optional. A unique arbitrary ID which can be used to associate the specific process calling this method, with its corresponding "ReportDownloaded" event handler.<para/>This can be used to differentiate between different processes targeting different report types.</param>
        /// <param name="targetHandlerArgs">Optional. A dictionary which can be used to specify any data needed at the time the report has been downloaded and is about to be processed, as part of the event handler(s) which will be invoked.</param>
        void QueueReport(ReportRequestPropertiesContainer reportRequestContainer, string targetHandlerId = null, Dictionary<string, object> targetHandlerArgs = null);

        /// <summary>
        /// Add a new FeedSubmissionRequest to a queue of feeds to be submitted to amazon, with the final result of obtaining of posting the feed data to amazon and downloading a feed processing response from amazon.<para/>
        /// Once a feed has been uploaded to amazon, after its corresponding processing report has been downloaded it can be obtained by subscribing to the IEasyMwsClient.FeedUploaded event.<para/>
        /// Given that each feed processing report might be processed in a different manner, the "targetHandlerId" argument can be specified to target a specific process to handle each report at the time the FeedUploaded event handled is invoked.<para/>
        /// </summary>
        /// <param name="feedSubmissionContainer">An object that contains the arguments required to submit a feed to Amazon.</param>
        /// <param name="targetEventId">Optional. A unique arbitrary ID which can be used to associate the specific process calling this method, with its corresponding "FeedUploaded" event handler.<para/>This can be used to differentiate between different processes targeting different feed processing report types.</param>
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
        /// Event which is invoked as soon as a Report has been downloaded from Amazon<para/>
        /// The event provides the following data arguments : <para/>
        ///     -   ReportContent provides access to the report as a stream, as downloaded from amazon.<para/>
        ///     -   ReportType and TargetHandlerId, both of which can be used to identify the report type therefore allows processing the report in a specific manner. <para/>
        ///     -   TargetHandlerId can be provided at the time the report is Queued.<para/>
        ///     -   TargetHandlerArgs contains a dictionary of arbitrary key value pairs which can be provided at the time the report is Queued.
        /// </summary>
        event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;

        /// <summary>
        /// Event which is invoked as soon as a Feed has been uploaded to Amazon<para/>
        /// The event provides the following data arguments : <para/>
        ///     -   ReportContent provides access to the report as a stream, as downloaded from amazon.<para/>
        ///     -   ReportType and TargetHandlerId, both of which can be used to identify the report type therefore allows processing the report in a specific manner. <para/>
        ///     -   TargetHandlerId can be provided at the time the report is Queued.<para/>
        ///     -   TargetHandlerArgs contains a dictionary of arbitrary key value pairs which can be provided at the time the report is Queued.
        /// </summary>
        event EventHandler<FeedUploadedEventArgs> FeedUploaded;
    }
}
