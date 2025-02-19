using System;
using uLink;
using UnityEngine;

[Serializable]
public class IntVector4
{
	public int x;

	public int y;

	public int z;

	public int w;

	public static IntVector4 Zero => new IntVector4(0, 0, 0, 0);

	public IntVector3 XYZ => new IntVector3(x, y, z);

	public IntVector4()
	{
		x = 0;
		y = 0;
		z = 0;
		w = 0;
	}

	public IntVector4(int x_, int y_, int z_, int w_)
	{
		x = x_;
		y = y_;
		z = z_;
		w = w_;
	}

	public IntVector4(IntVector3 v3, int w_)
	{
		x = v3.x;
		y = v3.y;
		z = v3.z;
		w = w_;
	}

	public IntVector4(IntVector4 v4)
	{
		x = v4.x;
		y = v4.y;
		z = v4.z;
		w = v4.w;
	}

	public IntVector4(Vector3 xyz, int w_)
	{
		x = Mathf.RoundToInt(xyz.x);
		y = Mathf.RoundToInt(xyz.y);
		z = Mathf.RoundToInt(xyz.z);
		w = w_;
	}

	public IntVector3 ToIntVector3()
	{
		return new IntVector3(x, y, z);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public static int SqrMagnitude(IntVector4 vec)
	{
		return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w;
	}

	public bool ContainInTerrainNode(Vector3 pos)
	{
		int num = 32 << w;
		return pos.x >= (float)x && pos.x < (float)(x + num) && pos.y >= (float)y && pos.y < (float)(y + num) && pos.z >= (float)z && pos.z < (float)(z + num);
	}

	public bool Contains(Vector3 pos)
	{
		return pos.x >= (float)x && pos.x < (float)(x + w) && pos.y >= (float)y && pos.y < (float)(y + w) && pos.z >= (float)z && pos.z < (float)(z + w);
	}

	public override bool Equals(object obj)
	{
		IntVector4 intVector = (IntVector4)obj;
		return x == intVector.x && y == intVector.y && z == intVector.z && w == intVector.w;
	}

	public override int GetHashCode()
	{
		return (x & 0x3FF) + ((z & 0x3FF) << 10) + (((y + 1) & 0xFF) << 20) + (w << 28);
	}

	public override string ToString()
	{
		return $"[{x},{y},{z},{w}]";
	}

	public static void SerializeItem(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		IntVector4 intVector = (IntVector4)obj;
		stream.Write(intVector.x);
		stream.Write(intVector.y);
		stream.Write(intVector.z);
		stream.Write(intVector.w);
	}

	public static object DeserializeItem(uLink.BitStream stream, params object[] codecOptions)
	{
		int x_ = stream.Read<int>(new object[0]);
		int y_ = stream.Read<int>(new object[0]);
		int z_ = stream.Read<int>(new object[0]);
		int w_ = stream.Read<int>(new object[0]);
		return new IntVector4(x_, y_, z_, w_);
	}
}
