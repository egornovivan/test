using UnityEngine;

namespace Pathea.Maths;

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
