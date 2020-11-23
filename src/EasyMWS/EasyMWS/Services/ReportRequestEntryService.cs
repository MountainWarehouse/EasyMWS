using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
	internal class ReportRequestEntryService : IReportRequestEntryService, IDisposable
	{
		private readonly IReportRequestEntryRepository _reportRequestEntryRepository;
		private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;

		internal ReportRequestEntryService(IReportRequestEntryRepository reportRequestEntryRepository, EasyMwsOptions options = null, IEasyMwsLogger logger = null) : this(options, logger)
			=> (_reportRequestEntryRepository) = (reportRequestEntryRepository);

		internal ReportRequestEntryService(EasyMwsOptions options = null, IEasyMwsLogger logger = null) => (_reportRequestEntryRepository, _logger, _options) =
			(_reportRequestEntryRepository ?? new ReportRequestEntryRepository(options?.LocalDbConnectionStringOverride), logger, options);

		public void Lock(ReportRequestEntry entry, string reason)
		{
			if (entry.IsLocked)
			{
				_logger.Debug($"{DateTime.UtcNow}: ReportRequest Entry lock attempt: {entry.EntryIdentityDescription}, but it is already locked. Reason: {reason}.");
			}
			else
			{
				entry.IsLocked = true;
				_logger.Debug($"{DateTime.UtcNow}: ReportRequest Entry locked: {entry.EntryIdentityDescription}. Reason: {reason}.");
			}
		}

		public void Unlock(ReportRequestEntry entry, string reason)
		{
			if (!entry.IsLocked)
			{
				_logger.Debug($"{DateTime.UtcNow}: ReportRequest Entry unlock attempt: {entry.EntryIdentityDescription}, but it is already unlocked. Reason: {reason}.");
			}
			else
			{
				entry.IsLocked = false;
				_logger.Debug($"{DateTime.UtcNow}: ReportRequest Entry unlocked: {entry.EntryIdentityDescription}. Reason: {reason}.");
			}
		}

		public void Create(ReportRequestEntry entry) => _reportRequestEntryRepository.Create(entry);
		public async Task CreateAsync(ReportRequestEntry entry) => await _reportRequestEntryRepository.CreateAsync(entry);
		public void Update(ReportRequestEntry entry) => _reportRequestEntryRepository.Update(entry);
		public void Delete(ReportRequestEntry entry)
		{
			try
			{
				_reportRequestEntryRepository.Delete(entry);
			}
			catch (Exception e)
			{
				_logger.Error(!_reportRequestEntryRepository.GetAll().Where(rr => rr.Id == entry.Id).Select(r => r.Id).Any()
						? $"Delete ReportRequestCallback entity with ID: {entry.Id} failed. It is likely the entity has already been deleted"
						: $"Delete ReportRequestCallback entity with ID: {entry.Id} failed. See exception info for more details", e);
			}

		}

		public void DeleteRange(IEnumerable<ReportRequestEntry> entries)
		{
			_reportRequestEntryRepository.DeleteRange(entries);
		}

		public void SaveChanges() => _reportRequestEntryRepository.SaveChanges();
		public async Task SaveChangesAsync() => await _reportRequestEntryRepository.SaveChangesAsync();
		public IEnumerable<ReportRequestEntry> GetAll() => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id);

		public IEnumerable<ReportRequestEntry> Where(Func<ReportRequestEntry, bool> predicate) => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).Where(predicate);

		public ReportRequestEntry First() => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).First();
		public ReportRequestEntry FirstOrDefault() => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public ReportRequestEntry FirstOrDefault(Func<ReportRequestEntry, bool> predicate) => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);

		public ReportRequestEntry GetNextFromQueueOfReportsToRequest(string merchantId, AmazonRegion region, bool markEntryAsLocked = true)
		{
			var entry = FirstOrDefault(rre => rre.AmazonRegion == region && rre.MerchantId == merchantId
											 && rre.RequestReportId == null
											 && RetryIntervalHelper.IsRetryPeriodAwaited(rre.LastAmazonRequestDate, rre.ReportRequestRetryCount,
                                       _options.ReportRequestOptions.ReportRequestRetryInitialDelay, _options.ReportRequestOptions.ReportRequestRetryInterval,
									   _options.ReportRequestOptions.ReportRequestRetryType)
									   && rre.IsLocked == false);

			if (entry != null && markEntryAsLocked)
			{
				Lock(entry, "Locking single report request entry - ready for initial submission to amazon.");
				Update(entry);
				SaveChanges();
			}

			return entry;
		}


		public ReportRequestEntry GetNextFromQueueOfReportsToDownload(string merchantId, AmazonRegion region, bool markEntryAsLocked = true)
		{
			var entry = FirstOrDefault(rre => rre.AmazonRegion == region && rre.MerchantId == merchantId
							 && rre.RequestReportId != null && rre.GeneratedReportId != null && rre.Details == null && rre.LastAmazonReportProcessingStatus != AmazonReportProcessingStatus.DoneNoData
                             && RetryIntervalHelper.IsRetryPeriodAwaited(rre.LastAmazonRequestDate, rre.ReportDownloadRetryCount,
									   _options.ReportRequestOptions.ReportDownloadRetryInitialDelay, _options.ReportRequestOptions.ReportDownloadRetryInterval,
									   _options.ReportRequestOptions.ReportDownloadRetryType)
									   && rre.IsLocked == false);

			if (entry != null && markEntryAsLocked)
			{
				Lock(entry, "Locking single report request entry - ready to download report from amazon");
				Update(entry);
				SaveChanges();
			}

			return entry;
		}

		public IEnumerable<string> GetAllPendingReportFromQueue(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true)
		{
			var entries = Where(rre => rre.AmazonRegion == region && rre.MerchantId == merchantId
									 && rre.RequestReportId != null && rre.GeneratedReportId == null && rre.IsLocked == false);

			var entriesIds = entries.Select(r => r.RequestReportId)
				.Take(20)
				.ToList();

			if (entries.Any() && markEntriesAsLocked)
			{
				foreach (var entry in entries)
				{
					Lock(entry, "Locking multiple report request entries - ready for getting report processing statuses from amazon");
					Update(entry);
				}
				SaveChanges();
			}

			return entriesIds;
		}

		public IEnumerable<ReportRequestEntry> GetAllFromQueueOfReportsReadyForCallback(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true)
		{
			var entries = Where(rre => rre.AmazonRegion == region && rre.MerchantId == merchantId
							  && (rre.Details != null || rre.LastAmazonReportProcessingStatus == AmazonReportProcessingStatus.DoneNoData)
							  && RetryIntervalHelper.IsRetryPeriodAwaited(rre.LastAmazonRequestDate, rre.InvokeCallbackRetryCount,
							   _options.EventPublishingOptions.EventPublishingRetryInterval, _options.EventPublishingOptions.EventPublishingRetryInterval,
							   _options.EventPublishingOptions.EventPublishingRetryPeriodType) && rre.IsLocked == false).ToList();

            entries = EntryInvocationRestrictionHelper<ReportRequestEntry>.RestrictInvocationToOriginatingClientsIfEnabled(entries, _options);

            if (entries.Any() && markEntriesAsLocked)
			{
				foreach (var entry in entries)
				{
					Lock(entry, "Locking multiple report request entries - ready for callback invocation");
					Update(entry);
				}
				SaveChanges();
			}

			return entries;
		}

        public void Dispose()
		{
			_reportRequestEntryRepository.Dispose();
		}
	}
}
