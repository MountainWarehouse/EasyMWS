using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.ReportProcessors
{
    internal interface IFeedSubmissionProcessor
    {
	    FeedSubmissionCallback GetNextFeedToSubmitFromQueue();
    }
}
