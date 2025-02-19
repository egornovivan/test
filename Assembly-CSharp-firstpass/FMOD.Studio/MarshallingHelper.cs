using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio;

internal class MarshallingHelper
{
	public static int stringLengthUtf8(IntPtr nativeUtf8)
	{
		int i;
		for (i = 0; Marshal.ReadByte(nativeUtf8, i) != 0; i++)
		{
		}
		return i;
	}

	public static string stringFromNativeUtf8(IntPtr nativeUtf8)
	{
		int num = stringLengthUtf8(nativeUtf8);
		if (num == 0)
		{
			return string.Empty;
		}
		byte[] array = new byte[num];
		Marshal.Copy(nativeUtf8, array, 0, array.Length);
		return Encoding.UTF8.GetString(array, 0, num);
	}
}
