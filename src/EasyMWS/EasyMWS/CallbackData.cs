namespace MountainWarehouse.EasyMWS
{
	public class Callback
    {
	    internal Callback(string typeName,
		    string methodName,
		    string data,
		    string dataTypeName) =>
		    (TypeName, MethodName, Data, DataTypeName) =
		    (typeName, methodName, data, dataTypeName);

	    internal string TypeName { get; }
	    internal string MethodName { get; }
	    internal string Data { get; }
	    internal string DataTypeName { get; }

    }
}
