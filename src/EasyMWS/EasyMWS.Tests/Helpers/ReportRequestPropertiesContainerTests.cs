using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class ReportRequestPropertiesContainerTests
    {
	    [Test]
	    public void ReportRequestPropertiesContainer_IsSerializedAndDeserialized_AsExpected()
	    {
		    var testReportType = "test report type 123";
		    var testMerchant = "test merchant 123";
		    var testMwsAuthToken = "test auth token 123";
			var marketplaceIdList = new List<string>{"asdf1234", "tyui5678", "vbnm4567"};
		    ContentUpdateFrequency testUpdateFrequency = ContentUpdateFrequency.Daily;
		    var propertiesContainer = new ReportRequestPropertiesContainer(testReportType, testMerchant, testMwsAuthToken, testUpdateFrequency, marketplaceIdList);
		    var writeStream = new MemoryStream();
		    IFormatter formatter = new BinaryFormatter();

			formatter.Serialize(writeStream, propertiesContainer);

		    var serializedData = writeStream.ToArray();
		    var readStream = new MemoryStream(Convert.FromBase64String(Convert.ToBase64String(serializedData)));

		    ReportRequestPropertiesContainer deserializedPropertiesContainer = (ReportRequestPropertiesContainer) formatter.Deserialize(readStream);

			Assert.AreEqual(propertiesContainer.ReportType, deserializedPropertiesContainer.ReportType);
		    Assert.AreEqual(propertiesContainer.Merchant, deserializedPropertiesContainer.Merchant);
		    Assert.AreEqual(propertiesContainer.MwsAuthToken, deserializedPropertiesContainer.MwsAuthToken);
		    Assert.AreEqual(propertiesContainer.UpdateFrequency, deserializedPropertiesContainer.UpdateFrequency);
			CollectionAssert.AreEqual(propertiesContainer.MarketplaceIdList, deserializedPropertiesContainer.MarketplaceIdList);
		}
	}
}
