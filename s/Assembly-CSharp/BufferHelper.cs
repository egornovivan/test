using System;
using System.IO;
using CustomData;
using UnityEngine;

public class BufferHelper
{
	private static byte[] Buffer = new byte[8192];

	public static byte[] Export(Action<BinaryWriter> writer)
	{
		MemoryStream memoryStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			memoryStream = new MemoryStream(Buffer);
			binaryWriter = new BinaryWriter(memoryStream);
			writer(binaryWriter);
			return memoryStream.ToArray();
		}
		catch
		{
			return null;
		}
		finally
		{
			if (binaryWriter != null)
			{
				binaryWriter.Close();
				binaryWriter = null;
			}
			if (memoryStream != null)
			{
				memoryStream.Close();
				memoryStream = null;
			}
		}
	}

	public static byte[] ExportThreadSafe(Action<BinaryWriter> writer)
	{
		return ExportThreadSafe(writer, 256);
	}

	public static byte[] ExportThreadSafe(Action<BinaryWriter> writer, int capacity)
	{
		MemoryStream memoryStream = null;
		BinaryWriter binaryWriter = null;
		try
		{
			memoryStream = new MemoryStream(capacity);
			binaryWriter = new BinaryWriter(memoryStream);
			writer(binaryWriter);
			return memoryStream.ToArray();
		}
		catch
		{
			return null;
		}
		finally
		{
			if (binaryWriter != null)
			{
				binaryWriter.Close();
				binaryWriter = null;
			}
			if (memoryStream != null)
			{
				memoryStream.Close();
				memoryStream = null;
			}
		}
	}

	public static void Import(byte[] buffer, Action<BinaryReader> reader)
	{
		MemoryStream memoryStream = null;
		BinaryReader binaryReader = null;
		try
		{
			memoryStream = new MemoryStream(buffer);
			binaryReader = new BinaryReader(memoryStream);
			reader(binaryReader);
		}
		finally
		{
			if (binaryReader != null)
			{
				binaryReader.Close();
				binaryReader = null;
			}
			if (memoryStream != null)
			{
				memoryStream.Close();
				memoryStream = null;
			}
		}
	}

	internal static void Serialize(BinaryWriter _writer, string _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, bool _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, char _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, byte _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, sbyte _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, short _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, ushort _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, int _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, uint _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, long _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, ulong _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, float _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, double _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, decimal _value)
	{
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, byte[] _value)
	{
		if (_value == null || _value.Length == 0)
		{
			_writer.Write(0);
			return;
		}
		_writer.Write(_value.Length);
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, byte[] _value, int _index, int _count)
	{
		_writer.Write(_value, _index, _count);
	}

	internal static void Serialize(BinaryWriter _writer, char[] _value)
	{
		if (_value == null || _value.Length == 0)
		{
			_writer.Write(0);
			return;
		}
		_writer.Write(_value.Length);
		_writer.Write(_value);
	}

	internal static void Serialize(BinaryWriter _writer, char[] _value, int _index, int _count)
	{
		_writer.Write(_value, _index, _count);
	}

	internal static void Serialize(BinaryWriter _writer, IntVector3 _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
		_writer.Write(_value.z);
	}

	internal static void Serialize(BinaryWriter _writer, IntVector2 _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
	}

	internal static void Serialize(BinaryWriter _writer, Vector3 _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
		_writer.Write(_value.z);
	}

	internal static void Serialize(BinaryWriter _writer, Vector2 _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
	}

	internal static void Serialize(BinaryWriter _writer, Quaternion _value)
	{
		_writer.Write(_value.x);
		_writer.Write(_value.y);
		_writer.Write(_value.z);
		_writer.Write(_value.w);
	}

	internal static void Serialize(BinaryWriter _writer, B45Block _value)
	{
		_writer.Write(_value.blockType);
		_writer.Write(_value.materialType);
	}

	internal static void Serialize(BinaryWriter _writer, VFVoxel _value)
	{
		_writer.Write(_value.Type);
		_writer.Write(_value.Volume);
	}

	internal static void Serialize(BinaryWriter _writer, BSVoxel _value)
	{
		_writer.Write(_value.value0);
		_writer.Write(_value.value1);
	}

	internal static void Serialize(BinaryWriter _writer, Color _value)
	{
		_writer.Write(_value.a);
		_writer.Write(_value.r);
		_writer.Write(_value.g);
		_writer.Write(_value.b);
	}

	public static void Serialize(BinaryWriter writer, OBJECT obj)
	{
		writer.Write(obj.Id);
		writer.Write(obj.Group);
		writer.Write((byte)obj.type);
	}

	public static void Serialize(BinaryWriter writer, RANGE obj)
	{
		Serialize(writer, obj.center);
		Serialize(writer, obj.extend);
		Serialize(writer, obj.radius);
		Serialize(writer, (byte)obj.type);
		Serialize(writer, obj.inverse);
	}

	public static void Serialize(BinaryWriter writer, DIRRANGE obj)
	{
		Serialize(writer, obj.directrix);
		Serialize(writer, obj.error);
		Serialize(writer, (byte)obj.type);
		Serialize(writer, obj.inverse);
	}

	internal static bool InvalidReader(BinaryReader _reader)
	{
		return _reader.PeekChar() == -1;
	}

	internal static string ReadString(BinaryReader _reader)
	{
		return _reader.ReadString();
	}

	internal static bool ReadBoolean(BinaryReader _reader)
	{
		return _reader.ReadBoolean();
	}

	internal static char ReadChar(BinaryReader _reader)
	{
		return _reader.ReadChar();
	}

	internal static byte ReadByte(BinaryReader _reader)
	{
		return _reader.ReadByte();
	}

	internal static sbyte ReadSByte(BinaryReader _reader)
	{
		return _reader.ReadSByte();
	}

	internal static short ReadInt16(BinaryReader _reader)
	{
		return _reader.ReadInt16();
	}

	internal static ushort ReadUInt16(BinaryReader _reader)
	{
		return _reader.ReadUInt16();
	}

	internal static int ReadInt32(BinaryReader _reader)
	{
		return _reader.ReadInt32();
	}

	internal static uint ReadUInt32(BinaryReader _reader)
	{
		return _reader.ReadUInt32();
	}

	internal static long ReadInt64(BinaryReader _reader)
	{
		return _reader.ReadInt64();
	}

	internal static ulong ReadUInt64(BinaryReader _reader)
	{
		return _reader.ReadUInt64();
	}

	internal static float ReadSingle(BinaryReader _reader)
	{
		return _reader.ReadSingle();
	}

	internal static double ReadDouble(BinaryReader _reader)
	{
		return _reader.ReadDouble();
	}

	internal static decimal ReadDecimal(BinaryReader _reader)
	{
		return _reader.ReadDecimal();
	}

	internal static byte[] ReadBytes(BinaryReader _reader)
	{
		int num = ReadInt32(_reader);
		if (num == 0)
		{
			return null;
		}
		return _reader.ReadBytes(num);
	}

	internal static char[] ReadChars(BinaryReader _reader)
	{
		int num = ReadInt32(_reader);
		if (num == 0)
		{
			return null;
		}
		return _reader.ReadChars(num);
	}

	internal static IntVector3 ReadIntVector3(BinaryReader _reader)
	{
		IntVector3 intVector = new IntVector3();
		intVector.x = ReadInt32(_reader);
		intVector.y = ReadInt32(_reader);
		intVector.z = ReadInt32(_reader);
		return intVector;
	}

	internal static IntVector2 ReadIntVector2(BinaryReader _reader)
	{
		IntVector2 intVector = new IntVector2();
		intVector.x = ReadInt32(_reader);
		intVector.y = ReadInt32(_reader);
		return intVector;
	}

	internal static void ReadVector3(BinaryReader _reader, out Vector3 _value)
	{
		_value.x = ReadSingle(_reader);
		_value.y = ReadSingle(_reader);
		_value.z = ReadSingle(_reader);
	}

	internal static void ReadVector2(BinaryReader _reader, out Vector2 _value)
	{
		_value.x = _reader.ReadSingle();
		_value.y = _reader.ReadSingle();
	}

	internal static void ReadQuaternion(BinaryReader _reader, out Quaternion _value)
	{
		_value.x = ReadSingle(_reader);
		_value.y = ReadSingle(_reader);
		_value.z = ReadSingle(_reader);
		_value.w = ReadSingle(_reader);
	}

	internal static void ReadB45Block(BinaryReader _reader, out B45Block _value)
	{
		_value.blockType = ReadByte(_reader);
		_value.materialType = ReadByte(_reader);
	}

	internal static void ReadVFVoxel(BinaryReader _reader, out VFVoxel _value)
	{
		_value.Type = ReadByte(_reader);
		_value.Volume = ReadByte(_reader);
	}

	internal static void ReadBSVoxel(BinaryReader _reader, out BSVoxel _value)
	{
		byte v = ReadByte(_reader);
		byte v2 = ReadByte(_reader);
		_value = new BSVoxel(v, v2);
	}

	internal static void ReadColor(BinaryReader _reader, out Color _value)
	{
		_value.a = ReadSingle(_reader);
		_value.r = ReadSingle(_reader);
		_value.g = ReadSingle(_reader);
		_value.b = ReadSingle(_reader);
	}

	public static void ReadObject(BinaryReader _reader, out OBJECT obj)
	{
		obj.Id = _reader.ReadInt32();
		obj.Group = _reader.ReadInt32();
		obj.type = (OBJECTTYPE)_reader.ReadByte();
	}

	public static void ReadRange(BinaryReader _reader, out RANGE obj)
	{
		ReadVector3(_reader, out obj.center);
		ReadVector3(_reader, out obj.extend);
		obj.radius = _reader.ReadSingle();
		obj.type = (RANGE.RANGETYPE)_reader.ReadByte();
		obj.inverse = _reader.ReadBoolean();
	}

	public static void ReadDirRange(BinaryReader _reader, out DIRRANGE obj)
	{
		ReadVector3(_reader, out obj.directrix);
		ReadVector2(_reader, out obj.error);
		obj.type = (DIRRANGE.DIRRANGETYPE)_reader.ReadByte();
		obj.inverse = _reader.ReadBoolean();
	}
}
