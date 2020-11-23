using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using MountainWarehouse.EasyMWS.CallbackLogic;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Services;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal class ReportProcessor : IReportQueueingProcessor
	{
		private readonly IRequestReportProcessor _requestReportProcessor;
		private readonly ICallbackActivator _callbackActivator;
		private readonly IEasyMwsLogger _logger;
		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

        public event EventHandler<ReportDownloadedEventArgs> ReportDownloadedInternal;
		public event EventHandler<ReportRequestFailedEventArgs> ReportRequestFailedInternal;

		/// <summary>
		/// Constructor to be used for UnitTesting/Mocking (in the absence of a dedicated DependencyInjection framework)
		/// </summary>
		internal ReportProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient,
			IRequestReportProcessor requestReportProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
			: this(region, merchantId, mWSAuthToken, options, mwsClient, logger)
		{
			_requestReportProcessor = requestReportProcessor;
			_callbackActivator = callbackActivator;

			RegisterEvents();
		}

		internal ReportProcessor(AmazonRegion region, string merchantId, string mWSAuthToken, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();
			_requestReportProcessor = _requestReportProcessor ?? new RequestReportProcessor(_region, _merchantId, mWSAuthToken, mwsClient, _logger, _options);

			RegisterEvents();
		}

		private void RegisterEvents()
		{
			_requestReportProcessor.ReportEntryWasMarkedForDelete -= OnReportRequestFailedInternal;
			_requestReportProcessor.ReportEntryWasMarkedForDelete += OnReportRequestFailedInternal;
		}

		private void OnReportRequestFailedInternal(object sender, ReportRequestFailedEventArgs e) => ReportRequestFailedInternal?.Invoke(null, e);

		public void PollReports(IReportRequestEntryService reportRequestService)
		{
			_logger.Debug("Executing polling action for report requests.");

			_requestReportProcessor.CleanupReportRequests(reportRequestService);

			RequestNextReportInQueueFromAmazon(reportRequestService);

			RequestReportStatusesFromAmazon(reportRequestService);

			DownloadNextReportInQueueFromAmazon(reportRequestService);

			PublishEventsForPreviouslyDownloadedReports(reportRequestService);
		}

		public void DownloadNextReportInQueueFromAmazon(IReportRequestEntryService reportRequestService)
		{
			var reportToDownload = reportRequestService.GetNextFromQueueOfReportsToDownload(_merchantId, _region);
			if (reportToDownload == null) return;

			_requestReportProcessor.DownloadGeneratedReportFromAmazon(reportRequestService, reportToDownload);
		}

        private void OnReportDownloaded(ReportDownloadedEventArgs e) => ReportDownloadedInternal?.Invoke(this, e);

		private void PublishEventsForPreviouslyDownloadedReports(IReportRequestEntryService reportRequestService)
		{
			var reportsReadyForCallback = reportRequestService.GetAllFromQueueOfReportsReadyForCallback(_merchantId, _region);

			foreach (var reportEntry in reportsReadyForCallback)
			{
				try
				{
                    var reportType = reportEntry.ReportType;
                    var handledId = reportEntry.TargetHandlerId;
                    var handlerArgs = (reportEntry.TargetHandlerArgs == null) ? null : new ReadOnlyDictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(reportEntry.TargetHandlerArgs));

                    if (reportEntry.Details == null && reportEntry.LastAmazonReportProcessingStatus == AmazonReportProcessingStatus.DoneNoData && !_options.EventPublishingOptions.EventPublishingForReportStatusDoneNoData)
                    {
                        _logger.Debug($"An attempt will not be made to publish event ReportDownloaded for the following report in queue : {reportEntry.EntryIdentityDescription}, because AmazonProcessingStatus for this report is _DONE_NO_DATA_ but EventPublishingForReportStatusDoneNoData EasyMwsOption is FALSE.");
                    }
                    else if (reportEntry.Details == null && reportEntry.LastAmazonReportProcessingStatus == AmazonReportProcessingStatus.DoneNoData && _options.EventPublishingOptions.EventPublishingForReportStatusDoneNoData)
                    {
                        _logger.Warn($"Attempting to publish event ReportDownloaded for the following report in queue : {reportEntry.EntryIdentityDescription}, but the AmazonProcessingStatus for this report is _DONE_NO_DATA_ therefore the Stream argument will be null at invocation time.");
                        var eventArgs = new ReportDownloadedEventArgs(null, reportType, handledId, handlerArgs);
                        OnReportDownloaded(eventArgs);
                    }
                    else
                    {
                        _logger.Debug($"Attempting to publish event ReportDownloaded for the next downloaded report in queue : {reportEntry.EntryIdentityDescription}.");
                        var reportContent = ZipHelper.ExtractArchivedSingleFileToStream(reportEntry.Details?.ReportContent);
                        var eventArgs = new ReportDownloadedEventArgs(reportContent, reportType, handledId, handlerArgs);
                        OnReportDownloaded(eventArgs);
                    }

                    reportRequestService.Delete(reportEntry);
					_logger.Info($"Event publishing has succeeded for {reportEntry.EntryIdentityDescription}.");
				}
				catch(SqlException e)
				{
					_logger.Error($"ReportDownloaded event publishing failed for {reportEntry.EntryIdentityDescription} due to an internal error '{e.Message}'. The event publishing will be retried at the next poll request", e);
					reportRequestService.Unlock(reportEntry);
					reportRequestService.Update(reportEntry);
				}
				catch (Exception e)
				{
					_logger.Error($"ReportDownloaded event publishing failed for {reportEntry.EntryIdentityDescription}. Current retry count is :{reportEntry.InvokeCallbackRetryCount}. {e.Message}", e);
					reportEntry.InvokeCallbackRetryCount++;
					reportRequestService.Unlock(reportEntry);
					reportRequestService.Update(reportEntry);
				}
			}

			reportRequestService.SaveChanges();
		}

		public void QueueReport(IReportRequestEntryService reportRequestService, ReportRequestPropertiesContainer propertiesContainer, string targetEventId, Dictionary<string, object> targetEventArgs)
		{
			try
			{
				if (propertiesContainer == null) throw new ArgumentNullException();

				var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

                var reportRequest = new ReportRequestEntry(serializedPropertiesContainer)
				{
					AmazonRegion = _region,
					MerchantId = _merchantId,
					LastAmazonRequestDate = DateTime.MinValue,
					DateCreated = DateTime.UtcNow,
					ContentUpdateFrequency = propertiesContainer.UpdateFrequency,
					RequestReportId = null,
					GeneratedReportId = null,
					ReportRequestRetryCount = 0,
					ReportDownloadRetryCount = 0,
					ReportProcessRetryCount = 0,
					InvokeCallbackRetryCount = 0,
					ReportType = propertiesContainer.ReportType,
                    TargetHandlerId = targetEventId,
                    TargetHandlerArgs = targetEventArgs == null ? null : JsonConvert.SerializeObject(targetEventArgs),
                    InstanceId = _options?.EventPublishingOptions?.RestrictInvocationToOriginatingInstance?.HashedInstanceId,
                };

				reportRequestService.Unlock(reportRequest);
				reportRequestService.Create(reportRequest);
				reportRequestService.SaveChanges();

				_logger.Info($"The following report was queued for download from Amazon {reportRequest.EntryIdentityDescription}.");
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void PurgeQueue(IReportRequestEntryService reportRequestService)
		{
			var entriesToDelete = reportRequestService.GetAll().Where(rre => rre.AmazonRegion == _region && rre.MerchantId == _merchantId);
			reportRequestService.DeleteRange(entriesToDelete);
			reportRequestService.SaveChanges();
		}

		public void RequestNextReportInQueueFromAmazon(IReportRequestEntryService reportRequestService)
		{
			var reportRequest = reportRequestService.GetNextFromQueueOfReportsToRequest(_merchantId, _region);

			if (reportRequest == null) return;

			_requestReportProcessor.RequestReportFromAmazon(reportRequestService, reportRequest);
		}

		public void RequestReportStatusesFromAmazon(IReportRequestEntryService reportRequestService)
		{
			var pendingReportsRequestIds = reportRequestService.GetAllPendingReportFromQueue(_merchantId, _region).ToList();

			if (!pendingReportsRequestIds.Any()) return;

			var reportRequestStatuses =
				_requestReportProcessor.GetReportProcessingStatusesFromAmazon(reportRequestService, pendingReportsRequestIds, _merchantId);

			if (reportRequestStatuses != null)
			{
				_requestReportProcessor.QueueReportsAccordingToProcessingStatus(reportRequestService, reportRequestStatuses);
			}

			_requestReportProcessor.UnlockReportRequestEntries(reportRequestService, pendingReportsRequestIds);
		}
	}
}
