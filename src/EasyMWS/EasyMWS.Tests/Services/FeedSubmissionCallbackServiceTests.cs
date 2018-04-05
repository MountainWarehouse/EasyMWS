using System.Collections.Generic;
using System.Linq;
using Moq;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using MountainWarehouse.EasyMWS.Repositories;
using MountainWarehouse.EasyMWS.Services;
using Newtonsoft.Json;
using NUnit.Framework;

namespace EasyMWS.Tests.Services
{
    public class FeedSubmissionCallbackServiceTests
    {
	    private Mock<IFeedSubmissionCallbackRepo> _feedSubmissionCallbackRepoMock;
	    private IFeedSubmissionCallbackService _feedSubmissionCallbackService;

	    [SetUp]
	    public void Setup()
	    {
		    var propertiesContainer = new FeedSubmissionPropertiesContainer("testFeedContent", "testFeedType", false,
			    new List<string>(MwsMarketplaceGroup.AmazonEurope()));
		    var serializedPropertiesContainer = JsonConvert.SerializeObject(propertiesContainer);

			var feedSubmissionCallbacks = new List<FeedSubmissionCallback>
		    {
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer){ Id = 2 },
				new FeedSubmissionCallback(new Callback("", "", "", ""), serializedPropertiesContainer)
				{
					Id = 1,
					AmazonRegion = AmazonRegion.Europe,
					TypeName = "testTypeName",
					DataTypeName = "testDataTypeName",
					MethodName = "testMethodName",
					Data = "testData",
					FeedSubmissionData = JsonConvert.SerializeObject(propertiesContainer)
				}
			};

		    _feedSubmissionCallbackRepoMock = new Mock<IFeedSubmissionCallbackRepo>();
		    _feedSubmissionCallbackRepoMock.Setup(x => x.GetAll()).Returns(feedSubmissionCallbacks.AsQueryable());
		    _feedSubmissionCallbackService = new FeedSubmissionCallbackService(_feedSubmissionCallbackRepoMock.Object);
		}

	    [Test]
	    public void FirstOrDefault_TwoInQueue_ReturnsFirstObjectPopulatedWithCorrectData()
	    {
			var feedSubmissionCallback = _feedSubmissionCallbackService.FirstOrDefault();
		    var feedSubmissionData = JsonConvert.DeserializeObject<FeedSubmissionPropertiesContainer>(feedSubmissionCallback.FeedSubmissionData);

		    Assert.AreEqual(AmazonRegion.Europe, feedSubmissionCallback.AmazonRegion);
		    Assert.AreEqual("testData", feedSubmissionCallback.Data);
		    Assert.AreEqual("testMethodName", feedSubmissionCallback.MethodName);
		    Assert.AreEqual("testTypeName", feedSubmissionCallback.TypeName);
		    Assert.AreEqual("testDataTypeName", feedSubmissionCallback.DataTypeName);
		    Assert.AreEqual("testFeedType", feedSubmissionData.FeedType);
		    Assert.AreEqual("testFeedContent", feedSubmissionData.FeedContent);
			CollectionAssert.AreEquivalent(new List<string>(MwsMarketplaceGroup.AmazonEurope()), feedSubmissionData.MarketplaceIdList);
		}
	}
}
