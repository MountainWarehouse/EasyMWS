using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MountainWarehouse.EasyMWS.Enums;
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


	    public AmazonRegion AmazonRegion { get; set; }
	    public ContentUpdateFrequency ContentUpdateFrequency { get; set; }
	    public DateTime? LastRequested { get; set; }
	    public string ReportRequestData { get; set; }

	    /// <summary>The ID that Amazon has given us for this requested report</summary>
	    public string RequestReportId { get; set; }

	    /// <summary>The ID that Amazon gives us when the report has been generated (required to download the report)</summary>
		public string GeneratedReportId { get; set; }

		public ReportRequestCallback()
	    {
	    }

	    public ReportRequestCallback(Callback callback) => (TypeName, MethodName, Data, DataTypeName) =
		    (callback.TypeName, callback.MethodName, callback.Data, callback.DataTypeName);
    }
}
