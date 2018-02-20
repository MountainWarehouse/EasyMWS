using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MountainWarehouse.EasyMWS;
using NUnit.Framework;

namespace EasyMWS.Tests
{
	[TestFixture]
	[NonParallelizable] // Uses a static method and static variables
	public class CallbackActivatorTests
	{
		private Stream _passedStream;
		private static bool _called;
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
			var serialized = _callbackActivator.SerializeCallback(TestMethod, new {foo = "bar"});

			Assert.IsNotNull(serialized.typeName);
			Assert.IsNotNull(serialized.methodName);

		}

		[Test]
		public void CallMethod_CallsMethod()
		{
			_callbackActivator.CallMethod("EasyMWS.Tests.CallbackActivatorTests, EasyMWS.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "TestMethod");
		}


		public static void TestMethod(Stream stream, object callbackData)
		{
			_called = true;
		}


	}
}
