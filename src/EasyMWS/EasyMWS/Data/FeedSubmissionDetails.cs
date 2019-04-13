namespace MountainWarehouse.EasyMWS.Data
{
    public class FeedSubmissionDetails
    {
		public byte[] FeedContent { get; set; }
		public byte[] FeedSubmissionReport { get; set; }

	    public int FeedSubmissionEntryId { get; set; }

	    public virtual FeedSubmissionEntry FeedSubmissionEntry { get; set; }
    }
}
