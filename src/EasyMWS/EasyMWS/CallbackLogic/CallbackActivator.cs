using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.CallbackLogic
{
	internal class CallbackActivator : ICallbackActivator
	{

		public Callback SerializeCallback(
			Action<Stream, object> callbackMethod, object callbackData)
		{
			var type = callbackMethod.Method.DeclaringType;
			var method = callbackMethod.Method;
			var dataType = callbackData?.GetType();
			var serializedCallbackData = callbackData != null ? JsonConvert.SerializeObject(callbackData) : null;

			return new Callback(type.AssemblyQualifiedName,
				method.Name,
				serializedCallbackData,
				dataType?.AssemblyQualifiedName);
		}

		public void CallMethod(Callback callback, Stream stream)
		{
			var type = Type.GetType(callback.TypeName);
			var method = type.GetMethods().First(mi => mi.Name == callback.MethodName);
			var dataType = callback.DataTypeName != null ? Type.GetType(callback.DataTypeName) : typeof(object);
			var callbackData = callback.Data != null ? JsonConvert.DeserializeObject(callback.Data, dataType) : null;

			method.Invoke(null, new object[] { stream, callbackData });
		}


	}
}