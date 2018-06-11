using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainWarehouse.EasyMWS.Data
{
    public class ReportRequestDetails
    {
	    public byte[] ReportContent { get; set; }

		[Key, ForeignKey("ReportRequestEntry")]
	    public int ReportRequestEntryId { get; set; }

	    public virtual ReportRequestEntry ReportRequestEntry { get; set; }
    }
}
