using System.IO;
using System.IO.Compression;

namespace TheLastStand.Framework.Encryption;

public static class Compressor
{
	public static void Compress(Stream inputStream, Stream outputStream)
	{
		using GZipStream destination = new GZipStream(outputStream, CompressionLevel.Fastest);
		inputStream.CopyTo(destination);
	}

	public static void Decompress(Stream inputStream, Stream outputStream)
	{
		using GZipStream gZipStream = new GZipStream(inputStream, CompressionMode.Decompress);
		gZipStream.CopyTo(outputStream);
	}
}
