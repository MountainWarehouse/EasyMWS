using System.IO;
using Newtonsoft.Json;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public static class JsonHelper
    {
	    public static void SerializeToStream(object value, Stream s)
	    {
		    using (StreamWriter writer = new StreamWriter(s))
		    using (JsonTextWriter jsonWriter = new JsonTextWriter(writer))
		    {
			    JsonSerializer ser = new JsonSerializer();
			    ser.Serialize(jsonWriter, value);
			    jsonWriter.Flush();
		    }
	    }

	    public static T DeserializeFromStream<T>(Stream s)
	    {
		    using (StreamReader reader = new StreamReader(s))
		    using (JsonTextReader jsonReader = new JsonTextReader(reader))
		    {
			    JsonSerializer ser = new JsonSerializer();
			    return ser.Deserialize<T>(jsonReader);
		    }
	    }
	}
}
