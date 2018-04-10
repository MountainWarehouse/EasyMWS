using System.IO;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.CallbackLogic;
using NUnit.Framework;

namespace EasyMWS.Tests
{
	[TestFixture]
	[NonParallelizable] // Uses a static method and static variables
	public class CallbackActivatorTests
	{
		private static Stream _passedStream;
		private static bool _called;
		private static CallbackDataTest _callbackData;

		private CallbackActivator _callbackActivator;

		public struct CallbackDataTest
		{
			public string Foo;
		}

		[SetUp]
		public void SetUp()
		{
			_callbackActivator = new CallbackActivator();

			// Set up data that will be set by the static method activated.
			_called = false;
			_passedStream = null;
			_callbackData = default(CallbackDataTest);
		}

		[Test]
		public void SerializeCommand_GeneratesData()
		{
			var callback = _callbackActivator.SerializeCallback(TestMethod, new CallbackDataTest {Foo = "Bar"});

			Assert.IsNotNull(callback.TypeName);
			Assert.IsNotNull(callback.MethodName);
			Assert.IsNotNull(callback.Data);
		}

		[Test]
		public void CallMethod_CallsMethod()
		{
			_callbackActivator.CallMethod(
				new Callback(
					"EasyMWS.Tests.CallbackActivatorTests, EasyMWS.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
					"TestMethod",
					"{\"Foo\":\"123\"}",
					"EasyMWS.Tests.CallbackActivatorTests+CallbackDataTest, EasyMWS.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"),
				new MemoryStream()
				);
			Assert.IsTrue(_called);
			Assert.AreEqual("123", _callbackData.Foo);
		}

		[Test]
		public void SerializeDeserializeTest()
		{
			var serialized = _callbackActivator.SerializeCallback(TestMethod, new CallbackDataTest { Foo = "bar" });

			_callbackActivator.CallMethod(serialized, new MemoryStream());
			Assert.IsTrue(_called);
			Assert.AreEqual("bar", _callbackData.Foo);
		}


		public static void TestMethod(Stream stream, object callbackData)
		{
			_called = true;
			if (callbackData is CallbackDataTest test)
			{
				_callbackData = test;
			}
			
		}


	}
}
