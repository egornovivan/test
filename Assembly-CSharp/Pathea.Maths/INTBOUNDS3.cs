using UnityEngine;

namespace Pathea.Maths;

public struct INTBOUNDS3
{
	public int close_max;

	public INTVECTOR3 min;

	public INTVECTOR3 max;

	public INTVECTOR3 absmin => new INTVECTOR3((min.x >= max.x) ? max.x : min.x, (min.y >= max.y) ? max.y : min.y, (min.z >= max.z) ? max.z : min.z);

	public INTVECTOR3 absmax => new INTVECTOR3((min.x <= max.x) ? max.x : min.x, (min.y <= max.y) ? max.y : min.y, (min.z <= max.z) ? max.z : min.z);

	public INTVECTOR3 size => new INTVECTOR3(max.x - min.x + close_max, max.y - min.y + close_max, max.z - min.z + close_max);

	public INTVECTOR3 abssize => new INTVECTOR3(Mathf.Abs(max.x - min.x) + close_max, Mathf.Abs(max.y - min.y) + close_max, Mathf.Abs(max.z - min.z) + close_max);

	public INTBOUNDS3(INTVECTOR3 _min, INTVECTOR3 _max)
	{
		close_max = 0;
		min = _min;
		max = _max;
	}

	public INTBOUNDS3(INTVECTOR3 _min, INTVECTOR3 _max, bool _close_min, bool _close_max)
	{
		close_max = (_close_max ? 1 : 0);
		min = _min;
		max = _max;
	}

	public INTBOUNDS3(INTVECTOR3 _min, INTVECTOR3 _max, bool _close_max)
	{
		close_max = (_close_max ? 1 : 0);
		min = _min;
		max = _max;
	}

	public void OpenBounds()
	{
		max += INTVECTOR3.one * close_max;
		close_max = 0;
	}

	public bool Contains(INTVECTOR3 point)
	{
		return point.x >= min.x && point.x < max.x + close_max && point.y >= min.y && point.y < max.y + close_max && point.z >= min.z && point.z < max.z + close_max;
	}

	public INTVECTOR3 ClampInBound(INTVECTOR3 pos)
	{
		if (pos.x < min.x)
		{
			pos.x = min.x;
		}
		if (pos.x > max.x - 1 + close_max)
		{
			pos.x = max.x - 1 + close_max;
		}
		if (pos.y < min.y)
		{
			pos.y = min.y;
		}
		if (pos.y > max.y - 1 + close_max)
		{
			pos.y = max.y - 1 + close_max;
		}
		if (pos.z < min.z)
		{
			pos.z = min.z;
		}
		if (pos.z > max.z - 1 + close_max)
		{
			pos.z = max.z - 1 + close_max;
		}
		return pos;
	}

	public override bool Equals(object other)
	{
		if (!(other is INTBOUNDS3 iNTBOUNDS))
		{
			return false;
		}
		return min.Equals(iNTBOUNDS.min) && (max + INTVECTOR3.one * close_max).Equals(iNTBOUNDS.max + INTVECTOR3.one * iNTBOUNDS.close_max);
	}

	public override int GetHashCode()
	{
		return min.hash + max.hash;
	}

	public override string ToString()
	{
		return "min : " + min.ToString() + "  max : " + max.ToString();
	}

	public static implicit operator Bounds(INTBOUNDS3 bound)
	{
		Bounds result = default(Bounds);
		result.SetMinMax(bound.min, bound.max + INTVECTOR3.one * bound.close_max);
		return result;
	}

	public static INTBOUNDS3 operator *(INTBOUNDS3 lhs, int rhs)
	{
		if (lhs.close_max == 0)
		{
			return new INTBOUNDS3(lhs.min * rhs, lhs.max * rhs);
		}
		INTVECTOR3 iNTVECTOR = new INTVECTOR3(lhs.close_max, lhs.close_max, lhs.close_max);
		INTBOUNDS3 result = new INTBOUNDS3(lhs.min * rhs, (lhs.max + iNTVECTOR) * rhs - iNTVECTOR);
		result.close_max = lhs.close_max;
		return result;
	}

	public static INTBOUNDS3 operator &(INTBOUNDS3 lhs, INTBOUNDS3 rhs)
	{
		INTBOUNDS3 result = default(INTBOUNDS3);
		int num = rhs.close_max - lhs.close_max;
		rhs.close_max = lhs.close_max;
		rhs.max += INTVECTOR3.one * num;
		result.min.x = ((lhs.min.x <= rhs.min.x) ? rhs.min.x : lhs.min.x);
		result.min.y = ((lhs.min.y <= rhs.min.y) ? rhs.min.y : lhs.min.y);
		result.min.z = ((lhs.min.z <= rhs.min.z) ? rhs.min.z : lhs.min.z);
		result.max.x = ((lhs.max.x >= rhs.max.x) ? rhs.max.x : lhs.max.x);
		result.max.y = ((lhs.max.y >= rhs.max.y) ? rhs.max.y : lhs.max.y);
		result.max.z = ((lhs.max.z >= rhs.max.z) ? rhs.max.z : lhs.max.z);
		result.close_max = lhs.close_max;
		return result;
	}

	public static INTBOUNDS3 operator |(INTBOUNDS3 lhs, INTBOUNDS3 rhs)
	{
		INTBOUNDS3 result = default(INTBOUNDS3);
		int num = rhs.close_max - lhs.close_max;
		rhs.close_max = lhs.close_max;
		rhs.max += INTVECTOR3.one * num;
		result.min.x = ((lhs.min.x >= rhs.min.x) ? rhs.min.x : lhs.min.x);
		result.min.y = ((lhs.min.y >= rhs.min.y) ? rhs.min.y : lhs.min.y);
		result.min.z = ((lhs.min.z >= rhs.min.z) ? rhs.min.z : lhs.min.z);
		result.max.x = ((lhs.max.x <= rhs.max.x) ? rhs.max.x : lhs.max.x);
		result.max.y = ((lhs.max.y <= rhs.max.y) ? rhs.max.y : lhs.max.y);
		result.max.z = ((lhs.max.z <= rhs.max.z) ? rhs.max.z : lhs.max.z);
		result.close_max = lhs.close_max;
		return result;
	}

	public static bool operator ==(INTBOUNDS3 lhs, INTBOUNDS3 rhs)
	{
		return lhs.min == rhs.min && lhs.max + INTVECTOR3.one * lhs.close_max == rhs.max + INTVECTOR3.one * rhs.close_max;
	}

	public static bool operator !=(INTBOUNDS3 lhs, INTBOUNDS3 rhs)
	{
		return !(lhs == rhs);
	}
}
