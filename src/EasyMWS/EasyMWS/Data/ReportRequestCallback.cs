using System;
using System.ComponentModel.DataAnnotations;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Data
{
    internal class ReportRequestCallback
    {
	    [Key]
	    internal int Id { get; set; }
	    internal string TypeName { get; set; }
	    internal string MethodName { get; set; }
	    internal string Data { get; set; }
	    internal string DataTypeName { get; set; }


	    internal AmazonRegion AmazonRegion { get; set; }
	    internal ContentUpdateFrequency ContentUpdateFrequency { get; set; }
	    internal DateTime? LastRequested { get; set; }
	    internal string ReportRequestData { get; set; }

		/// <summary>The ID that Amazon has given us for this requested report</summary>
		internal string RequestReportId { get; set; }

		/// <summary>The ID that Amazon gives us when the report has been generated (required to download the report)</summary>
		internal string GeneratedReportId { get; set; }

	    internal ReportRequestCallback()
	    {
	    }

	    internal ReportRequestCallback(Callback callback) => (TypeName, MethodName, Data, DataTypeName) =
		    (callback.TypeName, callback.MethodName, callback.Data, callback.DataTypeName);
    }
}
