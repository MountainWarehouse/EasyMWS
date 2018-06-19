﻿using System;
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

		internal FeedSubmissionEntryService(IFeedSubmissionEntryRepository feedSubmissionRepo, EasyMwsOptions options = null,
			IEasyMwsLogger logger = null) : this(options, logger)
			=> (_feedRepo) = (feedSubmissionRepo);

		public FeedSubmissionEntryService(EasyMwsOptions options = null, IEasyMwsLogger logger = null) =>
		    (_feedRepo, _logger) = (_feedRepo ?? new FeedSubmissionEntryRepository(options?.LocalDbConnectionStringOverride), logger);

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
				    _logger.Error($"Delete FeedSubmissionCallback entity with ID: {entry.Id} failed. It is likely the entity has already been deleted.",e);
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
		public IQueryable<FeedSubmissionEntry> GetAll() => _feedRepo.GetAll().OrderBy(x => x.Id);
		public IQueryable<FeedSubmissionEntry> Where(Expression<Func<FeedSubmissionEntry, bool>> predicate) => _feedRepo.GetAll().OrderBy(x => x.Id).Where(predicate);
		public FeedSubmissionEntry First() => _feedRepo.GetAll().OrderBy(x => x.Id).First();
		public FeedSubmissionEntry FirstOrDefault() => _feedRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public FeedSubmissionEntry FirstOrDefault(Expression<Func<FeedSubmissionEntry, bool>> predicate) => _feedRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);
		public FeedSubmissionEntry Last() => _feedRepo.GetAll().OrderByDescending(x => x.Id).First();
		public FeedSubmissionEntry LastOrDefault() => _feedRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault();
		public FeedSubmissionEntry LastOrDefault(Expression<Func<FeedSubmissionEntry, bool>> predicate) => _feedRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault(predicate);

		public FeedSubmissionEntry GetNextFromQueueOfFeedsToSubmit(EasyMwsOptions options, string merchantId, AmazonRegion region) =>
			string.IsNullOrEmpty(merchantId) ? null : GetAll()
				.FirstOrDefault(fscs => fscs.AmazonRegion == region && fscs.MerchantId == merchantId
										&& IsFeedInASubmitFeedQueue(fscs)
				                        && IsFeedReadyForSubmission(options, fscs));


		public IEnumerable<string> GetIdsForSubmittedFeedsFromQueue(EasyMwsOptions options, string merchantId, AmazonRegion region) =>
			string.IsNullOrEmpty(merchantId) ? new List<string>().AsEnumerable() : Where(
				rrcs => rrcs.AmazonRegion == region && rrcs.MerchantId == merchantId
						&& rrcs.FeedSubmissionId != null
				        && rrcs.IsProcessingComplete == false
			).Select(f => f.FeedSubmissionId);

		public FeedSubmissionEntry GetNextFromQueueOfProcessingCompleteFeeds(EasyMwsOptions options, string merchantId, AmazonRegion region)
			=> string.IsNullOrEmpty(merchantId) ? null : FirstOrDefault(
				ffscs => ffscs.AmazonRegion == region && ffscs.MerchantId == merchantId
						 && ffscs.FeedSubmissionId != null
				         && ffscs.IsProcessingComplete == true
				         && IsReadyForRequestingSubmissionReport(options, ffscs));

		public IEnumerable<FeedSubmissionEntry> GetAllFromQueueOfFeedsReadyForCallback(EasyMwsOptions options,
			string merchantId, AmazonRegion region)
			=> string.IsNullOrEmpty(merchantId)
				? new List<FeedSubmissionEntry>().AsEnumerable()
				: Where(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId && fse.Details != null &&
				               fse.Details.FeedSubmissionReport != null);

		private bool IsFeedReadyForSubmission(EasyMwsOptions options, FeedSubmissionEntry feedSubmission)
		{
			var isInRetryQueueAndReadyForRetry = feedSubmission.FeedSubmissionRetryCount > 0
			                                     && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted,
				                                     feedSubmission.FeedSubmissionRetryCount, options.FeedSubmissionRetryInitialDelay,
				                                     options.FeedSubmissionRetryInterval, options.FeedSubmissionRetryType);

			var isNotInRetryState = feedSubmission.FeedSubmissionRetryCount == 0;

			return isInRetryQueueAndReadyForRetry || isNotInRetryState;
		}

		private bool IsReadyForRequestingSubmissionReport(EasyMwsOptions options, FeedSubmissionEntry feedSubmission)
		{
			var isInRetryQueueAndReadyForRetry = feedSubmission.FeedSubmissionRetryCount > 0
			                                     && RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted,
				                                     feedSubmission.FeedSubmissionRetryCount, options.ReportDownloadRetryInitialDelay,
				                                     options.ReportDownloadRetryInterval, options.ReportDownloadRetryType);

			var isNotInRetryState = feedSubmission.FeedSubmissionRetryCount == 0;

			return isInRetryQueueAndReadyForRetry || isNotInRetryState;
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
