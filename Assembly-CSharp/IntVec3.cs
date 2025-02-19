using UnityEngine;

public class IntVec3
{
	public int x;

	public int y;

	public int z;

	public static IntVec3 zero => new IntVec3(0f, 0f, 0f);

	public static IntVec3 one => new IntVec3(1f, 1f, 1f);

	public static IntVec3 minusone => new IntVec3(-1f, -1f, -1f);

	public long _XYZ
	{
		get
		{
			long num = 1L;
			num <<= 20;
			num |= (uint)x;
			num <<= 20;
			num |= (uint)y;
			num <<= 24;
			return num + z;
		}
	}

	public IntVec3()
	{
		x = (y = (z = 0));
	}

	public IntVec3(float _x, float _y, float _z)
	{
		x = (int)Mathf.Floor(_x);
		y = (int)Mathf.Floor(_y);
		z = (int)Mathf.Floor(_z);
	}

	public IntVec3(Vector3 _v3)
	{
		x = (int)Mathf.Floor(_v3.x);
		y = (int)Mathf.Floor(_v3.y);
		z = (int)Mathf.Floor(_v3.z);
	}

	public IntVec3(IntVec3 _v3)
	{
		x = _v3.x;
		y = _v3.y;
		z = _v3.z;
	}

	public IntVec3(long _xyz)
	{
		x = (int)(_xyz >> 44);
		y = (int)(_xyz >> 24) & 0xFFFFF;
		z = (int)_xyz & 0xFFFFFF;
	}

	public void BestMatchInt(float _x, float _y, float _z)
	{
		x = Mathf.RoundToInt(_x);
		y = Mathf.RoundToInt(_y);
		z = Mathf.RoundToInt(_z);
	}

	public void BestMatchInt(Vector3 _v3)
	{
		x = Mathf.RoundToInt(_v3.x);
		y = Mathf.RoundToInt(_v3.y);
		z = Mathf.RoundToInt(_v3.z);
	}

	public override bool Equals(object _obj)
	{
		if (_obj == null)
		{
			return false;
		}
		if (_obj.GetType() != GetType())
		{
			return false;
		}
		IntVec3 intVec = _obj as IntVec3;
		if (x != intVec.x)
		{
			return false;
		}
		if (y != intVec.y)
		{
			return false;
		}
		if (z != intVec.z)
		{
			return false;
		}
		return true;
	}

	public Vector3 ToVector3()
	{
		return new Vector3(x, y, z);
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
	}

	public static IntVec3 operator *(IntVec3 mul0, IntVec3 mul1)
	{
		return new IntVec3(mul0.x * mul1.x, mul0.y * mul1.y, mul0.z * mul1.z);
	}

	public static IntVec3 operator *(IntVec3 mul0, int mul1)
	{
		return new IntVec3(mul0.x * mul1, mul0.y * mul1, mul0.z * mul1);
	}

	public static IntVec3 operator -(IntVec3 vec0, IntVec3 vec1)
	{
		return new IntVec3(vec0.x - vec1.x, vec0.y - vec1.y, vec0.z - vec1.z);
	}

	public static IntVec3 operator +(IntVec3 vec0, IntVec3 vec1)
	{
		return new IntVec3(vec0.x + vec1.x, vec0.y + vec1.y, vec0.z + vec1.z);
	}
}
