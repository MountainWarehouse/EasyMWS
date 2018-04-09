using System;
using MountainWarehouse.EasyMWS.Client;

namespace MountainWarehouse.EasyMWS.Processors
{
    internal interface IFeedProcessor
    {
	    event EventHandler<FeedSubmittedEventArgs> FeedSubmitted;
	}
}
