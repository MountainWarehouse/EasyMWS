using System;
using System.Collections.Generic;
using System.IO;
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

    }
}
