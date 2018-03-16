using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Helpers
{
	/// <summary>
	/// More information can be found at : https://docs.developer.amazonservices.com/en_US/feeds/Feeds_SubmitFeed.html
	/// </summary>
	public class MarketplaceContentTypeHelper
    {
	    public string GetContentTypeForXmlFiles()
	    {
		    return "text/xml";
	    }

	    public List<string> GetSupportedContentTypesForFlatFiles(AmazonRegion region)
	    {
		    if (region == AmazonRegion.NorthAmerica || region == AmazonRegion.Europe)
		    {
			    return new List<string> {"text/tab-separated-values; charset=iso-8859-1"};
		    }
		    else if (region == AmazonRegion.Japan)
		    {
			    return new List<string> {"text/tab-separated-values; charset=Shift_JIS"};
		    }
		    else if (region == AmazonRegion.China)
		    {
			    return new List<string> {"text/tab-separated-values;charset=UTF-8", "text/tab-separated-values;charset=UTF-16" };
		    }
		    else
		    {
			    throw new NotSupportedException(
				    $"Amazon region '{region.ToString()}' not supported by EasyMwsClient due to unknown ContentType that should be used to submit a feed to amazon.");
		    }
	    }
    }
}
