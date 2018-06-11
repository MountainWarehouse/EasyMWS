using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MountainWarehouse.EasyMWS.Data
{
    public class FeedSubmissionDetails
    {
		public byte[] FeedContent { get; set; }
		public byte[] FeedSubmissionReport { get; set; }

	    [Key, ForeignKey("FeedSubmissionEntry")]
	    public int FeedSubmissionEntryId { get; set; }

	    public virtual FeedSubmissionEntry FeedSubmissionEntry { get; set; }
    }
}
