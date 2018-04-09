using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Client
{
    public class ReportDownloadedEventArgs
    {
	    public Stream ReportContent { get; set; }
	    public AmazonRegion AmazonRegion { get; set; }
	    public string MerchantId { get; set; }
	    public string GeneratedReportId { get; set; }
		public string ReportType { get; set; }
    }
}
