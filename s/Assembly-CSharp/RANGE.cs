using UnityEngine;

public struct RANGE
{
	public enum RANGETYPE
	{
		Anywhere,
		Box,
		Sphere,
		Circle
	}

	public RANGETYPE type;

	public Vector3 center;

	public Vector3 extend;

	public float radius;

	public bool inverse;

	public static RANGE anywhere
	{
		get
		{
			RANGE result = default(RANGE);
			result.type = RANGETYPE.Anywhere;
			result.inverse = false;
			return result;
		}
	}

	public static RANGE nowhere
	{
		get
		{
			RANGE result = default(RANGE);
			result.type = RANGETYPE.Anywhere;
			result.inverse = true;
			return result;
		}
	}

	public bool Contains(Vector3 position)
	{
		if (type == RANGETYPE.Anywhere)
		{
			return (byte)((inverse ? 1u : 0u) ^ 1u) != 0;
		}
		if (type == RANGETYPE.Box)
		{
			return inverse ^ (Mathf.Abs(position.x - center.x) <= extend.x && Mathf.Abs(position.y - center.y) <= extend.y && Mathf.Abs(position.z - center.z) <= extend.z);
		}
		if (type == RANGETYPE.Sphere)
		{
			return inverse ^ (Vector3.Distance(position, center) <= radius);
		}
		if (type == RANGETYPE.Circle)
		{
			position.y = center.y;
			return inverse ^ (Vector3.Distance(position, center) <= radius);
		}
		return false;
	}
}
