using System;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
    internal class FeedSubmissionCallbackService : IFeedSubmissionCallbackService
    {
	    private readonly IFeedSubmissionCallbackRepo _feedRepo;
	    private readonly IEasyMwsLogger _logger;

	    public FeedSubmissionCallbackService(IFeedSubmissionCallbackRepo feedSubmissionCallbackRepo = null, IEasyMwsLogger logger = null, EasyMwsOptions options = null) =>
		    (_feedRepo, _logger) = (feedSubmissionCallbackRepo ?? new FeedSubmissionCallbackRepo(options?.LocalDbConnectionStringOverride), logger);

	    public void Create(FeedSubmissionEntry entry) => _feedRepo.Create(entry);
	    public void Update(FeedSubmissionEntry entry) => _feedRepo.Update(entry);
		public void Delete(int id)
	    {
		    try
		    {
			    _feedRepo.Delete(id);
			}
		    catch (Exception e)
		    {
			    if (!_feedRepo.GetAll().Where(fs => fs.Id == id).Select(f => f.Id).Any())
			    {
				    _logger.Error($"Delete FeedSubmissionCallback entity with ID: {id} failed. It is likely the entity has already been deleted.",e);
			    }
			    else
			    {
				    _logger.Error($"Delete FeedSubmissionCallback entity with ID: {id} failed. See exception info for more details", e);
			    }
		    }
		    
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
	}
}
