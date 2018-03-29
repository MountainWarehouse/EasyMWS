using System;
using System.ComponentModel.DataAnnotations;

namespace MountainWarehouse.EasyMWS.Data
{
    public class AmazonReport
    {
		[Key]
	    public int Id { get; set; }
	    public string DownloadRequestId { get; set; }
	    public string DownloadTimestamp { get; set; }
	    public string Content { get; set; }
	    public string ReportType { get; set; }
	    public DateTime DateCreated { get; set; }
    }
}
