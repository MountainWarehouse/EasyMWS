namespace MountainWarehouse.EasyMWS
{
    public class Callback
    {
	    public Callback(string typeName,
		    string methodName,
		    string data,
		    string dataTypeName) =>
		    (TypeName, MethodName, Data, DataTypeName) =
		    (typeName, methodName, data, dataTypeName);

		public string TypeName { get; }
		public string MethodName { get; }
		public string Data { get; }
		public string DataTypeName { get; }

    }
}
