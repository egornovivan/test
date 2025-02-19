using System;
using uLink;
using UnityEngine;

[Serializable]
public class IntVector2
{
	public int x;

	public int y;

	public static IntVector2 Tmp = new IntVector2();

	public static IntVector2 Zero => new IntVector2(0, 0);

	public static IntVector2 One => new IntVector2(1, 1);

	public IntVector2()
	{
		x = (y = 0);
	}

	public IntVector2(int x_, int y_)
	{
		x = x_;
		y = y_;
	}

	public static int SqrMagnitude(IntVector2 vec)
	{
		return vec.x * vec.x + vec.y * vec.y;
	}

	public float Distance(IntVector2 vec)
	{
		return Mathf.Sqrt(Mathf.Pow(vec.x - x, 2f) + Mathf.Pow(vec.y - y, 2f));
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		IntVector2 intVector = (IntVector2)obj;
		return x == intVector.x && y == intVector.y;
	}

	public override int GetHashCode()
	{
		return x + (y << 16);
	}

	public override string ToString()
	{
		return $"[{x},{y}]";
	}

	public static IntVector2 Parse(string str)
	{
		string[] array = str.Split(',');
		int x_ = int.Parse(array[0].Substring(1, array[0].Length - 1));
		int y_ = int.Parse(array[1].Substring(0, array[1].Length - 1));
		return new IntVector2(x_, y_);
	}

	public static void SerializeItem(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		IntVector2 intVector = (IntVector2)obj;
		stream.Write(intVector.x);
		stream.Write(intVector.y);
	}

	public static object DeserializeItem(uLink.BitStream stream, params object[] codecOptions)
	{
		int x_ = stream.Read<int>(new object[0]);
		int y_ = stream.Read<int>(new object[0]);
		return new IntVector2(x_, y_);
	}

	public static IntVector2 operator *(IntVector2 mul0, IntVector2 mul1)
	{
		return new IntVector2(mul0.x * mul1.x, mul0.y * mul1.y);
	}

	public static IntVector2 operator *(IntVector2 mul0, int mul1)
	{
		return new IntVector2(mul0.x * mul1, mul0.y * mul1);
	}

	public static IntVector2 operator -(IntVector2 vec0, IntVector2 vec1)
	{
		return new IntVector2(vec0.x - vec1.x, vec0.y - vec1.y);
	}

	public static IntVector2 operator +(IntVector2 vec0, IntVector2 vec1)
	{
		return new IntVector2(vec0.x + vec1.x, vec0.y + vec1.y);
	}

	public static implicit operator IntVector2(Vector3 vec3)
	{
		return new IntVector2((int)vec3.x, (int)vec3.y);
	}

	public static implicit operator Vector2(IntVector2 vec)
	{
		return new Vector2(vec.x, vec.y);
	}

	public static implicit operator Vector3(IntVector2 vec)
	{
		return new Vector3(vec.x, vec.y);
	}
}
