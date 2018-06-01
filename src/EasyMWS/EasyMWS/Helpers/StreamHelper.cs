using System.IO;

namespace MountainWarehouse.EasyMWS.Helpers
{
    public static class StreamHelper
    {
	    public static Stream CreateMemoryStream(string streamContent)
	    {
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(streamContent);
			writer.Flush();
		    stream.Position = 0;
		    return stream;
	    }

	    public static MemoryStream GetStreamFromBytes(byte[] byteArray)
	    {
			return new MemoryStream(byteArray);
	    }

	    public static byte[] GetBytesFromStream(MemoryStream stream)
	    {
		    return stream.ToArray();
	    }
	}
}
