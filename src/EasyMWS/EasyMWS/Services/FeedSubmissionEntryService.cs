using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
	internal class FeedSubmissionEntryService : IFeedSubmissionEntryService, IDisposable
	{
		private readonly IFeedSubmissionEntryRepository _feedRepo;
		private readonly IEasyMwsLogger _logger;
		private readonly EasyMwsOptions _options;

		internal FeedSubmissionEntryService(IFeedSubmissionEntryRepository feedSubmissionRepo, EasyMwsOptions options = null,
			IEasyMwsLogger logger = null) : this(options, logger)
			=> (_feedRepo) = (feedSubmissionRepo);

		public FeedSubmissionEntryService(EasyMwsOptions options = null, IEasyMwsLogger logger = null) =>
			(_feedRepo, _logger, _options) = (_feedRepo ?? new FeedSubmissionEntryRepository(options?.LocalDbConnectionStringOverride), logger, options);

		public void Create(FeedSubmissionEntry entry) => _feedRepo.Create(entry);
		public void Update(FeedSubmissionEntry entry) => _feedRepo.Update(entry);
		public void Delete(FeedSubmissionEntry entry)
		{
			try
			{
				_feedRepo.Delete(entry);
			}
			catch (Exception e)
			{
				if (!_feedRepo.GetAll().Where(fs => fs.Id == entry.Id).Select(f => f.Id).Any())
				{
					_logger.Error($"Delete FeedSubmissionCallback entity with ID: {entry.Id} failed. It is likely the entity has already been deleted", e);
				}
				else
				{
					_logger.Error($"Delete FeedSubmissionCallback entity with ID: {entry.Id} failed. See exception info for more details", e);
				}
			}

		}

		public void DeleteRange(IEnumerable<FeedSubmissionEntry> entries)
		{
			_feedRepo.DeleteRange(entries);
		}

		public void SaveChanges() => _feedRepo.SaveChanges();
		public IEnumerable<FeedSubmissionEntry> GetAll() => _feedRepo.GetAll().OrderBy(x => x.Id);
		public IEnumerable<FeedSubmissionEntry> Where(Func<FeedSubmissionEntry, bool> predicate) => _feedRepo.GetAll().OrderBy(x => x.Id).Where(predicate);
		public FeedSubmissionEntry First() => _feedRepo.GetAll().OrderBy(x => x.Id).First();
		public FeedSubmissionEntry FirstOrDefault() => _feedRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public FeedSubmissionEntry FirstOrDefault(Func<FeedSubmissionEntry, bool> predicate) => _feedRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);
		public FeedSubmissionEntry GetNextFromQueueOfFeedsToSubmit(string merchantId, AmazonRegion region, bool markEntryAsLocked = true)
		{
			var entry = FirstOrDefault(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
			                         && IsFeedInASubmitFeedQueue(fse)
			                         && RetryIntervalHelper.IsRetryPeriodAwaited(fse.LastSubmitted,
										 fse.FeedSubmissionRetryCount, _options.FeedSubmissionOptions.FeedSubmissionRetryInitialDelay,
										 _options.FeedSubmissionOptions.FeedSubmissionRetryInterval, _options.FeedSubmissionOptions.FeedSubmissionRetryType)
									&& fse.IsLocked == false);

			if(entry != null && markEntryAsLocked)
			{
				entry.IsLocked = true;
				Update(entry);
				SaveChanges();
			}

			return entry;
		}


		public IEnumerable<string> GetIdsForSubmittedFeedsFromQueue(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true)
		{
			var entries = Where(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
						  && fse.FeedSubmissionId != null && fse.IsProcessingComplete == false && fse.IsLocked == false);
			var entriesIds = entries.Select(f => f.FeedSubmissionId).ToList();

			if (entries.Any() && markEntriesAsLocked)
			{
				foreach (var entry in entries)
				{
					entry.IsLocked = true;
					Update(entry);
				}
				SaveChanges();
			}

			return entriesIds;
		}

		public FeedSubmissionEntry GetNextFromQueueOfProcessingCompleteFeeds(string merchantId, AmazonRegion region, bool markEntryAsLocked = true)
		{
			var entry = FirstOrDefault(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
						   && fse.FeedSubmissionId != null && fse.IsProcessingComplete == true
						   && RetryIntervalHelper.IsRetryPeriodAwaited(fse.LastSubmitted,
										   fse.ReportDownloadRetryCount, _options.ReportRequestOptions.ReportDownloadRetryInitialDelay,
										   _options.ReportRequestOptions.ReportDownloadRetryInterval, _options.ReportRequestOptions.ReportDownloadRetryType)
						   && fse.IsLocked == false);

			if (entry != null && markEntryAsLocked)
			{
				entry.IsLocked = true;
				Update(entry);
				SaveChanges();
			}

			return entry;
		}

		public IEnumerable<FeedSubmissionEntry> GetAllFromQueueOfFeedsReadyForCallback(string merchantId, AmazonRegion region, bool markEntriesAsLocked = true)
		{
			var entries = Where(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
						   && fse.Details != null && fse.Details.FeedSubmissionReport != null && fse.IsLocked == false).ToList();

            entries = EntryInvocationRestrictionHelper<FeedSubmissionEntry>.RestrictInvocationToOriginatingClientsIfEnabled(entries, _options);

            if(entries.Any() && markEntriesAsLocked)
			{
				foreach (var entry in entries)
				{
					entry.IsLocked = true;
					Update(entry);
				}
				SaveChanges();
			}

			return entries;
		}

        private bool IsFeedInASubmitFeedQueue(FeedSubmissionEntry feedSubmission)
		{
			return feedSubmission.FeedSubmissionId == null;
		}

		public void Dispose()
		{
			_feedRepo.Dispose();
		}
	}
}
