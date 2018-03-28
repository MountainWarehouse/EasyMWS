using System;
using MountainWarehouse.EasyMWS.Enums;


namespace MountainWarehouse.EasyMWS.Logging
{
	public class LogAvailableEventArgs
	{
		public string Message { get; set; }
		public LogLevel Level { get; set; }
		public Exception Exception { get; set; }
		public bool HasRequestInfo => RequestInfo != null;
		public RequestInfo RequestInfo { get; set; }

		public LogAvailableEventArgs()
		{
		}

		public LogAvailableEventArgs(LogLevel level, string message) => (Level, Message, Exception, RequestInfo) = (level, message, null, null);
		public LogAvailableEventArgs(LogLevel level, string message, Exception exception) => (Level, Message, Exception) = (level, message, exception);
	}
}
