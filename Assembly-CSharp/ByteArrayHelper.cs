public class ByteArrayHelper
{
	public static ushort to_ushort(byte[] arr, int ofs)
	{
		return (ushort)(arr[ofs] + (arr[ofs + 1] << 8));
	}

	public static int to_int(byte[] arr, int ofs)
	{
		return arr[ofs] + (arr[ofs + 1] << 8) + (arr[ofs + 2] << 16) + (arr[ofs + 3] << 24);
	}

	public static IntVector3 to_IntVector3(byte[] arr, int ofs)
	{
		return new IntVector3(arr[ofs], arr[ofs + 1], arr[ofs + 2]);
	}

	public static void ushort_to(byte[] arr, int ofs, ushort val)
	{
		arr[ofs] = (byte)(val & 0xFF);
		arr[ofs + 1] = (byte)((val >> 8) & 0xFF);
	}

	public static void int_to(byte[] arr, int ofs, int val)
	{
		arr[ofs] = (byte)(val & 0xFF);
		arr[ofs + 1] = (byte)((val >> 8) & 0xFF);
		arr[ofs + 2] = (byte)((val >> 16) & 0xFF);
		arr[ofs + 3] = (byte)((val >> 24) & 0xFF);
	}

	public static void IntVector3_to(byte[] arr, int ofs, IntVector3 val)
	{
		arr[ofs] = (byte)val.x;
		arr[ofs + 1] = (byte)val.y;
		arr[ofs + 2] = (byte)val.z;
	}
}
