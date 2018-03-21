using System;
using System.IO;

namespace MountainWarehouse.EasyMWS.Processors
{
	internal interface IQueueingProcessor<T>
	{
		void Poll();
		void Queue(T propertiesContainer, Action<Stream, object> callbackMethod, object callbackData);
	}
}
