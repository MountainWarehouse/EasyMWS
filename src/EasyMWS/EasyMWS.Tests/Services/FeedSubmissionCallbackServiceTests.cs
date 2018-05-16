using System.Collections.Generic;
using System.Linq;
using Moq;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Enums;
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

			var feedSubmissionCallbacks = new List<FeedSubmissionEntry>
		    {
				new FeedSubmissionEntry(serializedPropertiesContainer){ Id = 2 },
				new FeedSubmissionEntry(serializedPropertiesContainer)
				{
					Id = 1,
					AmazonRegion = AmazonRegion.Europe,
					TypeName = "testTypeName",
					DataTypeName = "testDataTypeName",
					MethodName = "testMethodName",
					Data = "testData",
					FeedType = propertiesContainer.FeedType,
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
			var feedSubmissionEntry = _feedSubmissionCallbackService.FirstOrDefault();
		    var feedSubmissionData = JsonConvert.DeserializeObject<FeedSubmissionPropertiesContainer>(feedSubmissionEntry.FeedSubmissionData);

		    Assert.AreEqual(AmazonRegion.Europe, feedSubmissionEntry.AmazonRegion);
		    Assert.AreEqual("testData", feedSubmissionEntry.Data);
		    Assert.AreEqual("testMethodName", feedSubmissionEntry.MethodName);
		    Assert.AreEqual("testTypeName", feedSubmissionEntry.TypeName);
		    Assert.AreEqual("testDataTypeName", feedSubmissionEntry.DataTypeName);
		    Assert.AreEqual("testFeedType", feedSubmissionEntry.FeedType);
			CollectionAssert.AreEquivalent(new List<string>(MwsMarketplaceGroup.AmazonEurope()), feedSubmissionData.MarketplaceIdList);
		}
	}
}
