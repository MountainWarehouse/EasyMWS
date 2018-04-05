using System;
using System.Collections.Generic;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Model;
using NUnit.Framework;

namespace EasyMWS.Tests.Model
{
    public class FeedSubmissionPropertiesContainerTests
    {
	    [Test]
	    public void FeedSubmissionPropertiesContainer_InitializationWithEmptyFeedContent_ThrowsException()
	    {
		    Assert.Throws<ArgumentException>((() =>
			    new FeedSubmissionPropertiesContainer("", "test")));
	    }

	    [Test]
	    public void FeedSubmissionPropertiesContainer_InitializationWithNullFeedContent_ThrowsException()
	    {
		    Assert.Throws<ArgumentException>((() =>
			    new FeedSubmissionPropertiesContainer(null, "test")));
	    }

	    [Test]
	    public void FeedSubmissionPropertiesContainer_InitializationWithEmptyFeedType_ThrowsException()
	    {
		    Assert.Throws<ArgumentException>((() =>
			    new FeedSubmissionPropertiesContainer("test", "")));
	    }

	    [Test]
	    public void FeedSubmissionPropertiesContainer_InitializationWithNullFeedType_ThrowsException()
	    {
		    Assert.Throws<ArgumentException>((() =>
			    new FeedSubmissionPropertiesContainer("test", null)));
	    }
	}
}
