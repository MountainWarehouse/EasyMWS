using System;
using MountainWarehouse.EasyMWS.Enums;

namespace MountainWarehouse.EasyMWS.Logging
{
	public interface IEasyMwsLogger
	{
		event EventHandler<LogAvailableEventArgs> LogAvailable;

		void Log(LogLevel level, string message, RequestInfo includeRequestInfo = null, Exception ex = null);
		void Debug(string message, RequestInfo includeRequestInfo = null);
		void Info(string message, RequestInfo includeRequestInfo = null);
		void Warn(string message, RequestInfo includeRequestInfo = null);
		void Warn(string message, Exception e);
		void Error(string message, Exception e);
	}
}
