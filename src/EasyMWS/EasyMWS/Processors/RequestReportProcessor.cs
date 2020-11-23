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
    internal class RequestReportProcessor : IRequestReportProcessor
    {
	    private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
	    private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
	    private readonly AmazonRegion _region;
	    private readonly string _merchantId;
        private readonly string _mWSAuthToken;

		public event EventHandler<ReportRequestFailedEventArgs> ReportEntryWasMarkedForDelete;

		internal RequestReportProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
	    {
		    _region = region;
		    _merchantId = merchantId;
		    _options = options;
		    _logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
            _mWSAuthToken = mWSAuthToken;
	    }

		private void OnReportEntryWasMarkedForDelete(ReportRequestFailedEventArgs e) => ReportEntryWasMarkedForDelete?.Invoke(null, e);

		public void RequestReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry)
	    {
			var missingInformationExceptionMessage = "Cannot request report from amazon due to missing report request information";

			if (reportRequestEntry?.ReportRequestData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Report request data is missing");
		    if (string.IsNullOrEmpty(reportRequestEntry?.ReportType)) throw new ArgumentException($"{missingInformationExceptionMessage}: Report Type is missing");

			var reportRequestData = reportRequestEntry.GetPropertiesContainer();

			_logger.Debug($"Attempting to request the next report in queue from Amazon: {reportRequestEntry.EntryIdentityDescription}.");

			var reportRequest = new RequestReportRequest
			{
				Merchant = reportRequestEntry.MerchantId,
				ReportType = reportRequestEntry.ReportType,
            };

            if (!string.IsNullOrEmpty(_mWSAuthToken)) reportRequest.MWSAuthToken = _mWSAuthToken;

            if (reportRequestData.MarketplaceIdList != null) reportRequest.MarketplaceIdList = new IdList {Id = reportRequestData.MarketplaceIdList.ToList()};
			if (reportRequestData.StartDate.HasValue) reportRequest.StartDate = reportRequestData.StartDate.Value;
		    if (reportRequestData.EndDate.HasValue) reportRequest.EndDate = reportRequestData.EndDate.Value;
		    if (!string.IsNullOrEmpty(reportRequestData.ReportOptions)) reportRequest.ReportOptions = reportRequestData.ReportOptions;

		    try
		    {
				reportRequestService.Unlock(reportRequestEntry, "Unlocking single report request entry - attempt to request report from amazon has been completed.");
				reportRequestService.Update(reportRequestEntry);

				var reportResponse = _marketplaceWebServiceClient.RequestReport(reportRequest);
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
			    reportRequestEntry.RequestReportId = reportResponse?.RequestReportResult?.ReportRequestInfo?.ReportRequestId;

				var requestId = reportResponse?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			    var timestamp = reportResponse?.ResponseHeaderMetadata?.Timestamp ?? "unknown";

				if (string.IsNullOrEmpty(reportRequestEntry.RequestReportId))
			    {
					reportRequestEntry.ReportRequestRetryCount++;
					_logger.Warn($"RequestReport did not generate a ReportRequestId for {reportRequestEntry.EntryIdentityDescription}. Report request will be retried. ReportRequestRetryCount is now : {reportRequestEntry.ReportRequestRetryCount}.",
						new RequestInfo(timestamp, requestId));
				}
			    else
			    {
				    reportRequestEntry.ReportRequestRetryCount = 0;
					_logger.Info($"AmazonMWS RequestReport succeeded for {reportRequestEntry.EntryIdentityDescription}. ReportRequestId:'{reportRequestEntry.RequestReportId}'.",
						new RequestInfo(timestamp, requestId));
				}
			}
		    catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
		    {
			    reportRequestService.Delete(reportRequestEntry);
				_logger.Error($"AmazonMWS RequestReport failed for {reportRequestEntry.EntryIdentityDescription}. The entry will now be removed from queue", e);
			}
		    catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
		    {
				reportRequestEntry.ReportRequestRetryCount++;
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
				_logger.Warn($"AmazonMWS RequestReport failed for {reportRequestEntry.EntryIdentityDescription}. Report request will be retried. ReportRequestRetryCount is now : {reportRequestEntry.ReportRequestRetryCount}.", e);
		    }
		    catch (Exception e)
		    {

				reportRequestEntry.ReportRequestRetryCount++;
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
				_logger.Warn($"AmazonMWS RequestReport failed for {reportRequestEntry.EntryIdentityDescription}. Report request will be retried. ReportRequestRetryCount is now : {reportRequestEntry.ReportRequestRetryCount}.", e);
			}
		    finally
			{
				reportRequestService.SaveChanges();
			}
	    }

		public void UnlockReportRequestEntries(IReportRequestEntryService reportRequestService, IEnumerable<string> reportRequestIds)
		{
			foreach (var reportRequestId in reportRequestIds)
			{
				var reportRequestEntry = reportRequestService.FirstOrDefault(fsc => fsc.RequestReportId == reportRequestId);
				if (reportRequestEntry != null)
				{
					reportRequestService.Unlock(reportRequestEntry, "Unlocking multiple report request entries - amazon processing status update has been completed.");
					reportRequestService.Update(reportRequestEntry);
				}
			}
			reportRequestService.SaveChanges();
		}

		public List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(
			IReportRequestEntryService reportRequestService, IEnumerable<string> requestIdList, string merchant)
	    {
			

			_logger.Debug($"Attempting to request report processing statuses for all reports in queue.");

		    var request = new GetReportRequestListRequest() { ReportRequestIdList = new IdList(), Merchant = merchant };
		    request.ReportRequestIdList.Id.AddRange(requestIdList);

            if (!string.IsNullOrEmpty(_mWSAuthToken)) request.MWSAuthToken = _mWSAuthToken;

            try
		    {
			    var response = _marketplaceWebServiceClient.GetReportRequestList(request);
			    var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			    var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			    _logger.Debug($"Request to MWS.GetReportRequestList was successful!", new RequestInfo(timestamp, requestId));

			    var responseInformation =
				    new List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)>();

			    if (response?.GetReportRequestListResult?.ReportRequestInfo != null)
			    {
				    foreach (var reportRequestInfo in response.GetReportRequestListResult.ReportRequestInfo)
				    {
					    responseInformation.Add((reportRequestInfo.ReportRequestId, reportRequestInfo.GeneratedReportId, reportRequestInfo.ReportProcessingStatus));
				    }
			    }
			    else
			    {
					_logger.Warn("AmazonMWS GetReportRequestList response does not contain any results. The operation will be executed again at the next poll request.");
				}

			    return responseInformation;

		    }
		    catch (MarketplaceWebServiceException e)
		    {
				_logger.Warn($"AmazonMWS GetReportRequestList failed! The operation will be executed again at the next poll request.", e);
				return null;
			}
			catch (Exception e)
		    {
				_logger.Warn($"AmazonMWS GetReportRequestList failed! The operation will be executed again at the next poll request.", e);
				return null;
			}
	    }

		public void CleanupReportRequests(IReportRequestEntryService reportRequestService)
		{
			_logger.Debug("Executing cleanup of report requests queue.");
			var allEntriesForRegionAndMerchant = reportRequestService.GetAll().Where(rrc => IsMatchForRegionAndMerchantId(rrc));
			var entriesToDelete = new List<EntryToDelete>();

			void DeleteUniqueEntries(IEnumerable<EntryToDelete> entries)
			{
				foreach (var entryToDelete in entries)
				{
					if (!reportRequestService.GetAll().Any(rrc => rrc.Id == entryToDelete.Entry.Id)) continue;
					reportRequestService.Delete(entryToDelete.Entry);
					reportRequestService.SaveChanges();

					_logger.Warn($"Report request entry {entryToDelete.Entry.EntryIdentityDescription} deleted from queue. {entryToDelete.ReportRequestEntryDeleteReason.ToString()} exceeded");
				}
			}
			
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsRequestRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, ReportRequestEntryDeleteReason = ReportRequestFailureReasonType.ReportRequestMaxRetryCountExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsDownloadRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, ReportRequestEntryDeleteReason = ReportRequestFailureReasonType.ReportDownloadMaxRetryCountExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsProcessingRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, ReportRequestEntryDeleteReason = ReportRequestFailureReasonType.ReportProcessingMaxRetryCountExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsExpirationPeriodExceeded)
				.Select(e => new EntryToDelete { Entry = e, ReportRequestEntryDeleteReason = ReportRequestFailureReasonType.ReportRequestEntryExpirationPeriodExceeded }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(IsCallbackInvocationRetryCountExceeded)
				.Select(e => new EntryToDelete { Entry = e, ReportRequestEntryDeleteReason = ReportRequestFailureReasonType.InvokeCallbackMaxRetryCountExceeded }));

			foreach (var entryToDelete in entriesToDelete)
			{
				OnReportEntryWasMarkedForDelete(new ReportRequestFailedEventArgs(
					entryToDelete.ReportRequestEntryDeleteReason,
					entryToDelete.Entry.AmazonRegion,
					entryToDelete.Entry.LastAmazonRequestDate,
					entryToDelete.Entry.LastAmazonReportProcessingStatus,
					entryToDelete.Entry.RequestReportId,
					entryToDelete.Entry.GeneratedReportId,
					entryToDelete.Entry.GetPropertiesContainer(),
					entryToDelete.Entry.TargetHandlerId,
					entryToDelete.Entry.TargetHandlerArgs,
					entryToDelete.Entry.ReportType));
			}

			DeleteUniqueEntries(entriesToDelete.Distinct());
		}

		public void QueueReportsAccordingToProcessingStatus(IReportRequestEntryService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses)
	    {
			foreach (var reportGenerationInfo in reportGenerationStatuses)
			{
				var reportRequestEntry = reportRequestService.FirstOrDefault(rrc => rrc.RequestReportId == reportGenerationInfo.ReportRequestId && rrc.GeneratedReportId == null);
                if (reportRequestEntry == null) continue;

				reportRequestEntry.LastAmazonReportProcessingStatus = reportGenerationInfo.ReportProcessingStatus;

                var genericProcessingInfo = $"ProcessingStatus returned by Amazon for {reportRequestEntry.EntryIdentityDescription} is '{reportGenerationInfo.ReportProcessingStatus}'.";

				if (reportGenerationInfo.ReportProcessingStatus == AmazonReportProcessingStatus.Done)
				{
					reportRequestEntry.GeneratedReportId = reportGenerationInfo.GeneratedReportId;
					reportRequestEntry.ReportProcessRetryCount = 0;
					reportRequestService.Update(reportRequestEntry);
					_logger.Info($"The following report is now ready for download : {genericProcessingInfo}");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == AmazonReportProcessingStatus.DoneNoData)
				{
                    if (_options.EventPublishingOptions.EventPublishingForReportStatusDoneNoData)
                    {
                        reportRequestEntry.ReportProcessRetryCount = 0;
                        reportRequestService.Update(reportRequestEntry);
                        _logger.Warn($"{genericProcessingInfo}. The Report was successfully processed by Amazon, but it contains no data. An callback invocation will be done with a null stream argument.");
                    }
                    else
                    {
                        reportRequestService.Delete(reportRequestEntry);
                        _logger.Warn($"{genericProcessingInfo}. The Report request entry will now be removed from queue.");
                    }
				}
				else if (reportGenerationInfo.ReportProcessingStatus == AmazonReportProcessingStatus.Submitted 
					  || reportGenerationInfo.ReportProcessingStatus == AmazonReportProcessingStatus.InProgress)
				{
                    reportRequestService.Update(reportRequestEntry);
                    _logger.Debug($"{genericProcessingInfo}. The report processing status will be checked again at the next poll request.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == AmazonReportProcessingStatus.Cancelled)
				{
					reportRequestEntry.RequestReportId = null;
					reportRequestEntry.GeneratedReportId = null;
					reportRequestEntry.ReportProcessRetryCount++;
					reportRequestService.Update(reportRequestEntry);
					_logger.Warn($"{genericProcessingInfo}. The Report request will be retried. ReportProcessRetryCount is now '{reportRequestEntry.ReportProcessRetryCount}'.");
				}
				else
				{
					reportRequestEntry.RequestReportId = null;
					reportRequestEntry.GeneratedReportId = null;
					reportRequestEntry.ReportProcessRetryCount++;
					reportRequestService.Update(reportRequestEntry);
					_logger.Warn($"{genericProcessingInfo}. The Report request will be retried. This report processing status is not yet handled by EasyMws. ReportProcessRetryCount is now '{reportRequestEntry.ReportProcessRetryCount}'.");
				}
			}
			reportRequestService.SaveChanges();
	    }

		public void DownloadGeneratedReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry)
	    {
			var missingInformationExceptionMessage = "Cannot request report from amazon due to missing report request information";
			_logger.Debug($"Attempting to download the next report in queue from Amazon: {reportRequestEntry.EntryIdentityDescription}.");

			if (string.IsNullOrEmpty(reportRequestEntry?.GeneratedReportId)) throw new ArgumentException($"{missingInformationExceptionMessage}: GeneratedReportId is missing");

			var reportResultStream = new MemoryStream();
		    var getReportRequest = new GetReportRequest
		    {
			    ReportId = reportRequestEntry.GeneratedReportId,
			    Report = reportResultStream,
			    Merchant = reportRequestEntry.MerchantId,
            };

            if (!string.IsNullOrEmpty(_mWSAuthToken)) getReportRequest.MWSAuthToken = _mWSAuthToken;

            try
		    {
				reportRequestService.Unlock(reportRequestEntry, "Unlocking single report request entry - request to download report from amazon has been completed.");
				reportRequestService.Update(reportRequestEntry);

				var response = _marketplaceWebServiceClient.GetReport(getReportRequest);
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;

				
				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			    var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			    _logger.Info(
				    $"The following report was downloaded from Amazon : {reportRequestEntry.EntryIdentityDescription}.",
				    new RequestInfo(timestamp, requestId));
			    reportResultStream.Position = 0;

				var md5Hash = response?.GetReportResult?.ContentMD5;
				var hasValidHash = MD5ChecksumHelper.IsChecksumCorrect(reportResultStream, md5Hash);
			    if (hasValidHash)
			    {
				    _logger.Debug($"Checksum verification succeeded for report {reportRequestEntry.EntryIdentityDescription}");
				    reportRequestEntry.ReportDownloadRetryCount = 0;

					using (var streamReader = new StreamReader(reportResultStream))
				    {
					    var zippedReport = ZipHelper.CreateArchiveFromContent(streamReader.ReadToEnd());
					    reportRequestEntry.Details = new ReportRequestDetails { ReportContent = zippedReport };
				    }
			    }
			    else
			    {
				    reportRequestEntry.ReportDownloadRetryCount++;
					_logger.Warn($"Checksum verification failed for report {reportRequestEntry.EntryIdentityDescription}. Report download will be retried. ReportDownloadRetryCount is now : '{reportRequestEntry.ReportDownloadRetryCount}'.");
				}
			}
		    catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
		    {
			    reportRequestService.Delete(reportRequestEntry);
			    _logger.Error($"AmazonMWS report download failed for {reportRequestEntry.EntryIdentityDescription}! The entry will now be removed from queue", e);
			}
		    catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
			{
				reportRequestEntry.ReportDownloadRetryCount++;
				reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
				_logger.Warn($"AmazonMWS report download failed for {reportRequestEntry.EntryIdentityDescription} Report download will be retried. ReportDownloadRetryCount is now : '{reportRequestEntry.ReportDownloadRetryCount}'.", e);
			}
			catch (Exception e)
		    {
			    reportRequestEntry.ReportDownloadRetryCount++;
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
				_logger.Warn($"AmazonMWS report download failed for {reportRequestEntry.EntryIdentityDescription}!", e);
		    }
		    finally
		    {
				reportRequestService.SaveChanges();
		    }
		}


	    private bool IsAmazonErrorCodeFatal(string errorCode)
	    {
		    var fatalErrorCodes = new List<string>
		    {
			    "AccessToReportDenied",
			    "InvalidReportId",
			    "InvalidReportType",
			    "InvalidRequest",
			    "ReportNoLongerAvailable"
		    };

		    return fatalErrorCodes.Contains(errorCode);
	    }
	    private bool IsAmazonErrorCodeNonFatal(string errorCode)
	    {
		    var nonFatalErrorCodes = new List<string>
		    {
			    "ReportNotReady",
			    "InvalidScheduleFrequency"
		    };

		    return nonFatalErrorCodes.Contains(errorCode) || !IsAmazonErrorCodeFatal(errorCode);
	    }
	    private void MarkEntriesAsDeleted(IReportRequestEntryService reportRequestService, IQueryable<ReportRequestEntry> entriesToMarkAsDeleted, List<int> entriesIdsAlreadyMarkedAsDeleted, string deleteReason)
	    {
		    foreach (var entry in entriesToMarkAsDeleted)
		    {
			    if (entriesIdsAlreadyMarkedAsDeleted.Exists(e => e == entry.Id)) continue;
			    entriesIdsAlreadyMarkedAsDeleted.Add(entry.Id);
			    reportRequestService.Delete(entry);
			    _logger.Warn($"Report request entry {entry.EntryIdentityDescription} deleted from queue. {deleteReason}");
		    }
	    }
	    private bool IsMatchForRegionAndMerchantId(ReportRequestEntry e) => e.AmazonRegion == _region && e.MerchantId == _merchantId;
	    private bool IsRequestRetryCountExceeded(ReportRequestEntry e) => e.ReportRequestRetryCount > _options.ReportRequestOptions.ReportRequestMaxRetryCount;
	    private bool IsDownloadRetryCountExceeded(ReportRequestEntry e) => e.ReportDownloadRetryCount > _options.ReportRequestOptions.ReportDownloadMaxRetryCount;
	    private bool IsProcessingRetryCountExceeded(ReportRequestEntry e) => e.ReportProcessRetryCount > _options.ReportRequestOptions.ReportProcessingMaxRetryCount;
	    private bool IsCallbackInvocationRetryCountExceeded(ReportRequestEntry e) => e.InvokeCallbackRetryCount > _options.EventPublishingOptions.EventPublishingMaxRetryCount;
	    private bool IsExpirationPeriodExceeded(ReportRequestEntry reportRequestEntry) =>
		    (DateTime.Compare(reportRequestEntry.DateCreated, DateTime.UtcNow.Subtract(_options.ReportRequestOptions.ReportDownloadRequestEntryExpirationPeriod)) < 0);

	    private class EntryToDelete : IEquatable<EntryToDelete>
	    {
		    public ReportRequestEntry Entry { get; set; }
		    public ReportRequestFailureReasonType ReportRequestEntryDeleteReason { get; set; }

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
