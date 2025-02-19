using UnityEngine;

namespace Pathea.Maths;

public struct INTVECTOR3
{
	public int x;

	public int y;

	public int z;

	public int hash
	{
		get
		{
			return (x & 0x7FF) | ((z & 0x7FF) << 11) | ((y & 0x3FF) << 22);
		}
		set
		{
			x = value & 0x7FF;
			z = (value >> 11) & 0x7FF;
			y = (value >> 22) & 0x3FF;
			if (x >= 1024)
			{
				x -= 2048;
			}
			if (y >= 512)
			{
				y -= 1024;
			}
			if (z >= 1024)
			{
				z -= 2048;
			}
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
			x = value & 0x7FF;
			z = (value >> 11) & 0x7FF;
			y = (value >> 22) & 0x3FF;
		}
	}

	public static INTVECTOR3 zero => new INTVECTOR3(0, 0, 0);

	public static INTVECTOR3 one => new INTVECTOR3(1, 1, 1);

	public static INTVECTOR3 minusone => new INTVECTOR3(-1, -1, -1);

	public static INTVECTOR3 unit_x => new INTVECTOR3(1, 0, 0);

	public static INTVECTOR3 unit_y => new INTVECTOR3(0, 1, 0);

	public static INTVECTOR3 unit_z => new INTVECTOR3(0, 0, 1);

	public INTVECTOR2 xy => new INTVECTOR2(x, y);

	public INTVECTOR2 yx => new INTVECTOR2(y, x);

	public INTVECTOR2 zx => new INTVECTOR2(z, x);

	public INTVECTOR2 xz => new INTVECTOR2(x, z);

	public INTVECTOR2 yz => new INTVECTOR2(y, z);

	public INTVECTOR2 zy => new INTVECTOR2(z, y);

	public float sqrMagnitude => x * x + y * y + z * z;

	public float magnitude => Mathf.Sqrt(sqrMagnitude);

	public INTVECTOR3(INTVECTOR3 vec)
	{
		x = vec.x;
		y = vec.y;
		z = vec.z;
	}

	public INTVECTOR3(int x_, int y_, int z_)
	{
		x = x_;
		y = y_;
		z = z_;
	}

	public INTVECTOR3(float x_, float y_, float z_)
	{
		x = Mathf.FloorToInt(x_ + 1E-05f);
		y = Mathf.FloorToInt(y_ + 1E-05f);
		z = Mathf.FloorToInt(z_ + 1E-05f);
	}

	public float Distance(INTVECTOR3 vec)
	{
		return Mathf.Sqrt((vec.x - x) * (vec.x - x) + (vec.y - y) * (vec.y - y) + (vec.z - z) * (vec.z - z));
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is INTVECTOR3 iNTVECTOR)
		{
			return x == iNTVECTOR.x && y == iNTVECTOR.y && z == iNTVECTOR.z;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public override string ToString()
	{
		return $"[{x},{y},{z}]";
	}

	public static bool operator ==(INTVECTOR3 lhs, INTVECTOR3 rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	public static bool operator !=(INTVECTOR3 lhs, INTVECTOR3 rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	public static INTVECTOR3 operator *(INTVECTOR3 lhs, INTVECTOR3 rhs)
	{
		return new INTVECTOR3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
	}

	public static INTVECTOR3 operator *(INTVECTOR3 lhs, int rhs)
	{
		return new INTVECTOR3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
	}

	public static INTVECTOR3 operator /(INTVECTOR3 lhs, int rhs)
	{
		return new INTVECTOR3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
	}

	public static INTVECTOR3 operator -(INTVECTOR3 lhs, INTVECTOR3 rhs)
	{
		return new INTVECTOR3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
	}

	public static INTVECTOR3 operator +(INTVECTOR3 lhs, INTVECTOR3 rhs)
	{
		return new INTVECTOR3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
	}

	public static implicit operator INTVECTOR3(Vector3 vec3)
	{
		return new INTVECTOR3(Mathf.FloorToInt(vec3.x + 1E-05f), Mathf.FloorToInt(vec3.y + 1E-05f), Mathf.FloorToInt(vec3.z + 1E-05f));
	}

	public static implicit operator Vector3(INTVECTOR3 vec3)
	{
		return new Vector3(vec3.x, vec3.y, vec3.z);
	}
}
