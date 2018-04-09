using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Client;
using MountainWarehouse.EasyMWS.Enums;
using MountainWarehouse.EasyMWS.Helpers;
using MountainWarehouse.EasyMWS.Logging;
using MountainWarehouse.EasyMWS.Model;
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
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "test", "test", "test", _logger, EasyMwsOptions.Defaults());
		}

		[Test]
		public void IfLoggingIsEnabled_Poll_LogsAtLeastOneMessage()
		{
			var isAtLeastOneMessageLogged = false;
			_logger.LogAvailable += (sender, args) => { isAtLeastOneMessageLogged = true; };

			_easyMwsClient.Poll();

			Assert.True(isAtLeastOneMessageLogged);
		}

		[Test]
		public void IfLoggingIsDisabled_Poll_DoesNotLogAMessage()
		{
			_easyMwsClient = new EasyMwsClient(AmazonRegion.Europe, "test", "test", "test", null, EasyMwsOptions.Defaults());
			var isAtLeastOneMessageLogged = false;
			_logger.LogAvailable += (sender, args) => { isAtLeastOneMessageLogged = true; };

			_easyMwsClient.Poll();

			Assert.IsFalse(isAtLeastOneMessageLogged);
		}
	}
}
