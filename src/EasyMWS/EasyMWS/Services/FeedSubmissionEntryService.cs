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

		public FeedSubmissionEntry GetNextFromQueueOfFeedsToSubmit(EasyMwsOptions options, string merchantId, AmazonRegion region) 
			=> FirstOrDefault(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
										&& fse.FeedSubmissionId == null
				                        && IsFeedSubmissionRetryPeriodAwaited(options, fse));


		public IEnumerable<string> GetIdsForSubmittedFeedsFromQueue(EasyMwsOptions options, string merchantId, AmazonRegion region) 
			=> Where(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
				        && fse.FeedSubmissionId != null && fse.IsProcessingComplete == false
			).Select(f => f.FeedSubmissionId);

		public FeedSubmissionEntry GetNextFromQueueOfProcessingCompleteFeeds(EasyMwsOptions options, string merchantId, AmazonRegion region)
			=> FirstOrDefault(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId
						 && fse.FeedSubmissionId != null && fse.IsProcessingComplete == true
				         && IsProcessingReportDownloadRetryPeriodAwaited(options, fse));

		public IEnumerable<FeedSubmissionEntry> GetAllFromQueueOfFeedsReadyForCallback(EasyMwsOptions options, string merchantId, AmazonRegion region)
			=> Where(fse => fse.AmazonRegion == region && fse.MerchantId == merchantId 
						&& fse.Details != null && fse.Details.FeedSubmissionReport != null
						&& IsFeedCallbackRetryPeriodAwaited(options, fse));

		private bool IsFeedCallbackRetryPeriodAwaited(EasyMwsOptions options, FeedSubmissionEntry feedSubmission)
			=> feedSubmission.InvokeCallbackRetryCount == 0 || (feedSubmission.InvokeCallbackRetryCount > 0 
			&& RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted, feedSubmission.InvokeCallbackRetryCount,
				options.InvokeCallbackRetryInterval, options.InvokeCallbackRetryInterval,
				options.InvokeCallbackRetryPeriodType));

		private bool IsFeedSubmissionRetryPeriodAwaited(EasyMwsOptions options, FeedSubmissionEntry feedSubmission)
		=> feedSubmission.FeedSubmissionRetryCount == 0 || (feedSubmission.FeedSubmissionRetryCount > 0
			&& RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted,
				feedSubmission.FeedSubmissionRetryCount, options.FeedSubmissionRetryInitialDelay,
				options.FeedSubmissionRetryInterval, options.FeedSubmissionRetryType));
		

		private bool IsProcessingReportDownloadRetryPeriodAwaited(EasyMwsOptions options, FeedSubmissionEntry feedSubmission)
		=> feedSubmission.ReportDownloadRetryCount == 0 || (feedSubmission.ReportDownloadRetryCount > 0
			&& RetryIntervalHelper.IsRetryPeriodAwaited(feedSubmission.LastSubmitted,
				feedSubmission.ReportDownloadRetryCount, options.ReportDownloadRetryInitialDelay,
				options.ReportDownloadRetryInterval, options.ReportDownloadRetryType));

		public void Dispose()
		{
			_feedRepo.Dispose();
		}
	}
}
