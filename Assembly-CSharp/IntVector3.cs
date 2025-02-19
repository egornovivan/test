using System;
using uLink;
using UnityEngine;

[Serializable]
public class IntVector3
{
	public int x;

	public int y;

	public int z;

	public static IntVector3 Zero => new IntVector3();

	public static IntVector3 One => new IntVector3(1, 1, 1);

	public static IntVector3 UnitX => new IntVector3(1);

	public static IntVector3 UnitY => new IntVector3(0, 1);

	public static IntVector3 UnitZ => new IntVector3(0, 0, 1);

	public IntVector3(IntVector3 vec3)
	{
		x = vec3.x;
		y = vec3.y;
		z = vec3.z;
	}

	public IntVector3(int x_ = 0, int y_ = 0, int z_ = 0)
	{
		x = x_;
		y = y_;
		z = z_;
	}

	public IntVector3(Vector3 xyz)
	{
		x = Mathf.RoundToInt(xyz.x);
		y = Mathf.RoundToInt(xyz.y);
		z = Mathf.RoundToInt(xyz.z);
	}

	public IntVector3(float x_, float y_, float z_)
	{
		x = Mathf.RoundToInt(x_ + 0.001f);
		y = Mathf.RoundToInt(y_ + 0.001f);
		z = Mathf.RoundToInt(z_ + 0.001f);
	}

	public static void SerializeItem(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		IntVector3 intVector = (IntVector3)obj;
		stream.Write(intVector.x);
		stream.Write(intVector.y);
		stream.Write(intVector.z);
	}

	public static object DeserializeItem(uLink.BitStream stream, params object[] codecOptions)
	{
		int x_ = stream.Read<int>(new object[0]);
		int y_ = stream.Read<int>(new object[0]);
		int z_ = stream.Read<int>(new object[0]);
		return new IntVector3(x_, y_, z_);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public static int SqrMagnitude(IntVector3 vec)
	{
		return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
	}

	public float Distance(IntVector3 vec)
	{
		return Mathf.Sqrt(Mathf.Pow(vec.x - x, 2f) + Mathf.Pow(vec.y - y, 2f) + Mathf.Pow(vec.z - z, 2f));
	}

	public override bool Equals(object obj)
	{
		IntVector3 intVector = (IntVector3)obj;
		return x == intVector.x && y == intVector.y && z == intVector.z;
	}

	public override int GetHashCode()
	{
		return x + (z << 11) + (y << 22);
	}

	public override string ToString()
	{
		return $"[{x},{y},{z}]";
	}

	public static IntVector3 operator *(IntVector3 mul0, IntVector3 mul1)
	{
		return new IntVector3(mul0.x * mul1.x, mul0.y * mul1.y, mul0.z * mul1.z);
	}

	public static IntVector3 operator *(IntVector3 mul0, int mul1)
	{
		return new IntVector3(mul0.x * mul1, mul0.y * mul1, mul0.z * mul1);
	}

	public static IntVector3 operator /(IntVector3 div0, int div1)
	{
		return new IntVector3(div0.x / div1, div0.y / div1, div0.z / div1);
	}

	public static IntVector3 operator -(IntVector3 vec0, IntVector3 vec1)
	{
		return new IntVector3(vec0.x - vec1.x, vec0.y - vec1.y, vec0.z - vec1.z);
	}

	public static IntVector3 operator +(IntVector3 vec0, IntVector3 vec1)
	{
		return new IntVector3(vec0.x + vec1.x, vec0.y + vec1.y, vec0.z + vec1.z);
	}

	public static implicit operator IntVector3(Vector3 vec3)
	{
		return new IntVector3(Mathf.RoundToInt(vec3.x), Mathf.RoundToInt(vec3.y), Mathf.RoundToInt(vec3.z));
	}

	public static implicit operator Vector3(IntVector3 vec)
	{
		return new Vector3(vec.x, vec.y, vec.z);
	}
}
