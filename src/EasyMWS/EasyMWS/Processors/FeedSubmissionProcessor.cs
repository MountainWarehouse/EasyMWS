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

		internal FeedSubmissionProcessor(AmazonRegion region, string merchantId, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
		}

		

		public string SubmitFeedToAmazon(FeedSubmissionEntry feedSubmission)
		{
			var missingInformationExceptionMessage = "Cannot submit queued feed to amazon due to missing feed submission information";

			if (feedSubmission?.FeedSubmissionData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Feed submission data is null.");

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

					var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
					var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
					_logger.Info($"Request to MWS.SubmitFeed was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']",
						new RequestInfo(timestamp, requestId));

					return response?.SubmitFeedResult?.FeedSubmissionInfo?.FeedSubmissionId;
				}
				catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest)
				{
					stream.Dispose();
					_logger.Error($"Request to MWS.SubmitFeed failed! [HttpStatusCode:'{e.StatusCode}', ErrorType:'{e.ErrorType}', ErrorCode:'{e.ErrorCode}', Message: '{e.Message}']", e);
					return HttpStatusCode.BadRequest.ToString();
					
				}
				catch (MarketplaceWebServiceException e)
				{
					stream.Dispose();
					_logger.Error($"Request to MWS.SubmitFeed failed! [HttpStatusCode:'{e.StatusCode}', ErrorType:'{e.ErrorType}', ErrorCode:'{e.ErrorCode}', Message: '{e.Message}']", e);
					return null;
				}
				catch (Exception e)
				{
					stream.Dispose();
					_logger.Error($"Request to MWS.SubmitFeed failed! [Message: '{e.Message}']", e);
					return null;
				}
				finally
				{
					stream.Close();
				}
			}
		}

		public void MoveToQueueOfSubmittedFeeds(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission, string feedSubmissionId)
		{
			feedSubmission.FeedSubmissionId = feedSubmissionId;
			feedSubmission.FeedSubmissionRetryCount = 0;
			feedSubmissionService.Update(feedSubmission);
			feedSubmissionService.SaveChanges();

			_logger.Info($"Moving {feedSubmission.RegionAndTypeComputed} to queue of feed submissions that await processing results.");
		}

		public void MoveToRetryQueue(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry feedSubmission)
		{
			feedSubmission.FeedSubmissionRetryCount++;
			feedSubmissionService.Update(feedSubmission);
			feedSubmissionService.SaveChanges();

			_logger.Warn($"Moving {feedSubmission.RegionAndTypeComputed} to retry queue. Retry count is now '{feedSubmission.FeedSubmissionRetryCount}'.");
		}

		public List<(string FeedSubmissionId, string FeedProcessingStatus)> RequestFeedSubmissionStatusesFromAmazon(
			IEnumerable<string> feedSubmissionIdList, string merchant)
		{
			_logger.Info($"Attempting to request feed submission statuses for all feeds in queue.");

			var request = new GetFeedSubmissionListRequest() {FeedSubmissionIdList = new IdList(), Merchant = merchant};
			request.FeedSubmissionIdList.Id.AddRange(feedSubmissionIdList);

			try
			{
				var response = _marketplaceWebServiceClient.GetFeedSubmissionList(request);

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info(
					$"Request to MWS.GetFeedSubmissionList was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']",
					new RequestInfo(timestamp, requestId));

				var responseInfo = new List<(string FeedSubmissionId, string IsProcessingComplete)>();

				foreach (var feedSubmissionInfo in response.GetFeedSubmissionListResult.FeedSubmissionInfo)
				{
					responseInfo.Add((feedSubmissionInfo.FeedSubmissionId, feedSubmissionInfo.FeedProcessingStatus));
				}

				_logger.Info($"AmazonMWS request for feed submission statuses succeeded.");
				return responseInfo;
			}
			catch (MarketplaceWebServiceException e)
			{
				_logger.Error($"Request to MWS.GetFeedSubmissionList failed! [Message: '{e.Message}', HttpStatusCode:'{e.StatusCode}', ErrorType:'{e.ErrorType}', ErrorCode:'{e.ErrorCode}']", e);
				return null;
			}
			catch (Exception e)
			{
				_logger.Error($"Request to MWS.GetFeedSubmissionList failed! [Message: '{e.Message}']", e);
				return null;
			}
		}

		public void QueueFeedsAccordingToProcessingStatus(IFeedSubmissionCallbackService feedSubmissionService, List<(string FeedSubmissionId, string FeedProcessingStatus)> feedProcessingStatuses)
		{
			foreach (var feedSubmissionInfo in feedProcessingStatuses)
			{
				var feedSubmissionCallback = feedSubmissionService.FirstOrDefault(fsc => fsc.FeedSubmissionId == feedSubmissionInfo.FeedSubmissionId);
				if(feedSubmissionCallback == null) continue;

				if (feedSubmissionInfo.FeedProcessingStatus == "_DONE_")
				{
					feedSubmissionCallback.IsProcessingComplete = true;
					feedSubmissionCallback.FeedSubmissionRetryCount = 0;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_AWAITING_ASYNCHRONOUS_REPLY_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_IN_PROGRESS_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_IN_SAFETY_NET_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_SUBMITTED_"
				           || feedSubmissionInfo.FeedProcessingStatus == "_UNCONFIRMED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.FeedSubmissionRetryCount = 0;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
				else if (feedSubmissionInfo.FeedProcessingStatus == "_CANCELLED_")
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.FeedSubmissionRetryCount++;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
				else
				{
					feedSubmissionCallback.IsProcessingComplete = false;
					feedSubmissionCallback.FeedSubmissionRetryCount++;
					feedSubmissionService.Update(feedSubmissionCallback);
				}
			}

			feedSubmissionService.SaveChanges();
		}

		public (MemoryStream processingReport, string md5hash) GetFeedSubmissionResultFromAmazon(FeedSubmissionEntry feedSubmissionEntry)
		{
			_logger.Info(
				$"Attempting to request the feed submission result for the next feed in queue from Amazon: {feedSubmissionEntry.RegionAndTypeComputed}.");

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

				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
				var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
				_logger.Info(
					$"Request to MWS.GetFeedSubmissionResult was successful! [RequestId:'{requestId}',Timestamp:'{timestamp}']",
					new RequestInfo(timestamp, requestId));
				_logger.Info(
					$"Feed submission result request from Amazon has succeeded for {feedSubmissionEntry.RegionAndTypeComputed}.");

				return (reportResultStream, response?.GetFeedSubmissionResultResult?.ContentMD5);
			}
			catch (MarketplaceWebServiceException e)
			{
				_logger.Error($"Request to MWS.GetFeedSubmissionResult failed! [Message: '{e.Message}', HttpStatusCode:'{e.StatusCode}', ErrorType:'{e.ErrorType}', ErrorCode:'{e.ErrorCode}']", e);
				return (null, null);
			}
			catch (Exception e)
			{
				_logger.Error($"Request to MWS.GetFeedSubmissionResult failed! [Message: '{e.Message}']", e);
				return (null, null);
			}
		}

		public void RemoveFromQueue(IFeedSubmissionCallbackService feedSubmissionService, FeedSubmissionEntry entry)
		{
			feedSubmissionService.Delete(entry);
			feedSubmissionService.SaveChanges();
		}

		public void CleanUpFeedSubmissionQueue(IFeedSubmissionCallbackService feedSubmissionService)
		{
			_logger.Info("Executing cleanup of feed submission requests queue.");
			var expiredFeedSubmissions = feedSubmissionService.GetAll()
				.Where(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
							   && fscs.FeedSubmissionId == null
				               && fscs.FeedSubmissionRetryCount > _options.FeedSubmissionMaxRetryCount);

			foreach (var feedSubmission in expiredFeedSubmissions)
			{
				feedSubmissionService.Delete(feedSubmission);
				_logger.Warn($"Feed submission entry {feedSubmission.RegionAndTypeComputed} deleted from queue. Reason: A feedSubmissionId could not be obtained from amazon for the feed submission request. Retry count exceeded : FeedSubmissionMaxRetryCount={_options.FeedSubmissionMaxRetryCount}.");
			}

			var expiredFeedProcessingResultRequestIds = feedSubmissionService.GetAll()
				.Where(fscs => fscs.AmazonRegion == _region && fscs.MerchantId == _merchantId
							   && fscs.FeedSubmissionId != null
				               && fscs.FeedSubmissionRetryCount > _options.ReportDownloadMaxRetryCount);

			foreach (var feedSubmission in expiredFeedProcessingResultRequestIds)
			{
				feedSubmissionService.Delete(feedSubmission);
				_logger.Warn($"Feed submission entry {feedSubmission.RegionAndTypeComputed} deleted from queue. Reason: While the feed might have been submitted to amazon, the checksum verification failed for the Feed Submission Report content received from Amazon. Retry count exceeded : ReportDownloadMaxRetryCount={_options.ReportDownloadMaxRetryCount}.");
			}

			var entriesWithExpirationPeriodExceeded = feedSubmissionService.GetAll()
				.Where(fse => fse.AmazonRegion == _region && fse.MerchantId == _merchantId && IsExpirationPeriodExceeded(fse));

			foreach (var feedSubmission in entriesWithExpirationPeriodExceeded)
			{
				feedSubmissionService.Delete(feedSubmission);
				_logger.Warn($"Feed submission entry {feedSubmission.RegionAndTypeComputed} deleted from queue. Reason: Expiration period of FeedSubmissionRequestEntryExpirationPeriod='{_options.FeedSubmissionRequestEntryExpirationPeriod.Hours} hours' was exceeded.");
			}

			var entriesWithCallbackInvocationRetryCountExceeded = feedSubmissionService.GetAll()
				.Where(fse => (fse.AmazonRegion == _region && fse.MerchantId == _merchantId) && fse.Details != null && fse.Details.FeedSubmissionReport != null && IsFeedSubmissionEntryCallbackInvocationRetryCountExceeded(fse));

			foreach (var expiredSubmission in entriesWithCallbackInvocationRetryCountExceeded)
			{
				feedSubmissionService.Delete(expiredSubmission);
				_logger.Warn($"Feed submission entry {expiredSubmission.RegionAndTypeComputed} deleted from queue. Reason: The feed submission report was downloaded successfully but the callback method provided at QueueFeed could not be invoked. Retry count exceeded : FeedSubmissionRequestEntryExpirationPeriod={_options.FeedSubmissionRequestEntryExpirationPeriod}");
			}

			feedSubmissionService.SaveChanges();
		}

		private bool IsFeedSubmissionEntryCallbackInvocationRetryCountExceeded(FeedSubmissionEntry feedSubmissionEntry) =>
			(feedSubmissionEntry.FeedSubmissionRetryCount > _options.InvokeCallbackMaxRetryCount);

		private bool IsExpirationPeriodExceeded(FeedSubmissionEntry feedSubmissionEntry) =>
			(DateTime.Compare(feedSubmissionEntry.DateCreated, DateTime.UtcNow.Subtract(_options.FeedSubmissionRequestEntryExpirationPeriod)) < 0);

		
	}
}
