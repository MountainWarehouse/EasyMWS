using System;

namespace MountainWarehouse.EasyMWS.Logging
{
    public class EasyMwsLogger : IEasyMwsLogger
    {
	    private readonly bool _isEnabled = false;

	    public EasyMwsLogger() : this(true)
	    {
	    }

	    internal EasyMwsLogger(bool isEnabled)
	    {
		    _isEnabled = isEnabled;
	    }

		public event EventHandler<LogAvailableEventArgs> LogAvailable;
	    public void Log(LogLevel level, string message)
	    {
			if(!_isEnabled) return;

		    EventHandler<LogAvailableEventArgs> handler = LogAvailable;
		    handler?.Invoke(this, new LogAvailableEventArgs(level, message));
	    }

	    public void Info(string message)
	    {
		    if (!_isEnabled) return;

			Log(LogLevel.Info, message);
	    }

	    public void Warn(string message)
	    {
		    if (!_isEnabled) return;

			Log(LogLevel.Warn, message);
	    }

	    public void Error(string message, Exception e)
	    {
		    if (!_isEnabled) return;

			EventHandler<LogAvailableEventArgs> handler = LogAvailable;
		    handler?.Invoke(this, new LogAvailableEventArgs(LogLevel.Error, message, e));
	    }
	}
}
