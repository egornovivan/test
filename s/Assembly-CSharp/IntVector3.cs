using uLink;
using UnityEngine;

public class IntVector3
{
	public int x;

	public int y;

	public int z;

	public static IntVector3 Zero => new IntVector3(0, 0, 0);

	public static IntVector3 One => new IntVector3(1, 1, 1);

	public static IntVector3 UnitX => new IntVector3(1, 0, 0);

	public static IntVector3 UnitY => new IntVector3(0, 1, 0);

	public static IntVector3 UnitZ => new IntVector3(0, 0, 1);

	public IntVector3(IntVector3 vec3)
	{
		x = vec3.x;
		y = vec3.y;
		z = vec3.z;
	}

	public IntVector3()
	{
		x = 0;
		y = 0;
		z = 0;
	}

	public IntVector3(int x_)
	{
		x = x_;
		y = 0;
		z = 0;
	}

	public IntVector3(int x_, int y_)
	{
		x = x_;
		y = y_;
		z = 0;
	}

	public IntVector3(int x_, int y_, int z_)
	{
		x = x_;
		y = y_;
		z = z_;
	}

	public IntVector3(Vector3 xyz)
	{
		x = Mathf.FloorToInt(xyz.x + 0.5f);
		y = Mathf.FloorToInt(xyz.y + 0.5f);
		z = Mathf.FloorToInt(xyz.z + 0.5f);
	}

	public IntVector3(float x_, float y_, float z_)
	{
		x = Mathf.FloorToInt(x_ + 0.5f);
		y = Mathf.FloorToInt(y_ + 0.5f);
		z = Mathf.FloorToInt(z_ + 0.5f);
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
		return new IntVector3((int)(vec3.x + 0.5f), (int)(vec3.y + 0.5f), (int)(vec3.z + 0.5f));
	}

	public static implicit operator Vector3(IntVector3 vec)
	{
		return new Vector3(vec.x, vec.y, vec.z);
	}
}
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
