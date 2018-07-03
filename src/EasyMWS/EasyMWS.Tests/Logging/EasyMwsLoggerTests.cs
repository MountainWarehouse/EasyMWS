using System;
using System.Net;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService;
using MountainWarehouse.EasyMWS.WebService.MarketplaceWebService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace EasyMWS.Tests.Logging
{
    public class EasyMwsLoggerTests
    {
	    private string DeserializeInternalMessage(string serializedMessage)
	    {
		    var deserializedMessage = ((JObject)JsonConvert.DeserializeObject(serializedMessage));
		    return (string)((JContainer)deserializedMessage)["Message"];
	    }

	    [Test]
	    public void Calling_ActionThatLogsInfoWithRequestInfo_Triggers_LogAvailableEventWithExpectedArguments()
	    {
			var logger = new EasyMwsLogger();
		    LogAvailableEventArgs logAvailableArgs = null;
		    logger.LogAvailable += (sender, args) => { logAvailableArgs = args; };
		    var testLogsPublisher = new TestEasyMwsLogPublisher(logger);

			testLogsPublisher.ActionThatLogsInfoWithRequestInfo();

			Assert.IsNotNull(logAvailableArgs);
		    Assert.IsNotNull(logAvailableArgs.Message);

		    var actualLogMessage = DeserializeInternalMessage(logAvailableArgs.Message);

			Assert.AreEqual("testMessage_ActionThatLogsInfoWithRequestInfo", actualLogMessage);
		    Assert.AreEqual(LogLevel.Info, logAvailableArgs.Level);
			Assert.IsTrue(logAvailableArgs.HasRequestInfo);
		    Assert.IsNotNull( logAvailableArgs.RequestInfo);
			Assert.AreEqual("testTimestamp1", logAvailableArgs.RequestInfo.Timestamp);
		    Assert.AreEqual("testRequestId1", logAvailableArgs.RequestInfo.RequestId);
		    Assert.AreEqual(HttpStatusCode.OK, logAvailableArgs.RequestInfo.StatusCode);
			Assert.IsNull(logAvailableArgs.RequestInfo.ErrorCode);
		    Assert.IsNull(logAvailableArgs.RequestInfo.ErrorType);
		}

	    [Test]
	    public void Calling_ActionThatLogsInfoWithoutRequestInfo_Triggers_LogAvailableEventWithExpectedArguments()
	    {
		    var logger = new EasyMwsLogger();
		    LogAvailableEventArgs logAvailableArgs = null;
		    logger.LogAvailable += (sender, args) => { logAvailableArgs = args; };
		    var testLogsPublisher = new TestEasyMwsLogPublisher(logger);

		    testLogsPublisher.ActionThatLogsInfoWithoutRequestInfo();

		    Assert.IsNotNull(logAvailableArgs);
		    Assert.IsNotNull(logAvailableArgs.Message);

		    var actualLogMessage = DeserializeInternalMessage(logAvailableArgs.Message);

		    Assert.AreEqual("testMessage_ActionThatLogsInfoWithoutRequestInfo", actualLogMessage);
		    Assert.AreEqual(LogLevel.Info, logAvailableArgs.Level);
		    Assert.IsFalse(logAvailableArgs.HasRequestInfo);
		    Assert.IsNull(logAvailableArgs.RequestInfo);
	    }

	    [Test]
	    public void Calling_ActionThatLogsWarningWithRequestInfo_Triggers_LogAvailableEventWithExpectedArguments()
	    {
		    var logger = new EasyMwsLogger();
		    LogAvailableEventArgs logAvailableArgs = null;
		    logger.LogAvailable += (sender, args) => { logAvailableArgs = args; };
		    var testLogsPublisher = new TestEasyMwsLogPublisher(logger);

		    testLogsPublisher.ActionThatLogsWarningWithRequestInfo();

		    Assert.IsNotNull(logAvailableArgs);
		    Assert.IsNotNull(logAvailableArgs.Message);

		    var actualLogMessage = DeserializeInternalMessage(logAvailableArgs.Message);

		    Assert.AreEqual("testMessage_ActionThatLogsWarningWithRequestInfo", actualLogMessage);
		    Assert.AreEqual(LogLevel.Warn, logAvailableArgs.Level);
		    Assert.IsTrue(logAvailableArgs.HasRequestInfo);
		    Assert.IsNotNull(logAvailableArgs.RequestInfo);
		    Assert.AreEqual("testTimestamp2", logAvailableArgs.RequestInfo.Timestamp);
		    Assert.AreEqual("testRequestId2", logAvailableArgs.RequestInfo.RequestId);
		    Assert.AreEqual(HttpStatusCode.Continue, logAvailableArgs.RequestInfo.StatusCode);
		    Assert.IsNull(logAvailableArgs.RequestInfo.ErrorCode);
		    Assert.IsNull(logAvailableArgs.RequestInfo.ErrorType);
	    }

	    [Test]
	    public void Calling_ActionThatLogsWarningWithoutRequestInfo_Triggers_LogAvailableEventWithExpectedArguments()
	    {
		    var logger = new EasyMwsLogger();
		    LogAvailableEventArgs logAvailableArgs = null;
		    logger.LogAvailable += (sender, args) => { logAvailableArgs = args; };
		    var testLogsPublisher = new TestEasyMwsLogPublisher(logger);

		    testLogsPublisher.ActionThatLogsWarningWithoutRequestInfo();

		    Assert.IsNotNull(logAvailableArgs);
		    Assert.IsNotNull(logAvailableArgs.Message);

		    var actualLogMessage = DeserializeInternalMessage(logAvailableArgs.Message);

		    Assert.AreEqual("testMessage_ActionThatLogsWarningWithoutRequestInfo", actualLogMessage);
		    Assert.AreEqual(LogLevel.Warn, logAvailableArgs.Level);
		    Assert.IsFalse(logAvailableArgs.HasRequestInfo);
		    Assert.IsNull(logAvailableArgs.RequestInfo);
	    }

	    [Test]
	    public void Calling_ActionThatLogsStandardException_Triggers_LogAvailableEventWithExpectedArguments()
	    {
		    var logger = new EasyMwsLogger();
		    LogAvailableEventArgs logAvailableArgs = null;
		    logger.LogAvailable += (sender, args) => { logAvailableArgs = args; };
		    var testLogsPublisher = new TestEasyMwsLogPublisher(logger);

		    testLogsPublisher.ActionThatLogsStandardException();

		    Assert.IsNotNull(logAvailableArgs);
		    Assert.IsNotNull(logAvailableArgs.Message);

		    var actualLogMessage = DeserializeInternalMessage(logAvailableArgs.Message);

		    Assert.AreEqual("testMessage_ActionThatLogsStandardException test exception message", actualLogMessage);
		    Assert.AreEqual(LogLevel.Error, logAvailableArgs.Level);
		    Assert.IsFalse(logAvailableArgs.HasRequestInfo);
		    Assert.IsNull(logAvailableArgs.RequestInfo);
	    }

	    [Test]
	    public void Calling_ActionThatLogsMarketplaceWebServiceException_Triggers_LogAvailableEventWithExpectedArguments()
	    {
		    var logger = new EasyMwsLogger();
		    LogAvailableEventArgs logAvailableArgs = null;
		    logger.LogAvailable += (sender, args) => { logAvailableArgs = args; };
		    var testLogsPublisher = new TestEasyMwsLogPublisher(logger);

		    testLogsPublisher.ActionThatLogsMarketplaceWebServiceException();

		    Assert.IsNotNull(logAvailableArgs);
		    Assert.IsNotNull(logAvailableArgs.Message);

		    var actualLogMessage = DeserializeInternalMessage(logAvailableArgs.Message);

		    Assert.AreEqual("testMessage_ActionThatLogsMarketplaceWebServiceException", actualLogMessage);
		    Assert.AreEqual(LogLevel.Error, logAvailableArgs.Level);
		    Assert.IsTrue(logAvailableArgs.HasRequestInfo);
		    Assert.IsNotNull(logAvailableArgs.RequestInfo);
		    Assert.AreEqual("testTimestamp123", logAvailableArgs.RequestInfo.Timestamp);
		    Assert.AreEqual("testRequestId123", logAvailableArgs.RequestInfo.RequestId);
		    Assert.AreEqual(HttpStatusCode.Conflict, logAvailableArgs.RequestInfo.StatusCode);
		    Assert.AreEqual("testErrorCode", logAvailableArgs.RequestInfo.ErrorCode);
		    Assert.AreEqual("testErrorType", logAvailableArgs.RequestInfo.ErrorType);
	    }
	}

	public class TestEasyMwsLogPublisher
	{
		private IEasyMwsLogger _logger;
		public TestEasyMwsLogPublisher(IEasyMwsLogger logger)
		{
			_logger = logger;
		}

		public void ActionThatLogsInfoWithRequestInfo()
		{
			_logger.Info("testMessage_ActionThatLogsInfoWithRequestInfo", new RequestInfo("testTimestamp1", "testRequestId1", HttpStatusCode.OK));
		}

		public void ActionThatLogsInfoWithoutRequestInfo()
		{
			_logger.Info("testMessage_ActionThatLogsInfoWithoutRequestInfo");
		}

		public void ActionThatLogsWarningWithRequestInfo()
		{
			_logger.Warn("testMessage_ActionThatLogsWarningWithRequestInfo", new RequestInfo("testTimestamp2", "testRequestId2", HttpStatusCode.Continue));
		}

		public void ActionThatLogsWarningWithoutRequestInfo()
		{
			_logger.Warn("testMessage_ActionThatLogsWarningWithoutRequestInfo");
		}

		public void ActionThatLogsStandardException()
		{
			try
			{
				throw new AccessViolationException("test exception message");
			}
			catch (Exception e)
			{
				_logger.Error("testMessage_ActionThatLogsStandardException", e);
			}
		}

		public void ActionThatLogsMarketplaceWebServiceException()
		{
			try
			{
				throw new MarketplaceWebServiceException("testMessage_someInternalExceptionMessage",
					HttpStatusCode.Conflict, "testErrorCode", "testErrorType", "testRequestId123", null,
					new ResponseHeaderMetadata("testRequestId123", null, "testTimestamp123"));
			}
			catch (Exception e)
			{
				_logger.Error("testMessage_ActionThatLogsMarketplaceWebServiceException", e);
			}
		}
	}
}

