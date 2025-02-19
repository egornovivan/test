using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WhiteCat;

[StructLayout(LayoutKind.Explicit)]
public struct UnionValue
{
	[FieldOffset(0)]
	[NonSerialized]
	public bool boolValue;

	[FieldOffset(0)]
	[NonSerialized]
	public sbyte sbyteValue;

	[FieldOffset(0)]
	[NonSerialized]
	public byte byteValue;

	[FieldOffset(0)]
	[NonSerialized]
	public char charValue;

	[FieldOffset(0)]
	[NonSerialized]
	public short shortValue;

	[FieldOffset(0)]
	[NonSerialized]
	public ushort ushortValue;

	[FieldOffset(0)]
	[NonSerialized]
	public int intValue;

	[FieldOffset(0)]
	[NonSerialized]
	public uint uintValue;

	[FieldOffset(0)]
	[NonSerialized]
	public long longValue;

	[FieldOffset(0)]
	[NonSerialized]
	public ulong ulongValue;

	[FieldOffset(0)]
	[NonSerialized]
	public float floatValue;

	[FieldOffset(0)]
	[NonSerialized]
	public double doubleValue;

	[FieldOffset(0)]
	[NonSerialized]
	public byte byte0;

	[FieldOffset(1)]
	[NonSerialized]
	public byte byte1;

	[FieldOffset(2)]
	[NonSerialized]
	public byte byte2;

	[FieldOffset(3)]
	[NonSerialized]
	public byte byte3;

	[FieldOffset(4)]
	[NonSerialized]
	public byte byte4;

	[FieldOffset(5)]
	[NonSerialized]
	public byte byte5;

	[FieldOffset(6)]
	[NonSerialized]
	public byte byte6;

	[FieldOffset(7)]
	[NonSerialized]
	public byte byte7;

	[FieldOffset(0)]
	[SerializeField]
	private long _data;

	public byte this[int index]
	{
		get
		{
			return index switch
			{
				0 => byte0, 
				1 => byte1, 
				2 => byte2, 
				3 => byte3, 
				4 => byte4, 
				5 => byte5, 
				6 => byte6, 
				7 => byte7, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				byte0 = value;
				break;
			case 1:
				byte1 = value;
				break;
			case 2:
				byte2 = value;
				break;
			case 3:
				byte3 = value;
				break;
			case 4:
				byte4 = value;
				break;
			case 5:
				byte5 = value;
				break;
			case 6:
				byte6 = value;
				break;
			case 7:
				byte7 = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}

	public UnionValue(bool value)
	{
		boolValue = value;
	}

	public UnionValue(sbyte value)
	{
		sbyteValue = value;
	}

	public UnionValue(byte value)
	{
		byteValue = value;
	}

	public UnionValue(char value)
	{
		charValue = value;
	}

	public UnionValue(short value)
	{
		shortValue = value;
	}

	public UnionValue(ushort value)
	{
		ushortValue = value;
	}

	public UnionValue(int value)
	{
		intValue = value;
	}

	public UnionValue(uint value)
	{
		uintValue = value;
	}

	public UnionValue(long value)
	{
		longValue = value;
	}

	public UnionValue(ulong value)
	{
		ulongValue = value;
	}

	public UnionValue(float value)
	{
		floatValue = value;
	}

	public UnionValue(double value)
	{
		doubleValue = value;
	}

	public void ReadFrom(byte[] buffer, ref int offset, int count)
	{
		int num = 0;
		while (num < count)
		{
			this[num++] = buffer[offset++];
		}
	}

	public void WriteTo(byte[] buffer, ref int offset, int count)
	{
		int num = 0;
		while (num < count)
		{
			buffer[offset++] = this[num++];
		}
	}

	public void ReadBoolFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
	}

	public void WriteBoolTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
	}

