using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS;
using NUnit.Framework;

namespace EasyMWS.Tests
{
	[TestFixture]
	public class CallbackActivatorTests
	{
		private Stream _passedStream;
		private bool _called;
		private CallbackActivator _callbackActivator;

		[SetUp]
		public void SetUp()
		{
			_callbackActivator = new CallbackActivator();

			// Set up data that will be set by the static method activated.
			_called = false;
			_passedStream = null;
		}

		[Test]
		public void SerializeCommand_GeneratesData()
		{

		}




		public static void TestMethod()
		{

		}


	}
}
