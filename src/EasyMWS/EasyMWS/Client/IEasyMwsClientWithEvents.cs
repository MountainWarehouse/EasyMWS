﻿using System;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;

namespace MountainWarehouse.EasyMWS.Client
{
	/// <summary>
	/// EasyMws client for downloading reports from Amazon / submitting feeds to Amazon.<para/>
	/// This client type expects the user to subscribe the FeedSubmitted / ReportDownloaded events, which are invoked when the respective actions have happened.
	/// </summary>
	public interface IEasyMwsClientWithEvents
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
	    /// An event that is fired as soon as a report is downloaded from Amazon.
	    /// </summary>
		event EventHandler<ReportDownloadedEventArgs> ReportDownloaded;

	    /// <summary>
	    /// An event that is fired as soon a feed has been submitted to Amazon, and it's corresponding processing report has been downloaded.
	    /// </summary>
		event EventHandler<FeedSubmittedEventArgs> FeedSubmitted;

		/// <summary>
		/// Method that drives the lifecycle of requesting reports from Amazon or submitting feeds to Amazon.
		/// It is handling the following operations : 
		/// 1. Requests the next report from report request queue from Amazon, if a ReportRequestId is successfully generated by amazon then the ReportRequest is moved in a queue of reports awaiting Amazon generation.
		///    If a ReportRequestId is not generated by amazon, a configurable retry policy will be applied.
		/// 2. Query amazon if any of the reports that are pending generation, were generated.
		///    If any reports were successfully generated (returned report processing status is "_DONE_"), those reports are moved to a queue of reports that await downloading.
		///    If any reports requests were canceled by amazon (returned report processing status is "_CANCELLED_"), then those ReportRequests are moved back to the report request queue.
		///    If amazon returns a processing status any other than "_DONE_" or "_CANCELLED_" for any report requests, those ReportRequests are moved back to the report request queue.
		/// 3. Downloads the next report from amazon (which is the next report ReportRequest in the queue of reports awaiting download).
		/// 4. Perform an invocation of the ReportDownloaded or FeedSubmitted events.
		/// 5. Perform a similar lifecycle for queued feeds.
		/// </summary>
		void Poll();

	    /// <summary>
	    /// Add a new ReportRequest to a queue of requests that are going to be processed, with the final result of trying to download the respective report from Amazon.<para/>
	    /// Once a report has been downloaded from Amazon, the ReportDownloaded event will be invoked offering the report content and all related information.
	    /// </summary>
	    /// <param name="reportRequestContainer">An object that contains the arguments required to request the report from Amazon. This object is meant to be obtained by calling a ReportRequestFactory, ex: IReportRequestFactoryFba.</param>
		void QueueReport(ReportRequestPropertiesContainer reportRequestContainer);

	    /// <summary>
	    /// Add a new FeedSubmissionRequest to a queue of feeds to be submitted to amazon, with the final result of obtaining of posting the feed data to amazon and obtaining a response.<para/>
	    /// Once a feed has been submitted to Amazon and it's processing report has been downloaded (and has a valid MD5 signature), the FeedSubmitted event will be invoked offering the feed processing report content and all related information.
	    /// </summary>
	    /// <param name="feedSubmissionContainer">An object that contains the arguments required to submit a feed to Amazon.</param>
		void QueueFeed(FeedSubmissionPropertiesContainer feedSubmissionContainer);
	}
}
