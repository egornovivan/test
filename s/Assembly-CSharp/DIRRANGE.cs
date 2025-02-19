using UnityEngine;

public struct DIRRANGE
{
	public enum DIRRANGETYPE
	{
		Anydirection,
		Cone,
		Fan
	}

	public DIRRANGETYPE type;

	public Vector3 directrix;

	public Vector2 error;

	public bool inverse;

	public static DIRRANGE anydir
	{
		get
		{
			DIRRANGE result = default(DIRRANGE);
			result.type = DIRRANGETYPE.Anydirection;
			result.inverse = false;
			return result;
		}
	}

	public static DIRRANGE nodir
	{
		get
		{
			DIRRANGE result = default(DIRRANGE);
			result.type = DIRRANGETYPE.Anydirection;
			result.inverse = true;
			return result;
		}
	}

	public bool Contains(Vector3 dir)
	{
		if (type == DIRRANGETYPE.Anydirection)
		{
			return (byte)((inverse ? 1u : 0u) ^ 1u) != 0;
		}
		if (type == DIRRANGETYPE.Cone)
		{
			return inverse ^ (Vector3.Angle(directrix, dir) <= error.x);
		}
		if (type == DIRRANGETYPE.Fan)
		{
			Vector3 from = dir;
			Vector3 to = directrix;
			from.y = (to.y = 0f);
			bool flag = Vector3.Angle(from, to) <= error.x;
			from = dir.normalized;
			to = directrix.normalized;
			float num = Mathf.Asin(from.y) * 57.29578f;
			float num2 = Mathf.Asin(to.y) * 57.29578f;
			bool flag2 = Mathf.Abs(num - num2) <= error.y;
			return inverse ^ (flag && flag2);
		}
		return false;
	}
}
