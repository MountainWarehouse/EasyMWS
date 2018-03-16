using System;

namespace MountainWarehouse.EasyMWS.Logging
{
    public interface IEasyMwsLogger
    {
	    event EventHandler<LogAvailableEventArgs> LogAvailable;

		void Log(LogLevel level, string message);
	    void Info(string message);
	    void Warn(string message);
	    void Error(string message, Exception e);
	}
}
