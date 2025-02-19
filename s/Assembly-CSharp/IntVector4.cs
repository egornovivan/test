using uLink;
using UnityEngine;

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

	public override bool Equals(object obj)
	{
		IntVector4 intVector = (IntVector4)obj;
		return x == intVector.x && y == intVector.y && z == intVector.z && w == intVector.w;
	}

	public override int GetHashCode()
	{
		return x + (z << 10) + (y << 20) + (w << 28);
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
public struct INTVECTOR4
{
	public int x;

	public int y;

	public int z;

	public int w;

	public int hash
	{
		get
		{
			return (x & 0x3FF) | ((z & 0x3FF) << 10) | ((y & 0x1FF) << 20) | (((w + 1) & 7) << 29);
		}
		set
		{
			x = value & 0x3FF;
			z = (value >> 10) & 0x3FF;
			y = (value >> 20) & 0x1FF;
			w = ((value >> 29) & 7) - 1;
			if (x >= 512)
			{
				x -= 1024;
			}
			if (y >= 256)
			{
				y -= 512;
			}
			if (z >= 512)
			{
				z -= 1024;
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
			x = value & 0x3FF;
			z = (value >> 10) & 0x3FF;
			y = (value >> 20) & 0x1FF;
			w = ((value >> 29) & 7) - 1;
		}
	}

	public static INTVECTOR4 zero => new INTVECTOR4(0, 0, 0, 0);

	public static INTVECTOR4 one => new INTVECTOR4(1, 1, 1, 1);

	public static INTVECTOR4 minusone => new INTVECTOR4(-1, -1, -1, -1);

	public static INTVECTOR4 unit_x => new INTVECTOR4(1, 0, 0, 0);

	public static INTVECTOR4 unit_y => new INTVECTOR4(0, 1, 0, 0);

	public static INTVECTOR4 unit_z => new INTVECTOR4(0, 0, 1, 0);

	public static INTVECTOR4 unit_w => new INTVECTOR4(0, 0, 0, 1);

	public INTVECTOR3 xyz => new INTVECTOR3(x, y, z);

	public Vector3 xyzf => new Vector3(x, y, z);

	public float sqrMagnitude => x * x + y * y + z * z + w * w;

	public float magnitude => Mathf.Sqrt(sqrMagnitude);

	public INTVECTOR4(int x_, int y_, int z_, int w_)
	{
		x = x_;
		y = y_;
		z = z_;
		w = w_;
	}

	public INTVECTOR4(INTVECTOR3 v3, int w_)
	{
		x = v3.x;
		y = v3.y;
		z = v3.z;
		w = w_;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is INTVECTOR4 iNTVECTOR)
		{
			return x == iNTVECTOR.x && y == iNTVECTOR.y && z == iNTVECTOR.z && w == iNTVECTOR.w;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public override string ToString()
	{
		return $"[{x},{y},{z},{w}]";
	}

	public static bool operator ==(INTVECTOR4 lhs, INTVECTOR4 rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
	}

	public static bool operator !=(INTVECTOR4 lhs, INTVECTOR4 rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z || lhs.w != rhs.w;
	}

	public static INTVECTOR4 operator *(INTVECTOR4 lhs, INTVECTOR4 rhs)
	{
		return new INTVECTOR4(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z, lhs.w * rhs.w);
	}

	public static INTVECTOR4 operator *(INTVECTOR4 lhs, int rhs)
	{
		return new INTVECTOR4(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
	}

	public static INTVECTOR4 operator /(INTVECTOR4 lhs, int rhs)
	{
		return new INTVECTOR4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);
	}

	public static INTVECTOR4 operator -(INTVECTOR4 lhs, INTVECTOR4 rhs)
	{
		return new INTVECTOR4(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z, lhs.w - rhs.w);
	}

	public static INTVECTOR4 operator +(INTVECTOR4 lhs, INTVECTOR4 rhs)
	{
		return new INTVECTOR4(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
	}

	public static implicit operator INTVECTOR4(Vector4 vec4)
	{
		return new INTVECTOR4(Mathf.FloorToInt(vec4.x + 1E-05f), Mathf.FloorToInt(vec4.y + 1E-05f), Mathf.FloorToInt(vec4.z + 1E-05f), Mathf.FloorToInt(vec4.w + 1E-05f));
	}

	public static implicit operator Vector4(INTVECTOR4 vec4)
	{
		return new Vector4(vec4.x, vec4.y, vec4.z, vec4.w);
	}
}
