using System;
using System.Linq;
using System.Linq.Expressions;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;

namespace MountainWarehouse.EasyMWS.Services
{
    internal class FeedSubmissionCallbackService : IFeedSubmissionCallbackService
    {
	    private readonly IFeedSubmissionCallbackRepo _feedRepo;

	    public FeedSubmissionCallbackService(IFeedSubmissionCallbackRepo feedSubmissionCallbackRepo = null, EasyMwsOptions options = null) =>
		    (_feedRepo) = (feedSubmissionCallbackRepo ?? new FeedSubmissionCallbackRepo(options?.LocalDbConnectionStringOverride));

	    public void Create(FeedSubmissionCallback callback) => _feedRepo.Create(callback);
	    public void Update(FeedSubmissionCallback callback) => _feedRepo.Update(callback);
		public void Delete(FeedSubmissionCallback callback) => _feedRepo.Delete(callback);
		public void SaveChanges() => _feedRepo.SaveChanges();
		public IQueryable<FeedSubmissionCallback> GetAll() => _feedRepo.GetAll().OrderBy(x => x.Id);
		public IQueryable<FeedSubmissionCallback> Where(Expression<Func<FeedSubmissionCallback, bool>> predicate) => _feedRepo.GetAll().OrderBy(x => x.Id).Where(predicate);
		public FeedSubmissionCallback First() => _feedRepo.GetAll().OrderBy(x => x.Id).First();
		public FeedSubmissionCallback FirstOrDefault() => _feedRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		public FeedSubmissionCallback FirstOrDefault(Expression<Func<FeedSubmissionCallback, bool>> predicate) => _feedRepo.GetAll().OrderBy(x => x.Id).FirstOrDefault(predicate);
		public FeedSubmissionCallback Last() => _feedRepo.GetAll().OrderByDescending(x => x.Id).First();
		public FeedSubmissionCallback LastOrDefault() => _feedRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault();
		public FeedSubmissionCallback LastOrDefault(Expression<Func<FeedSubmissionCallback, bool>> predicate) => _feedRepo.GetAll().OrderByDescending(x => x.Id).FirstOrDefault(predicate);
	}
}
