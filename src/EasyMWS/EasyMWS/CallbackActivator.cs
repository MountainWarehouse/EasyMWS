using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MountainWarehouse.EasyMWS
{
	public class CallbackActivator
	{

		public (string typeName, string methodName, string callbackData) SerializeCallback(
			Action<Stream, object> callbackMethod, object callbackData)
		{
			var type = callbackMethod.Method.DeclaringType;
			var method = callbackMethod.Method;

			return (type.AssemblyQualifiedName, method.Name, null);
		}

		public void CallMethod(string typeName, string methodName)
		{
			var type = Type.GetType(typeName);
			var method = type.GetMethods().First(mi => mi.Name == methodName);

			method.Invoke(null, new object[] {null, null});
		}


	}
}