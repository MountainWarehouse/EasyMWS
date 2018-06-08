using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using MountainWarehouse.EasyMWS.Helpers;
using NUnit.Framework;

namespace EasyMWS.Tests.Helpers
{
    public class ZipHelperTests
    {
	    [Test]
	    public void CreateArchiveFromContent_WithNullFileContent_ReturnsNull()
	    {
		    var nullContent = (string)null;

		    var resultingArchive = ZipHelper.CreateArchiveFromContent(nullContent);

			Assert.IsNull(resultingArchive);
	    }

		[Test]
		public void CreateArchiveFromContent_WithEmptyFileContent_ReturnsNull()
		{
			var emptyContent = string.Empty;

			var resultingArchive = ZipHelper.CreateArchiveFromContent(emptyContent);

			Assert.IsNull(resultingArchive);
		}

	    [Test]
	    public void CreateArchiveFromContent_WithValidFileContent_ReturnsValidArchiveContainingTheCorrectContent()
	    {
		    var content = GetLoremIpsumContent();

			var resultingArchive = ZipHelper.CreateArchiveFromContent(content);

		    Assert.NotNull(resultingArchive);

		    var successfullyOpenedArchive = new ZipArchive(new MemoryStream(resultingArchive), ZipArchiveMode.Read, true);
		    var successfullyReadEntry = successfullyOpenedArchive.Entries.First().Open();

			Assert.NotNull(successfullyReadEntry);

			var streamReader = new StreamReader(successfullyReadEntry);
			
			Assert.AreEqual(content, streamReader.ReadToEnd());

		    successfullyOpenedArchive.Dispose();
			streamReader.Close();
		}

		[Test]
	    public void ExtractArchivedSingleFileToStream_WithNullArgument_ReturnsNull()
		{
			var nullArchiveBytes = (byte[]) null;

			var resultingContentStream = ZipHelper.ExtractArchivedSingleFileToStream(nullArchiveBytes);

			Assert.IsNull(resultingContentStream);
		}

	    [Test]
	    public void ExtractArchivedSingleFileToStream_WithInvalidArgument_ThrowsInvalidDataException()
	    {
		    var contentThatIsNoArchiveType = GetLoremIpsumContent();
			var invalidArchiveBytes = Encoding.ASCII.GetBytes(contentThatIsNoArchiveType);

		    Assert.Throws<InvalidDataException>(() => ZipHelper.ExtractArchivedSingleFileToStream(invalidArchiveBytes));
	    }

	    [Test]
	    public void ExtractArchivedSingleFileToStream_WithValidArgument_ReturnsStreamContainingCorrectContent()
	    {
		    var testContent = GetLoremIpsumContent();
			var validArchive = GenerateValidArchive(testContent);

			var resultingContentStream = ZipHelper.ExtractArchivedSingleFileToStream(validArchive.ToArray());

			Assert.NotNull(resultingContentStream);

		    var streamReader = new StreamReader(resultingContentStream);

		    Assert.AreEqual(testContent, streamReader.ReadToEnd());

		    resultingContentStream.Dispose();
			validArchive.Dispose();
		    streamReader.Close();
		}

		[Test]
	    public void Integration_CreateArchiveFromContent_And_ExtractArchivedSingleFileToStream_WorkTogether()
	    {
			var content = GetLoremIpsumContent();

		    var resultingArchive = ZipHelper.CreateArchiveFromContent(content);
			var resultingContentStream = ZipHelper.ExtractArchivedSingleFileToStream(resultingArchive);

		    Assert.NotNull(resultingContentStream);
		    var streamReader = new StreamReader(resultingContentStream);

		    Assert.AreEqual(content, streamReader.ReadToEnd());

			resultingContentStream.Dispose();
		    streamReader.Close();
		}

	    private string GetLoremIpsumContent()
	    {
		    return
				"Lorem ipsum dolor sit amet, consectetur adipiscing elit. In eleifend eget nisi et condimentum. Sed id tristique ex. Donec semper porttitor venenatis. Duis volutpat maximus arcu pharetra fringilla. Curabitur at ipsum non dui rhoncus vulputate et id leo. Ut molestie tortor sit amet finibus rutrum. Donec consequat blandit diam suscipit efficitur. Cras tristique, nisl sed feugiat interdum, mauris augue dapibus eros, vitae porta est est ac justo. Fusce id eros sed lacus bibendum condimentum. Nunc sagittis consequat lorem sit amet scelerisque. In a sapien congue ante pharetra dignissim vitae quis nulla. In tincidunt, ligula efficitur posuere mattis. Die Vögel singen schön. Die Straße ist breit.";

	    }

		private MemoryStream GenerateValidArchive(string content)
	    {
			using (var zipFileStream = new MemoryStream())
			{
				using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create, true))
				{
					var fileToArchive = archive.CreateEntry("testFilename.txt", CompressionLevel.Fastest);
					using (var fileStream = fileToArchive.Open())
					using (var streamWriter = new StreamWriter(fileStream))
					{
						streamWriter.Write(content);
					}
				}

				zipFileStream.Position = 0;
				return zipFileStream;
			}
		}
	}
}
