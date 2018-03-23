using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MountainWarehouse.EasyMWS.Helpers
{
    internal static class MD5ChecksumHelper
    {
	    public static string ComputeHash(Stream stream)
	    {
		    return MD5.Create().ComputeHash(stream).Select(b => b.ToString("X2")).Aggregate((s1, s2) => $"{s1}{s2}");
	    }

	    public static string ComputeHashForAmazon(Stream stream)
	    {
		    return Convert.ToBase64String(MD5.Create().ComputeHash(stream));
	    }

		public static bool IsChecksumCorrect(Stream stream, string expectedHash)
	    {
		    return (stream != null && expectedHash != null) && ComputeHash(stream) == expectedHash;
	    }
    }
}
