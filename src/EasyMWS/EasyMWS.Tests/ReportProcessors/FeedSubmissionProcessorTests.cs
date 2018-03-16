using System;
using System.Collections.Generic;
using System.Linq;
using MarketplaceWebService;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.ReportProcessors;
using MountainWarehouse.EasyMWS.Services;
using NUnit.Framework;

namespace EasyMWS.Tests.ReportProcessors
{
    public class FeedSubmissionProcessorTests
    {
	    private IFeedSubmissionProcessor _feedSubmissionProcessor;
	    private Mock<IMarketplaceWebServiceClient> _marketplaceWebServiceClientMock;
	    private EasyMwsOptions _easyMwsOptions;
		private Mock<IFeedSubmissionCallbackService> _feedSubmissionCallbackServiceMock;
	    private List<FeedSubmissionCallback> _feedSubmissionCallbacks;

		[SetUp]
	    public void Setup()
	    {
		    _easyMwsOptions = EasyMwsOptions.Defaults;
			_marketplaceWebServiceClientMock = new Mock<IMarketplaceWebServiceClient>();
		    _feedSubmissionCallbackServiceMock = new Mock<IFeedSubmissionCallbackService>();
			_feedSubmissionProcessor = new FeedSubmissionProcessor(_marketplaceWebServiceClientMock.Object, _feedSubmissionCallbackServiceMock.Object, _easyMwsOptions);

			_feedSubmissionCallbacks = new List<FeedSubmissionCallback>
			{
				new FeedSubmissionCallback
				{
					AmazonRegion = AmazonRegion.Europe,
					Id = 1,
					FeedSubmissionId = "testSubmissionId1",
					FeedSubmissionData = "testFeedSubmissionData"
				}
			};

		    _feedSubmissionCallbackServiceMock.Setup(x => x.GetAll()).Returns(_feedSubmissionCallbacks.AsQueryable());
		}

	    [Test]
	    public void GetNextFeedToSubmitFromQueue_ReturnsFirstFeedSubmissionFromQueueWithNullFeedSubmissionId_AndSkipsEntriesWithNonNullFeedSubmissionId()
	    {
		    var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, Id = 2, FeedSubmissionId = "testSubmissionId2", SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };


		    _feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);

		    var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue();

			Assert.IsNotNull(feedSubmissionCallback);
		    Assert.AreEqual(feedSubmissionWithNullFeedSubmissionId1.Id, feedSubmissionCallback.Id);
	    }

	    [Test]
	    public void GetNextFeedToSubmitFromQueue_ReturnsFirstFeedSubmissionFromQueueWithAnyRegionSet()
	    {
			throw new ApplicationException(@"Need to do refactor so that stuff is returned to clients that submitted it (region, merchantId) and not other clients !!!
				Fix this for both Reports and Feeds");
		}
	}
}
