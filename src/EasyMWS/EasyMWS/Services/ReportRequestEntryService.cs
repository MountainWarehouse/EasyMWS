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

		internal ReportRequestEntryService(IReportRequestEntryRepository reportRequestEntryRepository, EasyMwsOptions options = null, IEasyMwsLogger logger = null) : this(options, logger)
			=> (_reportRequestEntryRepository) = (reportRequestEntryRepository);

		internal ReportRequestEntryService(EasyMwsOptions options = null, IEasyMwsLogger logger = null) => (_reportRequestEntryRepository, _logger) =
			(_reportRequestEntryRepository ?? new ReportRequestEntryRepository(options?.LocalDbConnectionStringOverride), logger);

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
						? $"Delete ReportRequestCallback entity with ID: {entry.Id} failed. It is likely the entity has already been deleted."
						: $"Delete ReportRequestCallback entity with ID: {entry.Id} failed. See exception info for more details", e);
			}

		}

		public void DeleteRange(IEnumerable<ReportRequestEntry> entries)
		{
			_reportRequestEntryRepository.DeleteRange(entries);
		}

		public void SaveChanges() => _reportRequestEntryRepository.SaveChanges();
		public async Task SaveChangesAsync() => await _reportRequestEntryRepository.SaveChangesAsync();
		public IQueryable<ReportRequestEntry> GetAll() => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id);

		public IQueryable<ReportRequestEntry> Where(Expression<Func<ReportRequestEntry, bool>> predicate) => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).Where(predicate);

		public ReportRequestEntry First() => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).First();
		public ReportRequestEntry FirstOrDefault() => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public ReportRequestEntry FirstOrDefault(Expression<Func<ReportRequestEntry, bool>> predicate) => _reportRequestEntryRepository.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);

		public ReportRequestEntry Last() => _reportRequestEntryRepository.GetAll().OrderByDescending(x => x.Id).First();
		public ReportRequestEntry LastOrDefault() => _reportRequestEntryRepository.GetAll().OrderByDescending(x => x.Id).FirstOrDefault();
		public ReportRequestEntry LastOrDefault(Expression<Func<ReportRequestEntry, bool>> predicate) => _reportRequestEntryRepository.GetAll().OrderByDescending(x => x.Id).FirstOrDefault(predicate);

		public ReportRequestEntry GetNextFromQueueOfReportsToRequest(EasyMwsOptions options, string merchantId, AmazonRegion region)
		{
			return string.IsNullOrEmpty(merchantId)
				? null
				: GetAll()
					.FirstOrDefault(rrc => rrc.AmazonRegion == region && rrc.MerchantId == merchantId
										   && rrc.RequestReportId == null
					                       && RetryIntervalHelper.IsRetryPeriodAwaited(rrc.LastAmazonRequestDate, rrc.ReportRequestRetryCount,
						                       options.ReportRequestRetryInitialDelay, options.ReportRequestRetryInterval,
						                       options.ReportRequestRetryType)
					);
		}

		public ReportRequestEntry GetNextFromQueueOfReportsToDownload( EasyMwsOptions options, string merchantId, AmazonRegion region)
		{
			return string.IsNullOrEmpty(merchantId)
				? null
				: FirstOrDefault(
					rrc => rrc.AmazonRegion == region && rrc.MerchantId == merchantId
						   && rrc.RequestReportId != null
					       && rrc.GeneratedReportId != null
					       && rrc.Details == null
					       && RetryIntervalHelper.IsRetryPeriodAwaited(rrc.LastAmazonRequestDate, rrc.ReportDownloadRetryCount,
						       options.ReportDownloadRetryInitialDelay, options.ReportDownloadRetryInterval,
						       options.ReportDownloadRetryType));
		}

		public IEnumerable<string> GetAllPendingReportFromQueue(string merchantId, AmazonRegion region)
		{
			return string.IsNullOrEmpty(merchantId)
				? new List<string>().AsEnumerable()
				: Where(rrcs => rrcs.AmazonRegion == region && rrcs.MerchantId == merchantId
								   && rrcs.RequestReportId != null
					               && rrcs.GeneratedReportId == null)
					.Select(r => r.RequestReportId);
		}

		public IEnumerable<ReportRequestEntry> GetAllFromQueueOfReportsReadyForCallback(EasyMwsOptions options, string merchantId, AmazonRegion region)
		{
			return string.IsNullOrEmpty(merchantId)
				? null
				: GetAll().Where(
					rre => rre.AmazonRegion == region && rre.MerchantId == merchantId
					       && rre.Details != null
					       && RetryIntervalHelper.IsRetryPeriodAwaited(rre.LastAmazonRequestDate, rre.InvokeCallbackRetryCount,
						       options.InvokeCallbackRetryInterval, options.InvokeCallbackRetryInterval,
						       options.InvokeCallbackRetryPeriodType));
		}

		public void Dispose()
		{
			_reportRequestEntryRepository.Dispose();
		}
	}
}
