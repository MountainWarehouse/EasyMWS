using System;
using System.ComponentModel.DataAnnotations;
using MountainWarehouse.EasyMWS.Helpers;

namespace MountainWarehouse.EasyMWS.Data
{
    public class ReportRequestCallback
    {
	    [Key]
	    public int Id { get; set; }
	    public string TypeName { get; set; }
	    public string MethodName { get; set; }
	    public string Data { get; set; }
	    public string DataTypeName { get; set; }


	    public string ReportType { get; set; }
	    public AmazonRegion AmazonRegion { get; set; }
	    public ContentUpdateFrequency ContentUpdateFrequency { get; set; }
	    public DateTime? LastRequested { get; set; }
	}
}
