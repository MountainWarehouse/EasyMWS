using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class FeedSubmissionProcessor : IFeedSubmissionProcessor
	{
		private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
		private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
		private readonly AmazonRegion _region;
		private readonly string _merchantId;
        private readonly string _mWSAuthToken;

		public event EventHandler<FeedRequestFailedEventArgs> FeedEntryWasMarkedForDelete;

		private readonly Dictionary<string, string> PendingStatusCodesAndMessages = new Dictionary<string, string>()
		{
			{AmazonFeedProcessingStatus.AwaitingAsyncReply, "The request is being processed, but is waiting for external information before it can complete."},
			{AmazonFeedProcessingStatus.InProgress, "The request is being processed."},
			{AmazonFeedProcessingStatus.InSafetyNet, "The request is being processed, but the system has determined that there is a potential error with the feed (for example, the request will remove all inventory from a seller's account.) An Amazon seller support associate will contact the seller to confirm whether the feed should be processed."},
			{AmazonFeedProcessingStatus.Submitted, "The request has been received, but has not yet started processing."},
			{AmazonFeedProcessingStatus.Unconfirmed, "The request is pending."}
		};

		internal FeedSubmissionProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
            _mWSAuthToken = mWSAuthToken;
		}

		private void OnFeedEntryWasMarkedForDelete(FeedRequestFailedEventArgs e) => FeedEntryWasMarkedForDelete?.Invoke(this, e);

		public void SubmitFeedToAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmission)
		{
			void HandleSubmitFeedSuccess(SubmitFeedResponse response)
			{
				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				feedSubmission.FeedSubmissionId = response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId;
				feedSubmission.FeedSubmissionRetryCount = 0;
				_logger.Info($"AmazonMWS feed submission has succeeded for {feedSubmission.EntryIdentityDescription}. FeedSubmissionId:'{feedSubmission.FeedSubmissionId}'.",
					new RequestInfo(timestamp, requestId));
			}
			void HandleMissingFeedSubmissionId()
			{
				feedSubmission.FeedSubmissionRetryCount++;
				_logger.Warn($"SubmitFeed did not generate a FeedSubmissionId for {feedSubmission.EntryIdentityDescription}. Feed submission will be retried. FeedSubmissionRetryCount is now : {feedSubmission.FeedSubmissionRetryCount}.");
			}
			void HandleNonFatalOrGenericException(Exception e)
			{
				feedSubmission.FeedSubmissionRetryCount++;
				feedSubmission.LastSubmitted = DateTime.UtcNow;

				_logger.Warn($"AmazonMWS SubmitFeed failed for {feedSubmission.EntryIdentityDescription}. Feed submission will be retried. FeedSubmissionRetryCount is now : {feedSubmission.FeedSubmissionRetryCount}.", e);
			}
			void HandleFatalException(Exception e)
			{
				feedSubmissionService.Delete(feedSubmission);
				_logger.Error($"AmazonMWS SubmitFeed failed for {feedSubmission.EntryIdentityDescription}. The entry will now be removed from queue", e);
			}

			var missingInformationExceptionMessage = "Cannot submit queued feed to amazon due to missing feed submission information";

			if (feedSubmission == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission entry is null");
			if (feedSubmission.FeedSubmissionData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission data is null");

			var feedContentZip = feedSubmission.Details?.FeedContent;
			if (feedContentZip == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed content is missing");

			if (string.IsNullOrEmpty(feedSubmission?.FeedType)) throw new ArgumentException($"{missingInformationExceptionMessage}: Feed type is missing");

			_logger.Debug($"Attempting to submit the next feed in queue to Amazon: {feedSubmission.EntryIdentityDescription}.");

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
					ContentMD5 = MD5ChecksumHelper.ComputeHashForAmazon(stream),
                };

                if (!string.IsNullOrEmpty(_mWSAuthToken)) submitFeedRequest.MWSAuthToken = _mWSAuthToken;

                try
				{
					feedSubmissionService.Unlock(feedSubmission, "Unlocking single feed submission entry - finished attempt to submit feed to amazon.");
					feedSubmissionService.Update(feedSubmission);

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

		public void UnlockFeedSubmissionEntries(IFeedSubmissionEntryService feedSubmissionService, IEnumerable<string> feedSubmissionIds)
		{
			foreach (var submissionId in feedSubmissionIds)
			{
				var feedSubmissionEntry = feedSubmissionService.FirstOrDefault(fsc => fsc.FeedSubmissionId == submissionId);
				if (feedSubmissionEntry != null)
				{
					feedSubmissionService.Unlock(feedSubmissionEntry, "Unlocking multiple feed submission entries - amazon processing status update has been completed.");
					feedSubmissionService.Update(feedSubmissionEntry);
				}
			}
			feedSubmissionService.SaveChanges();
		}

		public List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(IFeedSubmissionEntryService feedSubmissionService, IEnumerable<string> feedSubmissionIdList, string merchant)
		{
			List<(string FeedSubmissionId, string IsProcessingComplete)> GetProcessingStatusesPerSubmissionIdFromResponse(GetFeedSubmissionListResponse response)
			{
				var responseInfo = new List<(string FeedSubmissionId, string IsProcessingComplete)>();
				foreach (var feedSubmissionInfo in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
				{
					responseInfo.Add((feedSubmissionInfo.FeedSubmissionId, feedSubmissionInfo.FeedProcessingStatus));
				}
				return responseInfo;
			}

			_logger.Debug($"Attempting to request feed submission statuses for all feeds in queue.");

			var request = new GetFeedSubmissionListRequest() { FeedSubmissionIdList = new IdList(), Merchant = merchant };
			request.FeedSubmissionIdList.Id.AddRange(feedSubmissionIdList);

            if (!string.IsNullOrEmpty(_mWSAuthToken)) request.MWSAuthToken = _mWSAuthToken;

            try
			{
				var response = _marketplaceWebServiceClient.GetFeedSubmissionList(request);

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Debug($"AmazonMWS request for feed submission statuses succeeded.", new RequestInfo(timestamp, requestId));

				if (response?.GetFeedSubmissionListResult?.FeedSubmissionInfo != null)
				{
					return GetProcessingStatusesPerSubmissionIdFromResponse(response);
				}
				else
				{
					_logger.Warn("AmazonMWS GetFeedSubmissionList response does not contain any results. The operation will be executed again at the next poll request.");
					return null;
				}
			}
			catch (MarketplaceWebServiceException e)
			{
				_logger.Warn($"AmazonMWS GetFeedSubmissionList failed! The operation will be executed again at the next poll request.", e);
				return null;
			}
			catch (Exception e)
			{
				_logger.Warn($"AmazonMWS GetFeedSubmissionList failed! The operation will be executed again at the next poll request.", e);
				return null;
			}
		}

		public void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionEntryService feedSubmissionService, List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses)
		{
			if(feedProcessingStatuses == null || !feedProcessingStatuses.Any()) return;

			foreach (var feedSubmissionInfo in feedProcessingStatuses)
			{
				var feedSubmissionEntry = feedSubmissionService.FirstOrDefault(fsc => fsc.FeedSubmissionId == feedSubmissionInfo.FeedSubmissionId);
                if (feedSubmissionEntry == null) continue;

				feedSubmissionEntry.LastAmazonFeedProcessingStatus = feedSubmissionInfo.FeedProcessingStatus;

                var genericProcessingInfo = $"ProcessingStatus returned by Amazon for {feedSubmissionEntry.EntryIdentityDescription} is '{feedSubmissionInfo.FeedProcessingStatus}'.";

				if (feedSubmissionInfo.FeedProcessingStatus == AmazonFeedProcessingStatus.Done)
				{
					feedSubmissionEntry.IsProcessingComplete = true;
					feedSubmissionEntry.FeedProcessingRetryCount = 0;
					_logger.Debug($"{genericProcessingInfo}. The request has been processed. The feed processing report download is ready to be attempted.");
				}
				else if (PendingStatusCodesAndMessages.Keys.Any(pendingStatus => pendingStatus == feedSubmissionInfo.FeedProcessingStatus))
				{
					var specificProcessingInfo = PendingStatusCodesAndMessages[feedSubmissionInfo.FeedProcessingStatus];
					feedSubmissionEntry.FeedProcessingRetryCount = 0;
					_logger.Debug($"{genericProcessingInfo} {specificProcessingInfo} The feed processing status will be checked again at the next poll request.");
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == AmazonFeedProcessingStatus.Cancelled)
				{
					feedSubmissionEntry.FeedProcessingRetryCount++;
					feedSubmissionEntry.FeedSubmissionId = null;
					_logger.Debug($"{genericProcessingInfo}. The request has been aborted due to a fatal error. The feed submission operation will be retried. FeedProcessingRetryCount is now '{feedSubmissionEntry.FeedProcessingRetryCount}'.");
				}
				else
				{
					feedSubmissionEntry.FeedProcessingRetryCount++;
					_logger.Debug($"{genericProcessingInfo}. The feed submission operation will be retried. This feed processing status is not yet handled by EasyMws. FeedProcessingRetryCount is now '{feedSubmissionEntry.FeedProcessingRetryCount}'.");
				}
				feedSubmissionService.Update(feedSubmissionEntry);
			}

			feedSubmissionService.SaveChanges();
		}

		public void DownloadFeedSubmissionResultFromAmazon(IFeedSubmissionEntryService feedSubmissionService, FeedSubmissionEntry feedSubmissionEntry)
		{
			var missingInformationExceptionMessage = "Cannot download report from amazon due to missing report request information";

			if (feedSubmissionEntry == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission entry is null.");
			if (string.IsNullOrEmpty(feedSubmissionEntry.FeedSubmissionId)) throw new ArgumentException($"{missingInformationExceptionMessage}: FeedSubmissionId is missing");

			_logger.Debug($"Attempting to request the feed submission result for the next feed in queue from Amazon: {feedSubmissionEntry.EntryIdentityDescription}.");

			var reportResultStream = new MemoryStream();
			var request = new GetFeedSubmissionResultRequest
			{
				FeedSubmissionId = feedSubmissionEntry.FeedSubmissionId,
				Merchant = feedSubmissionEntry.MerchantId,
				FeedSubmissionResult = reportResultStream,
            };

            if (!string.IsNullOrEmpty(_mWSAuthToken)) request.MWSAuthToken = _mWSAuthToken;

            try
			{
				feedSubmissionService.Unlock(feedSubmissionEntry, "Unlocking single feed submission entry - attempt to download feed processing report from amazon has been completed.");
				feedSubmissionService.Update(feedSubmissionEntry);

				var response = _marketplaceWebServiceClient.GetFeedSubmissionResult(request);
				feedSubmissionEntry.LastSubmitted = DateTime.UtcNow;

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info($"Feed submission report download from Amazon has succeeded for {feedSubmissionEntry.EntryIdentityDescription}.",
					new RequestInfo(timestamp, requestId));

				var md5Hash = response?.GetFeedSubmissionResultResult?.ContentMD5;
				var hasValidHash = MD5ChecksumHelper.IsChecksumCorrect(reportResultStream, md5Hash);
				if (hasValidHash)
				{
					_logger.Debug($"Checksum verification succeeded for feed submission report for {feedSubmissionEntry.EntryIdentityDescription}");
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
					_logger.Warn($"Checksum verification failed for feed submission report for {feedSubmissionEntry.EntryIdentityDescription}");
				}
			}
			catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
			{
				feedSubmissionService.Delete(feedSubmissionEntry);
				_logger.Error($"AmazonMWS feed submission report download failed for {feedSubmissionEntry.EntryIdentityDescription}! The entry will now be removed from queue", e);
			}
			catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
			{
				feedSubmissionEntry.ReportDownloadRetryCount++;
				feedSubmissionEntry.LastSubmitted = DateTime.UtcNow;
				_logger.Warn($"AmazonMWS feed submission report download failed for {feedSubmissionEntry.EntryIdentityDescription}! Report download will be retried. ReportDownloadRetryCount is now : {feedSubmissionEntry.ReportDownloadRetryCount}.", e);
			}
			catch (Exception e)
			{
				feedSubmissionEntry.ReportDownloadRetryCount++;
				feedSubmissionEntry.LastSubmitted = DateTime.UtcNow;
				_logger.Warn($"AmazonMWS feed submission report download failed for {feedSubmissionEntry.EntryIdentityDescription}! Report download will be retried. ReportDownloadRetryCount is now : {feedSubmissionEntry.ReportDownloadRetryCount}.", e);
			}
			finally
			{
				feedSubmissionService.SaveChanges();
			}
		}

		public void CleanUpFeedSubmissionQueue(IFeedSubmissionEntryService feedSubmissionService)
		{
			_logger.Debug("Executing cleanup of feed submission requests queue.");
			var allEntriesForRegionAndMerchant = feedSubmissionService.GetAll().Where(fse => IsMatchForRegionAndMerchantId(fse));
			var entriesToDelete = new List<EntryToDelete>();

			void DeleteUniqueEntries(IEnumerable<EntryToDelete> entries)
			{
				foreach (var entryToDelete in entries)
				{
					if (!feedSubmissionService.GetAll().Any(fse => fse.Id == entryToDelete.Entry.Id)) continue;
					feedSubmissionService.Delete(entryToDelete.Entry);
					feedSubmissionService.SaveChanges();

					_logger.Warn($"Feed submission entry {entryToDelete.Entry.EntryIdentityDescription} deleted from queue. {entryToDelete.DeleteReason.ToString()} exceeded");
				}
			}

			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsFeedSubmissionRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = FeedRequestFailureReasonType.FeedSubmissionMaxRetryCountExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsReportDownloadRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = FeedRequestFailureReasonType.ProcessingReportDownloadMaxRetryCountExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsExpirationPeriodExceeded)
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = FeedRequestFailureReasonType.FeedSubmissionEntryExpirationPeriodExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsFeedProcessingRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = FeedRequestFailureReasonType.FeedProcessingMaxRetryCountExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsCallbackInvocationRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = FeedRequestFailureReasonType.InvokeCallbackMaxRetryCountExceeded }));

			foreach (var entryToDelete in entriesToDelete)
			{
				using (var feedStream = ZipHelper.ExtractArchivedSingleFileToStream(entryToDelete.Entry.Details?.FeedContent))
				using (var streamReader = feedStream == null ? null : new StreamReader(feedStream))
				{
					OnFeedEntryWasMarkedForDelete(new FeedRequestFailedEventArgs(
							entryToDelete.DeleteReason,
							entryToDelete.Entry.AmazonRegion,
							entryToDelete.Entry.LastSubmitted,
							entryToDelete.Entry.LastAmazonFeedProcessingStatus,
							entryToDelete.Entry.FeedSubmissionId,
							entryToDelete.Entry.GetPropertiesContainer(),
							entryToDelete.Entry.TargetHandlerId,
							entryToDelete.Entry.TargetHandlerArgs,
							streamReader?.ReadToEnd(),
							entryToDelete.Entry.FeedType));
				}
			}

			DeleteUniqueEntries(entriesToDelete.Distinct());
		}

		private bool IsMatchForRegionAndMerchantId(FeedSubmissionEntry e) => e.AmazonRegion == _region && e.MerchantId == _merchantId;
		private bool IsFeedSubmissionRetryCountExceeded(FeedSubmissionEntry e) => e.FeedSubmissionRetryCount > _options.FeedSubmissionOptions.FeedSubmissionMaxRetryCount;
		private bool IsReportDownloadRetryCountExceeded(FeedSubmissionEntry e) => e.ReportDownloadRetryCount > _options.ReportRequestOptions.ReportDownloadMaxRetryCount;
		private bool IsFeedProcessingRetryCountExceeded(FeedSubmissionEntry e) => e.FeedProcessingRetryCount > _options.FeedSubmissionOptions.FeedProcessingMaxRetryCount;
		private bool IsCallbackInvocationRetryCountExceeded(FeedSubmissionEntry e) =>e.InvokeCallbackRetryCount > _options.EventPublishingOptions.EventPublishingMaxRetryCount;

		private bool IsExpirationPeriodExceeded(FeedSubmissionEntry feedSubmissionEntry) =>
			(DateTime.Compare(feedSubmissionEntry.DateCreated, DateTime.UtcNow.Subtract(_options.FeedSubmissionOptions.FeedSubmissionRequestEntryExpirationPeriod)) < 0);
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

		 private class EntryToDelete : IEquatable<EntryToDelete>
	    {
		    public FeedSubmissionEntry Entry { get; set; }
		    public FeedRequestFailureReasonType DeleteReason { get; set; }

		    public bool Equals(EntryToDelete other)
		    {
			    return other != null && this.Entry.Id == other.Entry.Id;
		    }

		    public override int GetHashCode()
		    {
			    unchecked
			    {
				    return Entry.Id;
			    }
		    }
	    }
	}
}
