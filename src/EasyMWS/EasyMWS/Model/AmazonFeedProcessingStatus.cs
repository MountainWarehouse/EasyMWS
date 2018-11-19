using System;
using System.Collections.Generic;
using System.Text;

namespace MountainWarehouse.EasyMWS.Model
{
    internal class AmazonFeedProcessingStatus
    {
        public const string Done = "_DONE_";
        public const string Cancelled = "_CANCELLED_";
        public const string AwaitingAsyncReply = "_AWAITING_ASYNCHRONOUS_REPLY_";
        public const string InProgress = "_IN_PROGRESS_";
        public const string InSafetyNet = "_IN_SAFETY_NET_";
        public const string Submitted = "_SUBMITTED_";
        public const string Unconfirmed = "_UNCONFIRMED_";
    }
}