	public void ReadSByteFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
	}

	public void WriteSByteTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
	}

	public void ReadByteFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
	}

	public void WriteByteTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
	}

	public void ReadCharFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
		byte1 = buffer[offset++];
	}

	public void WriteCharTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
		buffer[offset++] = byte1;
	}

	public void ReadShortFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
		byte1 = buffer[offset++];
	}

	public void WriteShortTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
		buffer[offset++] = byte1;
	}

	public void ReadUShortFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
		byte1 = buffer[offset++];
	}

	public void WriteUShortTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
		buffer[offset++] = byte1;
	}

	public void ReadIntFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
		byte1 = buffer[offset++];
		byte2 = buffer[offset++];
		byte3 = buffer[offset++];
	}

	public void WriteIntTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
		buffer[offset++] = byte1;
		buffer[offset++] = byte2;
		buffer[offset++] = byte3;
	}

	public void ReadUIntFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
		byte1 = buffer[offset++];
		byte2 = buffer[offset++];
		byte3 = buffer[offset++];
	}

	public void WriteUIntTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
		buffer[offset++] = byte1;
		buffer[offset++] = byte2;
		buffer[offset++] = byte3;
	}

	public void ReadLongFrom(byte[] buffer, ref int offset)
	{
		int num = 0;
		while (num < 8)
		{
			this[num++] = buffer[offset++];
		}
	}

	public void WriteLongTo(byte[] buffer, ref int offset)
	{
		int num = 0;
		while (num < 8)
		{
			buffer[offset++] = this[num++];
		}
	}

	public void ReadULongFrom(byte[] buffer, ref int offset)
	{
		int num = 0;
		while (num < 8)
		{
			this[num++] = buffer[offset++];
		}
	}

	public void WriteULongTo(byte[] buffer, ref int offset)
	{
		int num = 0;
		while (num < 8)
		{
			buffer[offset++] = this[num++];
		}
	}

	public void ReadFloatFrom(byte[] buffer, ref int offset)
	{
		byte0 = buffer[offset++];
		byte1 = buffer[offset++];
		byte2 = buffer[offset++];
		byte3 = buffer[offset++];
	}

	public void WriteFloatTo(byte[] buffer, ref int offset)
	{
		buffer[offset++] = byte0;
		buffer[offset++] = byte1;
		buffer[offset++] = byte2;
		buffer[offset++] = byte3;
	}

	public void ReadDoubleFrom(byte[] buffer, ref int offset)
	{
		int num = 0;
		while (num < 8)
		{
			this[num++] = buffer[offset++];
		}
	}

	public void WriteDoubleTo(byte[] buffer, ref int offset)
	{
		int num = 0;
		while (num < 8)
		{
			buffer[offset++] = this[num++];
		}
	}

	public void ReadFrom(Stream stream, int count)
	{
		for (int i = 0; i < count; i++)
		{
			int num;
			if ((num = stream.ReadByte()) == -1)
			{
				throw new EndOfStreamException();
			}
			this[i] = (byte)num;
		}
	}

	public void WriteTo(Stream stream, int count)
	{
		for (int i = 0; i < count; i++)
		{
			stream.WriteByte(this[i]);
		}
	}

	public static implicit operator bool(UnionValue value)
	{
		return value.boolValue;
	}

	public static implicit operator UnionValue(bool value)
	{
		return new UnionValue(value);
	}

	public static implicit operator sbyte(UnionValue value)
	{
		return value.sbyteValue;
	}

	public static implicit operator UnionValue(sbyte value)
	{
		return new UnionValue(value);
	}

	public static implicit operator byte(UnionValue value)
	{
		return value.byteValue;
	}

	public static implicit operator UnionValue(byte value)
	{
		return new UnionValue(value);
	}

	public static implicit operator char(UnionValue value)
	{
		return value.charValue;
	}

	public static implicit operator UnionValue(char value)
	{
		return new UnionValue(value);
	}

	public static implicit operator short(UnionValue value)
	{
		return value.shortValue;
	}

	public static implicit operator UnionValue(short value)
	{
		return new UnionValue(value);
	}

	public static implicit operator ushort(UnionValue value)
	{
		return value.ushortValue;
	}

	public static implicit operator UnionValue(ushort value)
	{
		return new UnionValue(value);
	}

	public static implicit operator int(UnionValue value)
	{
		return value.intValue;
	}

	public static implicit operator UnionValue(int value)
	{
		return new UnionValue(value);
	}

	public static implicit operator uint(UnionValue value)
	{
		return value.uintValue;
	}

	public static implicit operator UnionValue(uint value)
	{
		return new UnionValue(value);
	}

	public static implicit operator long(UnionValue value)
	{
		return value.longValue;
	}

	public static implicit operator UnionValue(long value)
	{
		return new UnionValue(value);
	}

	public static implicit operator ulong(UnionValue value)
	{
		return value.ulongValue;
	}

	public static implicit operator UnionValue(ulong value)
	{
		return new UnionValue(value);
	}

	public static implicit operator float(UnionValue value)
	{
		return value.floatValue;
	}

	public static implicit operator UnionValue(float value)
	{
		return new UnionValue(value);
	}

	public static implicit operator double(UnionValue value)
	{
		return value.doubleValue;
	}

	public static implicit operator UnionValue(double value)
	{
		return new UnionValue(value);
	}
}
