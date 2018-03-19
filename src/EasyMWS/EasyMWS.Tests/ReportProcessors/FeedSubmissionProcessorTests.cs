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
		    var testMerchantId = "test merchant id";
		    var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 2, FeedSubmissionId = "testSubmissionId2", SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };


		    _feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);

		    var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue(AmazonRegion.Europe, testMerchantId);

			Assert.IsNotNull(feedSubmissionCallback);
		    Assert.AreEqual(feedSubmissionWithNullFeedSubmissionId1.Id, feedSubmissionCallback.Id);
	    }

	    [Test]
	    public void GetNextFeedToSubmitFromQueue_ReturnsFirstFeedSubmissionFromQueueForGivenRegionAndMerchantId()
	    {
		    var merchantId1 = "test merchant id 1";
		    var merchantId2 = "test merchant id 2";
		    var feedSubmissionWithDifferentRegion = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = merchantId1, Id = 2, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithSameRegionButDifferentMerchantId = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = merchantId1, Id = 2, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithSameRegionAndMerchantId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = merchantId2, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithSameRegionAndMerchantId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Australia, MerchantId = merchantId2, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };


			_feedSubmissionCallbacks.Add(feedSubmissionWithDifferentRegion);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionButDifferentMerchantId);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionAndMerchantId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithSameRegionAndMerchantId2);

			var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue(AmazonRegion.Australia, merchantId2);

		    Assert.IsNotNull(feedSubmissionCallback);
		    Assert.AreEqual(feedSubmissionWithSameRegionAndMerchantId1.Id, feedSubmissionCallback.Id);
		}

	    [Test]
	    public void GetNextFeedToSubmitFromQueue_CalledWithNullMerchantId_ReturnsNull()
	    {
			var testMerchantId = "test merchant id";
		    var feedSubmissionWithNonNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 2, FeedSubmissionId = "testSubmissionId2", SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId1 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 3, FeedSubmissionId = null, SubmissionRetryCount = 0 };
		    var feedSubmissionWithNullFeedSubmissionId2 = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = testMerchantId, Id = 4, FeedSubmissionId = null, SubmissionRetryCount = 0 };
			var feedSubmissionWithNullMerchant = new FeedSubmissionCallback { AmazonRegion = AmazonRegion.Europe, MerchantId = null, Id = 5, FeedSubmissionId = null, SubmissionRetryCount = 0 };


			_feedSubmissionCallbacks.Add(feedSubmissionWithNonNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId1);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullFeedSubmissionId2);
		    _feedSubmissionCallbacks.Add(feedSubmissionWithNullMerchant);

			var feedSubmissionCallback =
			    _feedSubmissionProcessor.GetNextFeedToSubmitFromQueue(AmazonRegion.Europe, null);

		    Assert.IsNull(feedSubmissionCallback);
		}

	    [Test]
	    public void SubmitSingleQueuedFeedToAmazon_CalledWithNullFeedSubmissionCallback_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(()=> _feedSubmissionProcessor.SubmitSingleQueuedFeedToAmazon(null, "testMerchantId"));
	    }

	    [Test]
	    public void SubmitSingleQueuedFeedToAmazon_CalledWithNullMerchantId_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() => _feedSubmissionProcessor.SubmitSingleQueuedFeedToAmazon(new FeedSubmissionCallback(), null));
	    }

	    [Test]
	    public void SubmitSingleQueuedFeedToAmazon_CalledWithEmptyMerchantId_ThrowsArgumentNullException()
	    {
		    Assert.Throws<ArgumentNullException>(() => _feedSubmissionProcessor.SubmitSingleQueuedFeedToAmazon(new FeedSubmissionCallback(), string.Empty));
	    }
	}
}
