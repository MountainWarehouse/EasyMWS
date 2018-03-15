using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS
{
	internal class CallbackActivator
	{

		internal Callback SerializeCallback(
			Action<Stream, object> callbackMethod, object callbackData)
		{
			var type = callbackMethod.Method.DeclaringType;
			var method = callbackMethod.Method;
			var dataType = callbackData.GetType();

			return new Callback(type.AssemblyQualifiedName,
				method.Name,
				JsonConvert.SerializeObject(callbackData),
				dataType.AssemblyQualifiedName);
		}

		internal void CallMethod(Callback callback, Stream stream)
		{
			var type = Type.GetType(callback.TypeName);
			var method = type.GetMethods().First(mi => mi.Name == callback.MethodName);
			var dataType = Type.GetType(callback.DataTypeName);

			method.Invoke(null, new object[] { stream, JsonConvert.DeserializeObject(callback.Data, dataType) });
		}


	}
}