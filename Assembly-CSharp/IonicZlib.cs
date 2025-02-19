using System.IO;
using Pathfinding.Ionic.Zlib;

public static class IonicZlib
{
	public const int BUFFER_SIZE = 4096;

	public static void Compress(Stream source, Stream dest)
	{
		using ZlibStream zlibStream = new ZlibStream(dest, CompressionMode.Compress, CompressionLevel.BestCompression, leaveOpen: true);
		byte[] array = new byte[4096];
		int count;
		while ((count = source.Read(array, 0, array.Length)) > 0)
		{
			zlibStream.Write(array, 0, count);
		}
	}

	public static void Decompress(Stream source, Stream dest)
	{
		using ZlibStream zlibStream = new ZlibStream(dest, CompressionMode.Decompress, CompressionLevel.BestCompression, leaveOpen: true);
		byte[] array = new byte[4096];
		int count;
		while ((count = source.Read(array, 0, array.Length)) > 0)
		{
			zlibStream.Write(array, 0, count);
		}
	}
}
