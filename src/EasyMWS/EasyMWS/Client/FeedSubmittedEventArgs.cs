using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Client
{
    public class FeedSubmittedEventArgs
    {
	    public Stream FeedSubmissionReport { get; set; }
	    public AmazonRegion AmazonRegion { get; set; }
	    public string MerchantId { get; set; }
	    public string FeedSubmissionId { get; set; }
	    public string FeedType { get; set; }
	    public string FeedContent { get; set; }
    }
}
