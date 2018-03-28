﻿using System;
using System.IO;
using System.Linq;
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
	internal class ReportProcessor : IQueueingProcessor<ReportRequestPropertiesContainer>, IReportProcessor
	{
		private readonly IReportRequestCallbackService _reportService;
		private readonly IRequestReportProcessor _requestReportProcessor;
		private readonly ICallbackActivator _callbackActivator;
		private readonly IEasyMwsLogger _logger;

		private readonly AmazonRegion _region;
		private readonly string _merchantId;
		private readonly EasyMwsOptions _options;

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IReportRequestCallbackService reportService, IMarketplaceWebServiceClient mwsClient,
			IRequestReportProcessor requestReportProcessor, ICallbackActivator callbackActivator, IEasyMwsLogger logger)
			: this(region, merchantId, options, mwsClient, logger)
		{
			_reportService = reportService;
			_requestReportProcessor = requestReportProcessor;
			_callbackActivator = callbackActivator;
		}

		internal ReportProcessor(AmazonRegion region, string merchantId, EasyMwsOptions options,
			IMarketplaceWebServiceClient mwsClient, IEasyMwsLogger logger)
		{
			_region = region;
			_merchantId = merchantId;
			_options = options;
			_logger = logger;

			_callbackActivator = _callbackActivator ?? new CallbackActivator();

			_reportService = _reportService ?? new ReportRequestCallbackService();
			_requestReportProcessor = _requestReportProcessor ?? new RequestReportProcessor(mwsClient, _reportService, options);
		}


		public void Poll()
		{
			try
			{
				CleanUpReportRequestQueue();
				RequestNextReportInQueueFromAmazon();
				_reportService.SaveChanges();

				RequestReportStatusesFromAmazon();
				_reportService.SaveChanges();

				var generatedReportRequestCallback = DownloadNextReportInQueueFromAmazon();
				_reportService.SaveChanges();

				ExecuteCallback(generatedReportRequestCallback.reportRequestCallback, generatedReportRequestCallback.stream);
				_reportService.SaveChanges();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void Queue(ReportRequestPropertiesContainer propertiesContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			try
			{
				_reportService.Create(GetSerializedReportRequestCallback(propertiesContainer, callbackMethod, callbackData));
				_reportService.SaveChanges();
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public void CleanUpReportRequestQueue()
		{
			var expiredReportRequests = _reportService.GetAll()
				.Where(rrc => rrc.RequestRetryCount > _options.ReportRequestMaxRetryCount);

			foreach (var reportRequest in expiredReportRequests)
			{
				_reportService.Delete(reportRequest);
			}
		}

		public void RequestNextReportInQueueFromAmazon()
		{
			var reportRequestCallbackReportQueued = _requestReportProcessor.GetNextFromQueueOfReportsToRequest(_region, _merchantId);

			if (reportRequestCallbackReportQueued == null) return;

			try
			{
				var reportRequestId = _requestReportProcessor.RequestReportFromAmazon(reportRequestCallbackReportQueued);

				reportRequestCallbackReportQueued.LastRequested = DateTime.UtcNow;
				_reportService.Update(reportRequestCallbackReportQueued);

				if (string.IsNullOrEmpty(reportRequestId))
				{
					_requestReportProcessor.MoveToRetryQueue(reportRequestCallbackReportQueued);
				}
				else
				{
					_requestReportProcessor.GetNextFromQueueOfReportsToGenerate(reportRequestCallbackReportQueued, reportRequestId);
				}
			}
			catch (Exception e)
			{
				_requestReportProcessor.MoveToRetryQueue(reportRequestCallbackReportQueued);
				_logger.Error(e.Message, e);
			}
		}

		public void RequestReportStatusesFromAmazon()
		{
			try
			{
				var reportRequestCallbacksPendingReports = _requestReportProcessor.GetAllPendingReportFromQueue(_region, _merchantId).ToList();

				if (!reportRequestCallbacksPendingReports.Any()) return;

				var reportRequestIds = reportRequestCallbacksPendingReports.Select(x => x.RequestReportId);

				var reportRequestStatuses = _requestReportProcessor.GetReportProcessingStatusesFromAmazon(reportRequestIds, _merchantId);

				_requestReportProcessor.QueueReportsAccordingToProcessingStatus(reportRequestStatuses);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		public (ReportRequestCallback reportRequestCallback, Stream stream) DownloadNextReportInQueueFromAmazon()
		{
			var generatedReportRequest = _requestReportProcessor.GetNextFromQueueOfReportsToDownload(_region, _merchantId);

			if (generatedReportRequest == null) return (null, null);

			try
			{
				var stream = _requestReportProcessor.DownloadGeneratedReportFromAmazon(generatedReportRequest);

				return (generatedReportRequest, stream);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
				return (null, null);
			}
		}

		public void ExecuteCallback(ReportRequestCallback reportRequestCallback, Stream stream)
		{
			try
			{
				if (reportRequestCallback == null || stream == null) return;

				var callback = new Callback(reportRequestCallback.TypeName, reportRequestCallback.MethodName,
					reportRequestCallback.Data, reportRequestCallback.DataTypeName);

				_callbackActivator.CallMethod(callback, stream);

				_requestReportProcessor.RemoveFromQueue(reportRequestCallback);
			}
			catch (Exception e)
			{
				_logger.Error(e.Message, e);
			}
		}

		private ReportRequestCallback GetSerializedReportRequestCallback(
			ReportRequestPropertiesContainer reportRequestContainer, Action<Stream, object> callbackMethod, object callbackData)
		{
			if (reportRequestContainer == null || callbackMethod == null) throw new ArgumentNullException();
			var serializedCallback = _callbackActivator.SerializeCallback(callbackMethod, callbackData);

			return new ReportRequestCallback(serializedCallback)
			{
				AmazonRegion = _region,
				MerchantId = _merchantId,
				LastRequested = DateTime.MinValue,
				ContentUpdateFrequency = reportRequestContainer.UpdateFrequency,
				RequestReportId = null,
				GeneratedReportId = null,
				RequestRetryCount = 0,
				ReportRequestData = JsonConvert.SerializeObject(reportRequestContainer)
			};
		}
	}
}