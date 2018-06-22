using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class FeedSubmissionProcessor : IFeedSubmissionProcessor
	{
		private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
		private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
		private readonly AmazonRegion _region;
		private readonly string _merchantId;

		internal FeedSubmissionProcessor(AmazonRegion region, string merchantId, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
		}


		public void SubmitFeedToAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmission)
		{
			void HandleSubmitFeedSuccess(SubmitFeedResponse response)
			{
				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				feedSubmission.FeedSubmissionId = response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId;
				feedSubmission.FeedSubmissionRetryCount = 0;
				_logger.Info($"AmazonMWS feed submission has succeeded for {feedSubmission.RegionAndTypeComputed}. FeedSubmissionId:'{feedSubmission.FeedSubmissionId}'.",
					new RequestInfo(timestamp, requestId));
			}
			void HandleMissingFeedSubmissionId()
			{
				feedSubmission.FeedSubmissionRetryCount++;
				_logger.Warn($"SubmitFeed did not generate a FeedSubmissionId for {feedSubmission.RegionAndTypeComputed}. Feed submission will be retried. FeedSubmissionRetryCount is now : {feedSubmission.FeedSubmissionRetryCount}.");
			}
			void HandleNonFatalOrGenericException(Exception e)
			{
				feedSubmission.FeedSubmissionRetryCount++;
				feedSubmission.LastSubmitted = DateTime.UtcNow;
				feedSubmissionService.Update(feedSubmission);
				_logger.Error($"AmazonMWS SubmitFeed failed for {feedSubmission.RegionAndTypeComputed}. Feed submission will be retried. FeedSubmissionRetryCount is now : {feedSubmission.FeedSubmissionRetryCount}.", e);
			}
			void HandleFatalException(Exception e)
			{
				feedSubmissionService.Delete(feedSubmission);
				_logger.Error($"AmazonMWS SubmitFeed failed for {feedSubmission.RegionAndTypeComputed}. The entry will now be removed from queue.", e);
			}

			var missingInformationExceptionMessage = "Cannot submit queued feed to amazon due to missing feed submission information";

			if (feedSubmission == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission entry is null.");
			if (feedSubmission.FeedSubmissionData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission data is null.");

			var feedContentZip = feedSubmission.Details?.FeedContent;
			if (feedContentZip == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed content is missing.");

			if (string.IsNullOrEmpty(feedSubmission?.FeedType)) throw new ArgumentException($"{missingInformationExceptionMessage}: Feed type is missing.");

			_logger.Info($"Attempting to submit the next feed in queue to Amazon: {feedSubmission.RegionAndTypeComputed}.");

			var feedSubmissionData = feedSubmission.GetPropertiesContainer();

			using (var stream = ZipHelper.ExtractArchivedSingleFileToStream(feedContentZip))
			{
				var submitFeedRequest = new SubmitFeedRequest
				{
					Merchant = feedSubmission.MerchantId,
					FeedType = feedSubmission.FeedType,
					FeedContent = stream,
					MarketplaceIdList = feedSubmissionData.MarketplaceIdList == null ? null : new IdList {Id = feedSubmissionData.MarketplaceIdList},
					PurgeAndReplace = feedSubmissionData.PurgeAndReplace ?? false,
					ContentMD5 = MD5ChecksumHelper.ComputeHashForAmazon(stream)
				};

				try
				{
					var response = _marketplaceWebServiceClient.SubmitFeed(submitFeedRequest);
					feedSubmission.LastSubmitted = DateTime.UtcNow;

					if (string.IsNullOrEmpty(response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId))
					{
						HandleMissingFeedSubmissionId();
					}
					else
					{
						HandleSubmitFeedSuccess(response);
					}

					feedSubmissionService.Update(feedSubmission);
				}
				catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
				{
					HandleFatalException(e);
				}
				catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
				{
					HandleNonFatalOrGenericException(e);
				}
				catch (Exception e)
				{
					HandleNonFatalOrGenericException(e);
				}
				finally
				{
					stream.Dispose();
					feedSubmissionService.SaveChanges();
				}
			}
		}

		public List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(IEnumerable<string> feedSubmissionIdList, string merchant)
		{
			_logger.Info($"Attempting to request feed submission statuses for all feeds in queue.");

			var request = new GetFeedSubmissionListRequest() {FeedSubmissionIdList = new IdList(), Merchant = merchant};
			request.FeedSubmissionIdList.Id.AddRange(feedSubmissionIdList);

			try
			{
				var response = _marketplaceWebServiceClient.GetFeedSubmissionList(request);

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info($"AmazonMWS request for feed submission statuses succeeded.", new RequestInfo(timestamp, requestId));

				var responseInfo = new List<(string FeedSubmissionId, string IsProcessingComplete)>();

				if (response?.GetFeedSubmissionListResult?.FeedSubmissionInfo != null)
				{
					foreach (var feedSubmissionInfo in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
					{
						responseInfo.Add((feedSubmissionInfo.FeedSubmissionId, feedSubmissionInfo.FeedProcessingStatus));
					}
				}
				else
				{
					_logger.Warn("AmazonMWS GetFeedSubmissionList response does not contain any results. The operation will be executed again at the next poll request.");
				}
				
				return responseInfo;
			}
			catch (MarketplaceWebServiceException e)
			{
				_logger.Error($"AmazonMWS GetFeedSubmissionList failed! The operation will be executed again at the next poll request.", e);
				return null;
			}
			catch (Exception e)
			{
				_logger.Error($"AmazonMWS GetFeedSubmissionList failed! The operation will be executed again at the next poll request.", e);
				return null;
			}
		}

		public void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionEntryService feedSubmissionService, List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses)
		{
			foreach (var feedSubmissionInfo in feedProcessingStatuses)
			{
				var feedSubmissionEntry = feedSubmissionService.FirstOrDefault(fsc => fsc.FeedSubmissionId == feedSubmissionInfo.FeedSubmissionId);
				if(feedSubmissionEntry == null) continue;

				var pendingProcessingStatusesAndInfo = new Dictionary<string, string>()
				{
					{"_AWAITING_ASYNCHRONOUS_REPLY_", "The request is being processed, but is waiting for external information before it can complete."},
					{"_IN_PROGRESS_", "The request is being processed."},
					{"_IN_SAFETY_NET_", "The request is being processed, but the system has determined that there is a potential error with the feed (for example, the request will remove all inventory from a seller's account.) An Amazon seller support associate will contact the seller to confirm whether the feed should be processed."},
					{"_SUBMITTED_", "The request has been received, but has not yet started processing."},
					{"_UNCONFIRMED_", "The request is pending."}
				};

				var genericProcessingInfo = $"ProcessingStatus returned by Amazon for {feedSubmissionEntry.RegionAndTypeComputed} is '{feedSubmissionInfo.FeedProcessingStatus}'.";

				if (feedSubmissionInfo.FeedProcessingStatus == "_DONE_")
				{
					feedSubmissionEntry.IsProcessingComplete = true;
					feedSubmissionEntry.FeedProcessingRetryCount = 0;
					_logger.Info($"{genericProcessingInfo}. The request has been processed. The feed processing report download is ready to be attempted.");
				}
				else if (pendingProcessingStatusesAndInfo.Keys.Any(pendingStatus => pendingStatus == feedSubmissionInfo.FeedProcessingStatus))
				{
					var specificProcessingInfo = pendingProcessingStatusesAndInfo[feedSubmissionInfo.FeedProcessingStatus];
					feedSubmissionEntry.FeedProcessingRetryCount = 0;
					_logger.Info($"{genericProcessingInfo} {specificProcessingInfo} The feed processing status will be checked again at the next poll request.");
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_CANCELLED_")
				{
					feedSubmissionEntry.FeedProcessingRetryCount++;
					feedSubmissionEntry.FeedSubmissionId = null;
					_logger.Info($"{genericProcessingInfo}. The request has been aborted due to a fatal error. The feed submission operation will be retried. FeedProcessingRetryCount is now '{feedSubmissionEntry.FeedProcessingRetryCount}'.");
				}
				else
				{
					feedSubmissionEntry.FeedProcessingRetryCount++;
					_logger.Info($"{genericProcessingInfo}. The feed submission operation will be retried. This feed processing status is not yet handled by EasyMws. FeedProcessingRetryCount is now '{feedSubmissionEntry.FeedProcessingRetryCount}'.");
				}
				feedSubmissionService.Update(feedSubmissionEntry);
			}

			feedSubmissionService.SaveChanges();
		}

		public void DownloadFeedSubmissionResultFromAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmissionEntry)
		{
			var missingInformationExceptionMessage = "Cannot download report from amazon due to missing report request information";

			if (feedSubmissionEntry == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission entry is null.");
			if (string.IsNullOrEmpty(feedSubmissionEntry.FeedSubmissionId)) throw new ArgumentException($"{missingInformationExceptionMessage}: FeedSubmissionId is missing.");

			_logger.Info($"Attempting to request the feed submission result for the next feed in queue from Amazon: {feedSubmissionEntry.RegionAndTypeComputed}.");

			var reportResultStream = new MemoryStream();
			var request = new GetFeedSubmissionResultRequest
			{
				FeedSubmissionId = feedSubmissionEntry.FeedSubmissionId,
				Merchant = feedSubmissionEntry.MerchantId,
				FeedSubmissionResult = reportResultStream
			};

			try
			{
				var response = _marketplaceWebServiceClient.GetFeedSubmissionResult(request);
				feedSubmissionEntry.LastSubmitted = DateTime.UtcNow;

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info($"Feed submission report download from Amazon has succeeded for {feedSubmissionEntry.RegionAndTypeComputed}.",
					new RequestInfo(timestamp, requestId));

				var md5Hash = response?.GetFeedSubmissionResultResult?.ContentMD5;
				var hasValidHash = MD5ChecksumHelper.IsChecksumCorrect(reportResultStream, md5Hash);
				if (hasValidHash)
				{
					_logger.Info($"Checksum verification succeeded for feed submission report for {feedSubmissionEntry.RegionAndTypeComputed}");
					feedSubmissionEntry.Details.FeedContent = null;
					feedSubmissionEntry.ReportDownloadRetryCount = 0;

					using (var streamReader = new StreamReader(reportResultStream))
					{
						var zippedProcessingReport = ZipHelper.CreateArchiveFromContent(streamReader.ReadToEnd());
						feedSubmissionEntry.Details.FeedSubmissionReport = zippedProcessingReport;
					}
				}
				else
				{
					feedSubmissionEntry.ReportDownloadRetryCount++;
					_logger.Warn($"Checksum verification failed for feed submission report for {feedSubmissionEntry.RegionAndTypeComputed}");
				}

				feedSubmissionService.Update(feedSubmissionEntry);
			}
			catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
			{
				feedSubmissionService.Delete(feedSubmissionEntry);
				_logger.Error($"AmazonMWS feed submission report download failed for {feedSubmissionEntry.RegionAndTypeComputed}! The entry will now be removed from queue.", e);
			}
			catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
			{
				feedSubmissionEntry.ReportDownloadRetryCount++;
				feedSubmissionEntry.LastSubmitted = DateTime.UtcNow;
				feedSubmissionService.Update(feedSubmissionEntry);
				_logger.Error($"AmazonMWS feed submission report download failed for {feedSubmissionEntry.RegionAndTypeComputed}! Report download will be retried. ReportDownloadRetryCount is now : {feedSubmissionEntry.ReportDownloadRetryCount}.", e);
			}
			catch (Exception e)
			{
				feedSubmissionEntry.ReportDownloadRetryCount++;
				feedSubmissionEntry.LastSubmitted = DateTime.UtcNow;
				feedSubmissionService.Update(feedSubmissionEntry);
				_logger.Error($"AmazonMWS feed submission report download failed for {feedSubmissionEntry.RegionAndTypeComputed}! Report download will be retried. ReportDownloadRetryCount is now : {feedSubmissionEntry.ReportDownloadRetryCount}.", e);
			}
			finally
			{
				feedSubmissionService.SaveChanges();
			}
		}

		public void CleanUpFeedSubmissionQueue(IFeedSubmissionEntryService feedSubmissionService)
		{
			_logger.Info("Executing cleanup of feed submission requests queue.");
			var allFeedSubmissionEntries = feedSubmissionService.GetAll();
			var removedEntriesIds = new List<int>();

			var entriesWithFeedSubmissionRetryCountExceeded = allFeedSubmissionEntries.Where(fse => IsMatchForRegionAndMerchantId(fse) && IsFeedSubmissionRetryCountExceeded(fse));
			var entriesWithFeedSubmissionRetryCountExceededDeleteReason = $"FeedSubmissionMaxRetryCount exceeded.";
			MarkEntriesAsDeleted(feedSubmissionService, entriesWithFeedSubmissionRetryCountExceeded, removedEntriesIds, entriesWithFeedSubmissionRetryCountExceededDeleteReason);

			var entriesWithReportDownloadRetryCountExceeded = allFeedSubmissionEntries.Where(fse => IsMatchForRegionAndMerchantId(fse) && IsReportDownloadRetryCountExceeded(fse));
			var entriesWithReportDownloadRetryCountExceededDeleteReason = $"ReportDownloadMaxRetryCount exceeded.";
			MarkEntriesAsDeleted(feedSubmissionService, entriesWithReportDownloadRetryCountExceeded, removedEntriesIds, entriesWithReportDownloadRetryCountExceededDeleteReason);

			var entriesWithExpirationPeriodExceeded = allFeedSubmissionEntries.Where(fse => IsMatchForRegionAndMerchantId(fse) && IsExpirationPeriodExceeded(fse));
			var entriesWithExpirationPeriodExceededDeleteReason = $"FeedSubmissionRequestEntryExpirationPeriod exceeded.";
			MarkEntriesAsDeleted(feedSubmissionService, entriesWithExpirationPeriodExceeded, removedEntriesIds, entriesWithExpirationPeriodExceededDeleteReason);

			var entriesWithFeedProcessingRetryCountExceeded = allFeedSubmissionEntries.Where(fse => IsMatchForRegionAndMerchantId(fse) && IsFeedProcessingRetryCountExceeded(fse));
			var entriesWithFeedProcessingRetryCountExceededDeleteReason = $"FeedProcessingMaxRetryCount exceeded.";
			MarkEntriesAsDeleted(feedSubmissionService, entriesWithFeedProcessingRetryCountExceeded, removedEntriesIds, entriesWithFeedProcessingRetryCountExceededDeleteReason);

			var entriesWithCallbackInvocationRetryCountExceeded = allFeedSubmissionEntries.Where(fse => IsMatchForRegionAndMerchantId(fse) && IsCallbackInvocationRetryCountExceeded(fse));
			var entriesWithCallbackInvocationRetryCountExceededDeleteReason = $"InvokeCallbackMaxRetryCount exceeded.";
			MarkEntriesAsDeleted(feedSubmissionService, entriesWithCallbackInvocationRetryCountExceeded, removedEntriesIds, entriesWithCallbackInvocationRetryCountExceededDeleteReason);

			feedSubmissionService.SaveChanges();
		}


		private bool IsMatchForRegionAndMerchantId(FeedSubmissionEntry e) => e.AmazonRegion == _region && e.MerchantId == _merchantId;
		private bool IsFeedSubmissionRetryCountExceeded(FeedSubmissionEntry e) => e.FeedSubmissionRetryCount > _options.FeedSubmissionMaxRetryCount;
		private bool IsReportDownloadRetryCountExceeded(FeedSubmissionEntry e) => e.ReportDownloadRetryCount > _options.ReportDownloadMaxRetryCount;
		private bool IsFeedProcessingRetryCountExceeded(FeedSubmissionEntry e) => e.FeedProcessingRetryCount > _options.FeedProcessingMaxRetryCount;
		private bool IsCallbackInvocationRetryCountExceeded(FeedSubmissionEntry e) =>e.InvokeCallbackRetryCount > _options.InvokeCallbackMaxRetryCount;
		private void MarkEntriesAsDeleted(IFeedSubmissionEntryService feedSubmissionService, IQueryable<FeedSubmissionEntry> entriesToMarkAsDeleted, List<int> entriesIdsAlreadyMarkedAsDeleted, string deleteReason)
		{
			foreach (var entry in entriesToMarkAsDeleted)
			{
				if (entriesIdsAlreadyMarkedAsDeleted.Exists(e => e == entry.Id)) continue;
				entriesIdsAlreadyMarkedAsDeleted.Add(entry.Id);
				feedSubmissionService.Delete(entry);
				_logger.Warn($"Feed submission entry {entry.RegionAndTypeComputed} deleted from queue. {deleteReason}");
			}
		}
		private bool IsExpirationPeriodExceeded(FeedSubmissionEntry feedSubmissionEntry) =>
			(DateTime.Compare(feedSubmissionEntry.DateCreated, DateTime.UtcNow.Subtract(_options.FeedSubmissionRequestEntryExpirationPeriod)) < 0);
		private bool IsAmazonErrorCodeFatal(string errorCode)
		{
			var fatalErrorCodes = new List<string>
			{
				"AccessToFeedProcessingResultDenied",
				"FeedCanceled",
				"FeedProcessingResultNoLongerAvailable",
				"InputDataError",
				"InvalidFeedType",
				"InvalidRequest"
			};

			return fatalErrorCodes.Contains(errorCode);
		}
		private bool IsAmazonErrorCodeNonFatal(string errorCode)
		{
			var nonFatalErrorCodes = new List<string>
			{
				"ContentMD5Missing",
				"ContentMD5DoesNotMatch",
				"FeedProcessingResultNotReady",
				"InvalidFeedSubmissionId"
			};

			return nonFatalErrorCodes.Contains(errorCode) || !IsAmazonErrorCodeFatal(errorCode);
		}

	}
}
