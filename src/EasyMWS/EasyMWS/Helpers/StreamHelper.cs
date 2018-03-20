using System.IO;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public static class StreamHelper
    {
	    public static Stream CreateNewMemoryStream(string streamContent)
	    {
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(streamContent);
			writer.Flush();
		    stream.Position = 0;
		    return stream;
	    }
    }
}
