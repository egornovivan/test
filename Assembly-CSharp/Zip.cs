using System.IO;
using System.IO.Compression;

public static class Zip
{
	public const int BUFFER_SIZE = 4096;

	public static void Compress(Stream source, Stream dest)
	{
		using GZipStream gZipStream = new GZipStream(dest, CompressionMode.Compress, leaveOpen: true);
		byte[] array = new byte[4096];
		int count;
		while ((count = source.Read(array, 0, array.Length)) > 0)
		{
			gZipStream.Write(array, 0, count);
		}
	}

	public static void Decompress(Stream source, Stream dest)
	{
		using GZipStream gZipStream = new GZipStream(source, CompressionMode.Decompress, leaveOpen: true);
		byte[] array = new byte[4096];
		int count;
		while ((count = gZipStream.Read(array, 0, array.Length)) > 0)
		{
			dest.Write(array, 0, count);
		}
	}
}
