using System.IO;
using MountainWarehouse.EasyMWS.Data;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IFeedProcessor
	{
		void CleanUpFeedSubmissionQueue();
		void SubmitNextFeedInQueueToAmazon();
		void RequestFeedSubmissionStatusesFromAmazon();
		(FeedSubmissionCallback feedSubmissionCallback, Stream reportContent) RequestNextFeedSubmissionInQueueFromAmazon();
		void ExecuteCallback(FeedSubmissionCallback feedSubmissionCallback, Stream stream);
	}
}
