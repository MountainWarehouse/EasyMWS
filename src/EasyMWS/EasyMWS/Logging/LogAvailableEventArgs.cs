using System;
using MountainWarehouse.EasyMWS.Enums;


namespace MountainWarehouse.EasyMWS.Logging
{
	public class LogAvailableEventArgs
	{
		public string Message { get; set; }
		public LogLevel Level { get; set; }
		public Exception Exception { get; set; }

		public LogAvailableEventArgs(LogLevel level, string message) => (Level, Message, Exception) = (level, message, null);
		public LogAvailableEventArgs(LogLevel level, string message, Exception exception) => (Level, Message, Exception) = (level, message, exception);
	}
}
