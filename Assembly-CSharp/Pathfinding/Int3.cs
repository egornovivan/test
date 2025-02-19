using System;
using UnityEngine;

namespace Pathfinding;

public struct Int3
{
	public const int Precision = 1000;

	public const float FloatPrecision = 1000f;

	public const float PrecisionFactor = 0.001f;

	public int x;

	public int y;

	public int z;

	private static Int3 _zero = new Int3(0, 0, 0);

	public static Int3 zero => _zero;

	public int this[int i]
	{
		get
		{
			return i switch
			{
				0 => x, 
				1 => y, 
				_ => z, 
			};
		}
		set
		{
			switch (i)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				z = value;
				break;
			}
		}
	}

	public float magnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
		}
	}

	public int costMagnitude => (int)Math.Round(magnitude);

	public float worldMagnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3) * 0.001f;
		}
	}

	public float sqrMagnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return (float)(num * num + num2 * num2 + num3 * num3);
		}
	}

	public long sqrMagnitudeLong
	{
		get
		{
			long num = x;
			long num2 = y;
			long num3 = z;
			return num * num + num2 * num2 + num3 * num3;
		}
	}

	public int unsafeSqrMagnitude => x * x + y * y + z * z;

	[Obsolete("Same implementation as .magnitude")]
	public float safeMagnitude
	{
		get
		{
			double num = x;
			double num2 = y;
			double num3 = z;
			return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
		}
	}

	[Obsolete(".sqrMagnitude is now per default safe (.unsafeSqrMagnitude can be used for unsafe operations)")]
	public float safeSqrMagnitude
	{
		get
		{
			float num = (float)x * 0.001f;
			float num2 = (float)y * 0.001f;
			float num3 = (float)z * 0.001f;
			return num * num + num2 * num2 + num3 * num3;
		}
	}

	public Int3(Vector3 position)
	{
		x = (int)Math.Round(position.x * 1000f);
		y = (int)Math.Round(position.y * 1000f);
		z = (int)Math.Round(position.z * 1000f);
	}

	public Int3(int _x, int _y, int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}

	public Int3 DivBy2()
	{
		x >>= 1;
		y >>= 1;
		z >>= 1;
		return this;
	}

	public static float Angle(Int3 lhs, Int3 rhs)
	{
		double num = (double)Dot(lhs, rhs) / ((double)lhs.magnitude * (double)rhs.magnitude);
		num = ((num < -1.0) ? (-1.0) : ((!(num > 1.0)) ? num : 1.0));
		return (float)Math.Acos(num);
	}

	public static int Dot(Int3 lhs, Int3 rhs)
	{
		return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z;
	}

	public static long DotLong(Int3 lhs, Int3 rhs)
	{
		return (long)lhs.x * (long)rhs.x + (long)lhs.y * (long)rhs.y + (long)lhs.z * (long)rhs.z;
	}

	public Int3 Normal2D()
	{
		return new Int3(z, y, -x);
	}

	public Int3 NormalizeTo(int newMagn)
	{
		float num = magnitude;
		if (num == 0f)
		{
			return this;
		}
		x *= newMagn;
		y *= newMagn;
		z *= newMagn;
		x = (int)Math.Round((float)x / num);
		y = (int)Math.Round((float)y / num);
		z = (int)Math.Round((float)z / num);
		return this;
	}

	public override string ToString()
	{
		return "( " + x + ", " + y + ", " + z + ")";
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		Int3 @int = (Int3)o;
		return x == @int.x && y == @int.y && z == @int.z;
	}

	public override int GetHashCode()
	{
		return (x * 73856093) ^ (y * 19349663) ^ (z * 83492791);
	}

	public static bool operator ==(Int3 lhs, Int3 rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
	}

	public static bool operator !=(Int3 lhs, Int3 rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
	}

	public static explicit operator Int3(Vector3 ob)
	{
		return new Int3((int)Math.Round(ob.x * 1000f), (int)Math.Round(ob.y * 1000f), (int)Math.Round(ob.z * 1000f));
	}

	public static explicit operator Vector3(Int3 ob)
	{
		return new Vector3((float)ob.x * 0.001f, (float)ob.y * 0.001f, (float)ob.z * 0.001f);
	}

	public static Int3 operator -(Int3 lhs, Int3 rhs)
	{
		lhs.x -= rhs.x;
		lhs.y -= rhs.y;
		lhs.z -= rhs.z;
		return lhs;
	}

	public static Int3 operator -(Int3 lhs)
	{
		lhs.x = -lhs.x;
		lhs.y = -lhs.y;
		lhs.z = -lhs.z;
		return lhs;
	}

	public static Int3 operator +(Int3 lhs, Int3 rhs)
	{
		lhs.x += rhs.x;
		lhs.y += rhs.y;
		lhs.z += rhs.z;
		return lhs;
	}

	public static Int3 operator *(Int3 lhs, int rhs)
	{
		lhs.x *= rhs;
		lhs.y *= rhs;
		lhs.z *= rhs;
		return lhs;
	}

	public static Int3 operator *(Int3 lhs, float rhs)
	{
		lhs.x = (int)Math.Round((float)lhs.x * rhs);
		lhs.y = (int)Math.Round((float)lhs.y * rhs);
		lhs.z = (int)Math.Round((float)lhs.z * rhs);
		return lhs;
	}

	public static Int3 operator *(Int3 lhs, double rhs)
	{
		lhs.x = (int)Math.Round((double)lhs.x * rhs);
		lhs.y = (int)Math.Round((double)lhs.y * rhs);
		lhs.z = (int)Math.Round((double)lhs.z * rhs);
		return lhs;
	}

	public static Int3 operator *(Int3 lhs, Vector3 rhs)
	{
		lhs.x = (int)Math.Round((float)lhs.x * rhs.x);
		lhs.y = (int)Math.Round((float)lhs.y * rhs.y);
		lhs.z = (int)Math.Round((float)lhs.z * rhs.z);
		return lhs;
	}

	public static Int3 operator /(Int3 lhs, float rhs)
	{
		lhs.x = (int)Math.Round((float)lhs.x / rhs);
		lhs.y = (int)Math.Round((float)lhs.y / rhs);
		lhs.z = (int)Math.Round((float)lhs.z / rhs);
		return lhs;
	}

	public static implicit operator string(Int3 ob)
	{
		return ob.ToString();
	}
}
