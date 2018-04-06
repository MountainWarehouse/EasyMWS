using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Model
{
    public class ReportRequestPropertiesContainerTests
    {
	    private AmazonRegion _region = AmazonRegion.Europe;
	    private static bool _called;
	    private CallbackActivator _callbackActivator;
	    private static ReportRequestPropertiesContainer _callbackData;

		[SetUp]
	    public void SetUp()
	    {
		    _callbackActivator = new CallbackActivator();
			_called = false;
			_callbackData = new ReportRequestPropertiesContainer("testReportType", ContentUpdateFrequency.Daily, new List<string>());
		}

	    [Test]
	    public void ReportRequestPropertiesContainer_InitializationWithEmptyReportType_ThrowsException()
	    {
		    Assert.Throws<ArgumentException>((() =>
			    new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown)));
	    }

	    [Test]
	    public void ReportRequestPropertiesContainer_InitializationWithNullReportType_ThrowsException()
	    {
		    Assert.Throws<ArgumentException>((() =>
			    new ReportRequestPropertiesContainer(null, ContentUpdateFrequency.Unknown)));
	    }

		[Test]
	    public void ReportRequestPropertiesContainer_IsSerializedAndDeserialized_AsExpected()
	    {
		    var testReportType = "test report type 123";
		    var testMerchant = "test merchant 123";
		    var testMwsAuthToken = "test auth token 123";
			var marketplaceIdList = new List<string>{"asdf1234", "tyui5678", "vbnm4567"};
		    ContentUpdateFrequency testUpdateFrequency = ContentUpdateFrequency.Daily;
		    var propertiesContainer = new ReportRequestPropertiesContainer(testReportType, testUpdateFrequency, marketplaceIdList);
		    var writeStream = new MemoryStream();
		    IFormatter formatter = new BinaryFormatter();

			formatter.Serialize(writeStream, propertiesContainer);

		    var serializedData = writeStream.ToArray();
		    var readStream = new MemoryStream(Convert.FromBase64String(Convert.ToBase64String(serializedData)));

		    ReportRequestPropertiesContainer deserializedPropertiesContainer = (ReportRequestPropertiesContainer) formatter.Deserialize(readStream);

			Assert.AreEqual(propertiesContainer.ReportType, deserializedPropertiesContainer.ReportType);
		    Assert.AreEqual(propertiesContainer.UpdateFrequency, deserializedPropertiesContainer.UpdateFrequency);
			CollectionAssert.AreEqual(propertiesContainer.MarketplaceIdList, deserializedPropertiesContainer.MarketplaceIdList);
		}

		[Test]
	    public void ReportRequestPropertiesContainer_IsSerializedAndDeserializedProperly_UsingCallbackActivator()
	    {
		    var testReportType = "test report type 123";
		    var testMerchant = "test merchant 123";
		    var testMwsAuthToken = "test auth token 123";
		    var marketplaceIdList = new List<string> { "asdf1234", "tyui5678", "vbnm4567" };
		    ContentUpdateFrequency testUpdateFrequency = ContentUpdateFrequency.Daily;
		    var propertiesContainer = new ReportRequestPropertiesContainer(testReportType, testUpdateFrequency, marketplaceIdList);

			var serialized = _callbackActivator.SerializeCallback(TestMethod, propertiesContainer);
		    _callbackActivator.CallMethod(serialized, new MemoryStream());
		    Assert.IsTrue(_called);

			Assert.AreEqual(testReportType, _callbackData.ReportType);
		    Assert.AreEqual(testUpdateFrequency, _callbackData.UpdateFrequency);
		    CollectionAssert.AreEqual(marketplaceIdList, _callbackData.MarketplaceIdList);
		}

	    public static void TestMethod(Stream stream, object callbackData)
	    {
		    _called = true;
		    if (callbackData is ReportRequestPropertiesContainer test)
		    {
			    _callbackData = test;
		    }

	    }
	}
}
