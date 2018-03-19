using System;

namespace MountainWarehouse.EasyMWS.Logging
{
    public class EasyMwsLogger : IEasyMwsLogger
    {
		public event EventHandler<LogAvailableEventArgs> LogAvailable;
	    public void Log(LogLevel level, string message)
	    {
		    EventHandler<LogAvailableEventArgs> handler = LogAvailable;
		    handler?.Invoke(this, new LogAvailableEventArgs(level, message));
	    }

	    public void Info(string message)
	    {
		    Log(LogLevel.Info, message);
	    }

	    public void Warn(string message)
	    {
		    Log(LogLevel.Warn, message);
	    }

	    public void Error(string message, Exception e)
	    {
		    EventHandler<LogAvailableEventArgs> handler = LogAvailable;
		    handler?.Invoke(this, new LogAvailableEventArgs(LogLevel.Error, message, e));
	    }
	}
}
