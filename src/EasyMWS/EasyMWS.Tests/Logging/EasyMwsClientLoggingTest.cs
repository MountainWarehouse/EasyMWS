using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using NUnit.Framework;

namespace EasyMWS.Tests.Logging
{
    public class EasyMwsClientLoggingTest
    {
	    private EasyMwsClient _easyMwsClient;
	    private IEasyMwsLogger _logger;

	    [SetUp]
	    public void Setup()
	    {
		    _logger = new EasyMwsLogger();
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "", "", "", _logger, EasyMwsOptions.Defaults);
	    }

	    [Test]
	    public void IfLoggingIsEnabled_Poll_LogsAtLeastOneMessage()
	    {
		    var isAtLeastOneMessageLogged = false;
			_logger.LogAvailable += (sender, args) => { isAtLeastOneMessageLogged = true; };

			_easyMwsClient.Poll();

			Assert.IsTrue(isAtLeastOneMessageLogged);
		}

	    [Test]
	    public void IfLoggingIsEnabled_PQueueReport_LogsAtLeastOneMessage()
	    {
		    var isAtLeastOneMessageLogged = false;
		    _logger.LogAvailable += (sender, args) => { isAtLeastOneMessageLogged = true; };

		    _easyMwsClient.QueueReport(new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown),
			    (stream, o) => { }, new object());

		    Assert.IsTrue(isAtLeastOneMessageLogged);
		}

		[Test]
	    public void IfLoggingIsDisabled_Poll_DoesNotLogAMessage()
	    {
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "", "", "", null, EasyMwsOptions.Defaults);
		    var isAtLeastOneMessageLogged = false;
		    _logger.LogAvailable += (sender, args) => { isAtLeastOneMessageLogged = true; };

		    _easyMwsClient.Poll();

		    Assert.IsFalse(isAtLeastOneMessageLogged);
		}

	    [Test]
	    public void IfLoggingIsDisabled_Queue_DoesNotLogAMessage()
	    {
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "", "", "", null, EasyMwsOptions.Defaults);
		    var isAtLeastOneMessageLogged = false;
		    _logger.LogAvailable += (sender, args) => { isAtLeastOneMessageLogged = true; };

		    _easyMwsClient.QueueReport(new ReportRequestPropertiesContainer("", ContentUpdateFrequency.Unknown),
			    (stream, o) => { }, new object());

		    Assert.IsFalse(isAtLeastOneMessageLogged);
		}
	}
}
