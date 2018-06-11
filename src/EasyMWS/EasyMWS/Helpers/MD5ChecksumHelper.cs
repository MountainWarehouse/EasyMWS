using System;
using System.IO;
using System.Security.Cryptography;

namespace MountainWarehouse.EasyMWS.Helpers
{
    internal static class MD5ChecksumHelper
    {
	    public static string ComputeHashForAmazon(Stream stream)
	    {
		    return Convert.ToBase64String(MD5.Create().ComputeHash(stream));
	    }

		public static bool IsChecksumCorrect(MemoryStream stream, string expectedHash)
		{
			if (stream == null || expectedHash == null) return false;
			var actualHash = ComputeHashForAmazon(stream);
			return actualHash == expectedHash;
	    }
    }
}
