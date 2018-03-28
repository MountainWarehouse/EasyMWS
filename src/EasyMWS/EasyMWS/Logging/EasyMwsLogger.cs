using System;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using Newtonsoft.Json;

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
		public void Log(LogLevel level, string message, RequestInfo requestInfo = null)
		{
			if (!_isEnabled) return;

			var eventArgs =
				new LogAvailableEventArgs(level, $"{JsonConvert.SerializeObject(new {Message = message}, Formatting.None)}");

			if (requestInfo != null)
			{
				var messageObject = new
				{
					RequestInfo = requestInfo,
					Message = message
				};

				eventArgs.Message = $"{JsonConvert.SerializeObject(messageObject, Formatting.None)}";
				eventArgs.RequestInfo = requestInfo;
			}

			EventHandler<LogAvailableEventArgs> handler = LogAvailable;
			handler?.Invoke(this, eventArgs);
		}

		public void Info(string message, RequestInfo includeRequestInfo = null)
		{
			if (!_isEnabled) return;

			Log(LogLevel.Info, message, includeRequestInfo);
		}

		public void Warn(string message, RequestInfo includeRequestInfo = null)
		{
			if (!_isEnabled) return;

			Log(LogLevel.Warn, message, includeRequestInfo);
		}

		public void Error(string message, Exception e)
		{
			if (!_isEnabled) return;

			var eventArgs = new LogAvailableEventArgs(LogLevel.Error, $"{JsonConvert.SerializeObject(new { Message = message }, Formatting.None)}", e);

			if (e is MarketplaceWebServiceException mwsWebServiceException)
			{
				eventArgs.RequestInfo = new RequestInfo(
					mwsWebServiceException.ResponseHeaderMetadata.Timestamp,
					mwsWebServiceException.RequestId,
					mwsWebServiceException.StatusCode,
					mwsWebServiceException.ErrorType,
					mwsWebServiceException.ErrorCode);

				var messageObject = new
				{
					RequestInfo = eventArgs.RequestInfo,
					Message = message
				};

				eventArgs.Message = $"{JsonConvert.SerializeObject(messageObject, Formatting.None)}";
				
			}

			EventHandler<LogAvailableEventArgs> handler = LogAvailable;
			handler?.Invoke(this, eventArgs);
		}
	}
}
