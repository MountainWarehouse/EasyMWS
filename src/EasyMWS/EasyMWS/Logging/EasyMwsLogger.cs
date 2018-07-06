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

			var messageObject = new
			{
				Application = "EasyMws",
				Message = message,
				RequestInfo = requestInfo
			};

			var eventArgs = new LogAvailableEventArgs(level,
				$"{JsonConvert.SerializeObject(messageObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None})}")
			{
				RequestInfo = requestInfo
			};

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

			RequestInfo requestInfo = null;
			var messageObject = new
			{
				Application = string.Empty,
				Message = string.Empty,
				RequestInfo = requestInfo,
			};

			if (e is MarketplaceWebServiceException mwsWebServiceException)
			{
				requestInfo = new RequestInfo(
					mwsWebServiceException.ResponseHeaderMetadata?.Timestamp,
					mwsWebServiceException.RequestId,
					mwsWebServiceException.StatusCode,
					mwsWebServiceException.ErrorType,
					mwsWebServiceException.ErrorCode);

				messageObject = new
				{
					Application = "EasyMws",
					Message = message,
					RequestInfo = requestInfo
				};
			}
			else
			{
				messageObject = new
				{
					Application = "EasyMws",
					Message = $"{message} {e.Message}",
					RequestInfo = requestInfo
				};
			}
			

			var eventArgs = new LogAvailableEventArgs(LogLevel.Error, $"{JsonConvert.SerializeObject(messageObject, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None })}", e)
				{
					RequestInfo = requestInfo
				};

			EventHandler<LogAvailableEventArgs> handler = LogAvailable;
			handler?.Invoke(this, eventArgs);
		}

	}
}
