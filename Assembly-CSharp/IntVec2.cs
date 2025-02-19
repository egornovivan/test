using UnityEngine;

public class IntVec2
{
	public int x;

	public int y;

	public IntVec2()
	{
	}

	public IntVec2(float _x, float _y)
	{
		x = Mathf.FloorToInt(_x);
		y = Mathf.FloorToInt(_y);
	}

	public IntVec2(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public IntVec2(IntVec2 _v2)
	{
		x = _v2.x;
		y = _v2.y;
	}

	public void BestMatchInt(float _x, float _y)
	{
		x = (int)Mathf.Floor(_x);
		y = (int)Mathf.Floor(_y);
		float num = _x - (float)(int)Mathf.Floor(_x);
		if (num >= 0.5f)
		{
			x++;
		}
		num = _y - (float)(int)Mathf.Floor(_y);
		if (num >= 0.5f)
		{
			y++;
		}
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
		IntVec2 intVec = _obj as IntVec2;
		if (x != intVec.x)
		{
			return false;
		}
		if (y != intVec.y)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode();
	}
}
