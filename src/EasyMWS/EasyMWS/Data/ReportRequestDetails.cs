namespace MountainWarehouse.EasyMWS.Data
{
    public class ReportRequestDetails
    {
	    public byte[] ReportContent { get; set; }

	    public int ReportRequestEntryId { get; set; }

	    public virtual ReportRequestEntry ReportRequestEntry { get; set; }
    }
}
