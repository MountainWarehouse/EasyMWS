using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MountainWarehouse.EasyMWS.Data
{
    public class FeedSubmissionDetails
    {
	    public string FeedContent { get; set; }

		[Key, ForeignKey("FeedSubmissionEntry")]
	    public int FeedSubmissionEntryId { get; set; }

	    public FeedSubmissionEntry FeedSubmissionEntry { get; set; }
    }
}
