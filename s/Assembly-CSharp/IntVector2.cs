using uLink;
using UnityEngine;

public class IntVector2
{
	public int x;

	public int y;

	public IntVector2()
	{
		x = (y = 0);
	}

	public IntVector2(int x_, int y_)
	{
		x = x_;
		y = y_;
	}

	public float Distance(IntVector2 vec)
	{
		return Mathf.Sqrt(Mathf.Pow(vec.x - x, 2f) + Mathf.Pow(vec.y - y, 2f));
	}

	public override bool Equals(object obj)
	{
		if (obj is IntVector2)
		{
			IntVector2 intVector = (IntVector2)obj;
			return x == intVector.x && y == intVector.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x + (y << 16);
	}

	public override string ToString()
	{
		return $"[{x},{y}]";
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

	public static implicit operator Vector3(IntVector2 vec)
	{
		return new Vector3(vec.x, vec.y);
	}
}
public struct INTVECTOR2
{
	public int x;

	public int y;

	public int hash
	{
		get
		{
			return (x & 0xFFFF) | ((y & 0xFFFF) << 16);
		}
		set
		{
			x = (short)(value & 0xFFFF);
			y = (short)((value >> 16) & 0xFFFF);
		}
	}

	public int hash_u
	{
		get
		{
			return hash;
		}
		set
		{
			x = value & 0xFFFF;
			y = (value >> 16) & 0xFFFF;
		}
	}

	public static INTVECTOR2 zero => new INTVECTOR2(0, 0);

	public static INTVECTOR2 one => new INTVECTOR2(1, 1);

	public static INTVECTOR2 minusone => new INTVECTOR2(-1, -1);

	public static INTVECTOR2 unit_x => new INTVECTOR2(1, 0);

	public static INTVECTOR2 unit_y => new INTVECTOR2(0, 1);

	public float sqrMagnitude => x * x + y * y;

	public float magnitude => Mathf.Sqrt(sqrMagnitude);

	public INTVECTOR2(INTVECTOR2 vec)
	{
		x = vec.x;
		y = vec.y;
	}

	public INTVECTOR2(int x_, int y_)
	{
		x = x_;
		y = y_;
	}

	public INTVECTOR2(float x_, float y_)
	{
		x = Mathf.FloorToInt(x_ + 1E-05f);
		y = Mathf.FloorToInt(y_ + 1E-05f);
	}

	public float Distance(INTVECTOR2 vec)
	{
		return Mathf.Sqrt((vec.x - x) * (vec.x - x) + (vec.y - y) * (vec.y - y));
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is INTVECTOR2 iNTVECTOR)
		{
			return x == iNTVECTOR.x && y == iNTVECTOR.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public override string ToString()
	{
		return $"[{x},{y}]";
	}

	public static bool operator ==(INTVECTOR2 lhs, INTVECTOR2 rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	public static bool operator !=(INTVECTOR2 lhs, INTVECTOR2 rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}

	public static INTVECTOR2 operator *(INTVECTOR2 lhs, INTVECTOR2 rhs)
	{
		return new INTVECTOR2(lhs.x * rhs.x, lhs.y * rhs.y);
	}

	public static INTVECTOR2 operator *(INTVECTOR2 lhs, int rhs)
	{
		return new INTVECTOR2(lhs.x * rhs, lhs.y * rhs);
	}

	public static INTVECTOR2 operator /(INTVECTOR2 lhs, int rhs)
	{
		return new INTVECTOR2(lhs.x / rhs, lhs.y / rhs);
	}

	public static INTVECTOR2 operator -(INTVECTOR2 lhs, INTVECTOR2 rhs)
	{
		return new INTVECTOR2(lhs.x - rhs.x, lhs.y - rhs.y);
	}

	public static INTVECTOR2 operator +(INTVECTOR2 lhs, INTVECTOR2 rhs)
	{
		return new INTVECTOR2(lhs.x + rhs.x, lhs.y + rhs.y);
	}

	public static implicit operator INTVECTOR2(Vector2 vec2)
	{
		return new INTVECTOR2(Mathf.FloorToInt(vec2.x + 1E-05f), Mathf.FloorToInt(vec2.y + 1E-05f));
	}

	public static implicit operator Vector2(INTVECTOR2 vec2)
	{
		return new Vector2(vec2.x, vec2.y);
	}
}
