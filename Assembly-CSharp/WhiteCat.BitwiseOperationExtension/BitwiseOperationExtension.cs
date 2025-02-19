namespace WhiteCat.BitwiseOperationExtension;

public static class BitwiseOperationExtension
{
	public static sbyte SetBit0(this sbyte value, int bit)
	{
		return (sbyte)(value & ~(1 << bit));
	}

	public static byte SetBit0(this byte value, int bit)
	{
		return (byte)(value & ~(1 << bit));
	}

	public static short SetBit0(this short value, int bit)
	{
		return (short)(value & ~(1 << bit));
	}

	public static ushort SetBit0(this ushort value, int bit)
	{
		return (ushort)(value & ~(1 << bit));
	}

	public static int SetBit0(this int value, int bit)
	{
		return value & ~(1 << bit);
	}

	public static uint SetBit0(this uint value, int bit)
	{
		return value & (uint)(~(1 << bit));
	}

	public static long SetBit0(this long value, int bit)
	{
		return value & ~(1L << bit);
	}

	public static ulong SetBit0(this ulong value, int bit)
	{
		return value & (ulong)(~(1L << bit));
	}

	public static sbyte SetBit1(this sbyte value, int bit)
	{
		return (sbyte)((byte)value | (1 << bit));
	}

	public static byte SetBit1(this byte value, int bit)
	{
		return (byte)(value | (1 << bit));
	}

	public static short SetBit1(this short value, int bit)
	{
		return (short)((ushort)value | (1 << bit));
	}

	public static ushort SetBit1(this ushort value, int bit)
	{
		return (ushort)(value | (1 << bit));
	}

	public static int SetBit1(this int value, int bit)
	{
		return value | (1 << bit);
	}

	public static uint SetBit1(this uint value, int bit)
	{
		return value | (uint)(1 << bit);
	}

	public static long SetBit1(this long value, int bit)
	{
		return value | (1L << bit);
	}

	public static ulong SetBit1(this ulong value, int bit)
	{
		return value | (ulong)(1L << bit);
	}

	public static sbyte SetBit(this sbyte value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static byte SetBit(this byte value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static short SetBit(this short value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static ushort SetBit(this ushort value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static int SetBit(this int value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static uint SetBit(this uint value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static long SetBit(this long value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static ulong SetBit(this ulong value, int bit, bool is1)
	{
		return (!is1) ? value.SetBit0(bit) : value.SetBit1(bit);
	}

	public static sbyte ReverseBit(this sbyte value, int bit)
	{
		return (sbyte)(value ^ (1 << bit));
	}

	public static byte ReverseBit(this byte value, int bit)
	{
		return (byte)(value ^ (1 << bit));
	}

	public static short ReverseBit(this short value, int bit)
	{
		return (short)(value ^ (1 << bit));
	}

	public static ushort ReverseBit(this ushort value, int bit)
	{
		return (ushort)(value ^ (1 << bit));
	}

	public static int ReverseBit(this int value, int bit)
	{
		return value ^ (1 << bit);
	}

	public static uint ReverseBit(this uint value, int bit)
	{
		return value ^ (uint)(1 << bit);
	}

	public static long ReverseBit(this long value, int bit)
	{
		return value ^ (1L << bit);
	}

	public static ulong ReverseBit(this ulong value, int bit)
	{
		return value ^ (ulong)(1L << bit);
	}

	public static bool GetBit(this sbyte value, int bit)
	{
		return (value & (1 << bit)) != 0;
	}

	public static bool GetBit(this byte value, int bit)
	{
		return (value & (1 << bit)) != 0;
	}

	public static bool GetBit(this short value, int bit)
	{
		return (value & (1 << bit)) != 0;
	}

	public static bool GetBit(this ushort value, int bit)
	{
		return (value & (1 << bit)) != 0;
	}

	public static bool GetBit(this int value, int bit)
	{
		return (value & (1 << bit)) != 0;
	}

	public static bool GetBit(this uint value, int bit)
	{
		return (value & (uint)(1 << bit)) != 0;
	}

	public static bool GetBit(this long value, int bit)
	{
		return (value & (1L << bit)) != 0;
	}

	public static bool GetBit(this ulong value, int bit)
	{
		return (value & (ulong)(1L << bit)) != 0;
	}
}
