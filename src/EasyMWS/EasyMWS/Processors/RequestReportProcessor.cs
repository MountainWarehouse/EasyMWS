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
    internal class RequestReportProcessor : IRequestReportProcessor
    {
	    private readonly IMarketplaceWebServiceClient _marketplaceWebServiceClient;
	    private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;
	    private readonly AmazonRegion _region;
	    private readonly string _merchantId;

	    internal RequestReportProcessor(AmazonRegion region, string merchantId, IMarketplaceWebServiceClient marketplaceWebServiceClient, IEasyMwsLogger logger, EasyMwsOptions options)
	    {
		    _region = region;
		    _merchantId = merchantId;
		    _options = options;
		    _logger = logger;
			_marketplaceWebServiceClient = marketplaceWebServiceClient;
	    }


		public void RequestReportFromAmazon(IReportRequestEntryService reportRequestService, ReportRequestEntry reportRequestEntry)
	    {
			var missingInformationExceptionMessage = "Cannot request report from amazon due to missing report request information.";

			if (reportRequestEntry?.ReportRequestData == null) throw new ArgumentNullException($"{missingInformationExceptionMessage}: Report request data is missing.");
		    if (string.IsNullOrEmpty(reportRequestEntry?.ReportType)) throw new ArgumentException($"{missingInformationExceptionMessage}: Report Type is missing.");

			var reportRequestData = reportRequestEntry.GetPropertiesContainer();

			_logger.Info($"Attempting to request the next report in queue from Amazon: {reportRequestEntry.RegionAndTypeComputed}.");
			reportRequestEntry.IsLocked = false;

			var reportRequest = new RequestReportRequest
			{
				Merchant = reportRequestEntry.MerchantId,
				ReportType = reportRequestEntry.ReportType
			};

		    if (reportRequestData.MarketplaceIdList != null) reportRequest.MarketplaceIdList = new IdList {Id = reportRequestData.MarketplaceIdList.ToList()};
			if (reportRequestData.StartDate.HasValue) reportRequest.StartDate = reportRequestData.StartDate.Value;
		    if (reportRequestData.EndDate.HasValue) reportRequest.EndDate = reportRequestData.EndDate.Value;
		    if (!string.IsNullOrEmpty(reportRequestData.ReportOptions)) reportRequest.ReportOptions = reportRequestData.ReportOptions;

		    try
		    {
			    var reportResponse = _marketplaceWebServiceClient.RequestReport(reportRequest);
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
			    reportRequestEntry.RequestReportId = reportResponse?.RequestReportResult?.ReportRequestInfo?.ReportRequestId;

				var requestId = reportResponse?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			    var timestamp = reportResponse?.ResponseHeaderMetadata?.Timestamp ?? "unknown";

				if (string.IsNullOrEmpty(reportRequestEntry.RequestReportId))
			    {
					reportRequestEntry.ReportRequestRetryCount++;
					_logger.Warn($"RequestReport did not generate a ReportRequestId for {reportRequestEntry.RegionAndTypeComputed}. Report request will be retried. ReportRequestRetryCount is now : {reportRequestEntry.ReportRequestRetryCount}.",
						new RequestInfo(timestamp, requestId));
				}
			    else
			    {
				    reportRequestEntry.ReportRequestRetryCount = 0;
					_logger.Info($"AmazonMWS RequestReport succeeded for {reportRequestEntry.RegionAndTypeComputed}. ReportRequestId:'{reportRequestEntry.RequestReportId}'.",
						new RequestInfo(timestamp, requestId));
				}

			    reportRequestService.Update(reportRequestEntry);
			}
		    catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
		    {
			    reportRequestService.Delete(reportRequestEntry);
				_logger.Error($"AmazonMWS RequestReport failed for {reportRequestEntry.RegionAndTypeComputed}. The entry will now be removed from queue.", e);
			}
		    catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
		    {
			    reportRequestEntry.ReportRequestRetryCount++;
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
			    reportRequestService.Update(reportRequestEntry);
				_logger.Warn($"AmazonMWS RequestReport failed for {reportRequestEntry.RegionAndTypeComputed}. Report request will be retried. ReportRequestRetryCount is now : {reportRequestEntry.ReportRequestRetryCount}.");
		    }
		    catch (Exception e)
		    {
				reportRequestEntry.ReportRequestRetryCount++;
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
			    reportRequestService.Update(reportRequestEntry);
				_logger.Warn($"AmazonMWS RequestReport failed for {reportRequestEntry.RegionAndTypeComputed}. Report request will be retried. ReportRequestRetryCount is now : {reportRequestEntry.ReportRequestRetryCount}.");
			}
		    finally
			{
				reportRequestService.SaveChanges();
			}
	    }

	    public List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> GetReportProcessingStatusesFromAmazon(IEnumerable<string> requestIdList, string merchant)
	    {
		    _logger.Info($"Attempting to request report processing statuses for all reports in queue.");

		    var request = new GetReportRequestListRequest() {ReportRequestIdList = new IdList(), Merchant = merchant};
		    request.ReportRequestIdList.Id.AddRange(requestIdList);

		    try
		    {
			    var response = _marketplaceWebServiceClient.GetReportRequestList(request);
			    var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			    var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			    _logger.Info($"Request to MWS.GetReportRequestList was successful!", new RequestInfo(timestamp, requestId));

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
				_logger.Warn($"AmazonMWS GetReportRequestList failed! The operation will be executed again at the next poll request.");
			    return null;
			}
			catch (Exception e)
		    {
				_logger.Warn($"AmazonMWS GetReportRequestList failed! The operation will be executed again at the next poll request.");
			    return null;
			}
	    }

		public void CleanupReportRequests(IReportRequestEntryService reportRequestService)
		{
			_logger.Info("Executing cleanup of report requests queue.");
			var allEntriesForRegionAndMerchant = reportRequestService.GetAll().Where(rrc => IsMatchForRegionAndMerchantId(rrc));
			var entriesToDelete = new List<EntryToDelete>();

			void DeleteUniqueEntries(IEnumerable<EntryToDelete> entries)
			{
				foreach (var entryToDelete in entries)
				{
					if (!reportRequestService.GetAll().Any(rrc => rrc.Id == entryToDelete.Entry.Id)) continue;
					reportRequestService.Delete(entryToDelete.Entry);
					reportRequestService.SaveChanges();

					_logger.Warn($"Report request entry {entryToDelete.Entry.RegionAndTypeComputed} deleted from queue. {entryToDelete.DeleteReason.ToString()} exceeded");
				}
			}
			
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(rrc => IsRequestRetryCountExceeded(rrc))
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = DeleteReasonType.ReportRequestMaxRetryCount }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(rrc => IsDownloadRetryCountExceeded(rrc))
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = DeleteReasonType.ReportDownloadMaxRetryCount }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(rrc => IsProcessingRetryCountExceeded(rrc))
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = DeleteReasonType.ReportProcessingMaxRetryCount }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(rrc => IsExpirationPeriodExceeded(rrc))
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = DeleteReasonType.ReportDownloadRequestEntryExpirationPeriod }));
			entriesToDelete.AddRange(allEntriesForRegionAndMerchant.Where(rrc => IsCallbackInvocationRetryCountExceeded(rrc))
				.Select(e => new EntryToDelete { Entry = e, DeleteReason = DeleteReasonType.InvokeCallbackMaxRetryCount }));

			DeleteUniqueEntries(entriesToDelete.Distinct());
		}

		public void QueueReportsAccordingToProcessingStatus(IReportRequestEntryService reportRequestService,
			List<(string ReportRequestId, string GeneratedReportId, string ReportProcessingStatus)> reportGenerationStatuses)
	    {
			foreach (var reportGenerationInfo in reportGenerationStatuses)
			{
				var reportRequestEntry = reportRequestService.FirstOrDefault(rrc => rrc.RequestReportId == reportGenerationInfo.ReportRequestId && rrc.GeneratedReportId == null);
				if (reportRequestEntry == null) continue;
				reportRequestEntry.IsLocked = false;

				var genericProcessingInfo = $"ProcessingStatus returned by Amazon for {reportRequestEntry.RegionAndTypeComputed} is '{reportGenerationInfo.ReportProcessingStatus}'.";

				if (reportGenerationInfo.ReportProcessingStatus == "_DONE_")
				{
					reportRequestEntry.GeneratedReportId = reportGenerationInfo.GeneratedReportId;
					reportRequestEntry.ReportProcessRetryCount = 0;
					reportRequestService.Update(reportRequestEntry);
					_logger.Info($"{genericProcessingInfo}. The report is now ready for download.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_DONE_NO_DATA_")
				{
					reportRequestService.Delete(reportRequestEntry);
					_logger.Warn($"{genericProcessingInfo}. The Report request entry will now be removed from queue.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_SUBMITTED_" 
					  || reportGenerationInfo.ReportProcessingStatus == "_IN_PROGRESS_")
				{
					_logger.Info($"{genericProcessingInfo}. The report processing status will be checked again at the next poll request.");
				}
				else if (reportGenerationInfo.ReportProcessingStatus == "_CANCELLED_")
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
			var missingInformationExceptionMessage = "Cannot request report from amazon due to missing report request information.";
			_logger.Info($"Attempting to download the next report in queue from Amazon: {reportRequestEntry.RegionAndTypeComputed}.");
			reportRequestEntry.IsLocked = false;

			if (string.IsNullOrEmpty(reportRequestEntry?.GeneratedReportId)) throw new ArgumentException($"{missingInformationExceptionMessage}: GeneratedReportId is missing.");

			var reportResultStream = new MemoryStream();
		    var getReportRequest = new GetReportRequest
		    {
			    ReportId = reportRequestEntry.GeneratedReportId,
			    Report = reportResultStream,
			    Merchant = reportRequestEntry.MerchantId
		    };

		    try
		    {
			    var response = _marketplaceWebServiceClient.GetReport(getReportRequest);
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;

				
				var requestId = response?.ResponseHeaderMetadata?.RequestId ?? "unknown";
			    var timestamp = response?.ResponseHeaderMetadata?.Timestamp ?? "unknown";
			    _logger.Info(
				    $"Report download from Amazon has succeeded for {reportRequestEntry.RegionAndTypeComputed}.",
				    new RequestInfo(timestamp, requestId));
			    reportResultStream.Position = 0;

				var md5Hash = response?.GetReportResult?.ContentMD5;
				var hasValidHash = MD5ChecksumHelper.IsChecksumCorrect(reportResultStream, md5Hash);
			    if (hasValidHash)
			    {
				    _logger.Info($"Checksum verification succeeded for report {reportRequestEntry.RegionAndTypeComputed}");
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
					_logger.Warn($"Checksum verification failed for report {reportRequestEntry.RegionAndTypeComputed}. Report download will be retried. ReportDownloadRetryCount is now : '{reportRequestEntry.ReportDownloadRetryCount}'.");
				}

			    reportRequestService.Update(reportRequestEntry);
			}
		    catch (MarketplaceWebServiceException e) when (e.StatusCode == HttpStatusCode.BadRequest && IsAmazonErrorCodeFatal(e.ErrorCode))
		    {
			    reportRequestService.Delete(reportRequestEntry);
			    _logger.Error($"AmazonMWS report download failed for {reportRequestEntry.RegionAndTypeComputed}! The entry will now be removed from queue.", e);
			}
		    catch (MarketplaceWebServiceException e) when (IsAmazonErrorCodeNonFatal(e.ErrorCode))
			{
				reportRequestEntry.ReportDownloadRetryCount++;
				reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
				reportRequestService.Update(reportRequestEntry);
				_logger.Warn($"AmazonMWS report download failed for {reportRequestEntry.RegionAndTypeComputed} Report download will be retried. ReportDownloadRetryCount is now : '{reportRequestEntry.ReportDownloadRetryCount}'.");
			}
			catch (Exception e)
		    {
			    reportRequestEntry.ReportDownloadRetryCount++;
			    reportRequestEntry.LastAmazonRequestDate = DateTime.UtcNow;
			    reportRequestService.Update(reportRequestEntry);
				_logger.Warn($"AmazonMWS report download failed for {reportRequestEntry.RegionAndTypeComputed}!");
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
			    _logger.Warn($"Report request entry {entry.RegionAndTypeComputed} deleted from queue. {deleteReason}");
		    }
	    }
	    private bool IsMatchForRegionAndMerchantId(ReportRequestEntry e) => e.AmazonRegion == _region && e.MerchantId == _merchantId;
	    private bool IsRequestRetryCountExceeded(ReportRequestEntry e) => e.ReportRequestRetryCount > _options.ReportRequestMaxRetryCount;
	    private bool IsDownloadRetryCountExceeded(ReportRequestEntry e) => e.ReportDownloadRetryCount > _options.ReportDownloadMaxRetryCount;
	    private bool IsProcessingRetryCountExceeded(ReportRequestEntry e) => e.ReportProcessRetryCount > _options.ReportProcessingMaxRetryCount;
	    private bool IsCallbackInvocationRetryCountExceeded(ReportRequestEntry e) => e.InvokeCallbackRetryCount > _options.InvokeCallbackMaxRetryCount;
	    private bool IsExpirationPeriodExceeded(ReportRequestEntry reportRequestEntry) =>
		    (DateTime.Compare(reportRequestEntry.DateCreated, DateTime.UtcNow.Subtract(_options.ReportDownloadRequestEntryExpirationPeriod)) < 0);

	    private class EntryToDelete : IEquatable<EntryToDelete>
	    {
		    public ReportRequestEntry Entry { get; set; }
		    public DeleteReasonType DeleteReason { get; set; }

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

	    private enum DeleteReasonType
	    {
		    ReportRequestMaxRetryCount,
		    ReportDownloadMaxRetryCount,
		    ReportProcessingMaxRetryCount,
		    InvokeCallbackMaxRetryCount,
		    ReportDownloadRequestEntryExpirationPeriod
	    }
	}
}
