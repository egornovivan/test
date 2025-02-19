using System.Runtime.InteropServices;

public static class LZ4
{
	[DllImport("lz4_dll")]
	public static extern int LZ4_DllLoad();

	[DllImport("lz4_dll")]
	public static extern int LZ4_compress(byte[] source, byte[] dest, int isize);

	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress(byte[] source, byte[] dest, int osize);

	[DllImport("lz4_dll")]
	public static extern int LZ4_uncompress_unknownOutputSize(byte[] source, byte[] dest, int isize, int maxOutputSize);
}
