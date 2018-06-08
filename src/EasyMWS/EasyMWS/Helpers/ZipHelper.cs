using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MountainWarehouse.EasyMWS.Helpers
{
	/// <summary>
	/// A helper that can be used to archive feeds content and reports content.
	/// </summary>
    public static class ZipHelper
    {
		/// <summary>
		/// Creates an archive file expressed as a byte array.
		/// The content of the archive will consist of a single file whose own content consists of the fileContent argument provided.
		/// </summary>
		/// <param name="content">The content of the file to be added to an archive.</param>
		/// <param name="contentFilename">The filename of the file being created from the fileContent argument.</param>
		/// <param name="compressionLevel">The compression level used to archive fileContent.</param>
		/// <returns></returns>
		public static byte[] CreateArchiveFromContent(string content, string contentFilename = "file.txt", CompressionLevel compressionLevel = CompressionLevel.Fastest)
		{
			if (string.IsNullOrEmpty(content)) return null;

		    using (var zipFileStream = new MemoryStream())
		    {
			    using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, true))
			    {
				    var fileToArchive = archive.CreateEntry(contentFilename, compressionLevel);
					using (var fileStream = fileToArchive.Open())
					using (var streamWriter = new StreamWriter(fileStream))
					{
						streamWriter.Write(content);
					}
			    }

			    zipFileStream.Position = 0;
				return zipFileStream.ToArray();
		    }
	    }

	    /// <summary>
	    /// Extracts the content of the archive specified as argument, the content is expected to be a single file.<para/>
	    /// A stream is created from the extracted file, and the stream is returned.<para/>
	    /// This method should be called from inside a using, otherwise make sure the stream is always disposed of.
	    /// </summary>
	    /// <param name="zipArchive">An archive file expressed as a byte array.</param>
	    /// <returns></returns>
	    public static MemoryStream ExtractArchivedSingleFileToStream(byte[] zipArchive)
	    {
		    if (zipArchive == null) return null;

		    using (var archiveStream = new MemoryStream(zipArchive))
		    using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Read))
		    {
			    var file = zip.Entries.FirstOrDefault();
				var resultStream = new MemoryStream();
				file?.Open()?.CopyTo(resultStream);
			    resultStream.Position = 0;
			    return resultStream;
		    }
	    }
    }
}
