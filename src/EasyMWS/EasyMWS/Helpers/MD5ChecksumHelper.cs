using System;
using System.IO;
using System.Security.Cryptography;

namespace MountainWarehouse.EasyMWS.Helpers
{
    internal static class MD5ChecksumHelper
    {
	    public static string ComputeHashForAmazon(Stream stream)
	    {
		    stream.Position = 0;
		    var result = Convert.ToBase64String(MD5.Create().ComputeHash(stream));
		    stream.Position = 0;
		    return result;
	    }

		public static bool IsChecksumCorrect(MemoryStream stream, string expectedHash)
		{
			if (stream == null || expectedHash == null) return false;
			stream.Position = 0;
			var actualHash = ComputeHashForAmazon(stream);
			stream.Position = 0;
			return actualHash == expectedHash;
	    }
    }
}
