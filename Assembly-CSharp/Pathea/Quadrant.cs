using UnityEngine;

namespace Pathea;

public class Quadrant
{
	public static EQuadrant GetQuadrant(Vector3 v3)
	{
		EQuadrant eQuadrant = EQuadrant.None;
		int num;
		int num2;
		if (v3.x == 0f)
		{
			if (v3.z == 0f)
			{
				num = 1;
				num2 = 1;
			}
			else
			{
				num2 = (int)(v3.z / Mathf.Abs(v3.z));
				num = num2;
			}
		}
		else if (v3.z == 0f)
		{
			num = (int)(v3.x / Mathf.Abs(v3.x));
			num2 = num;
		}
		else
		{
			num = (int)(v3.x / Mathf.Abs(v3.x));
			num2 = (int)(v3.z / Mathf.Abs(v3.z));
		}
		if (num > 0)
		{
			if (num2 > 0)
			{
				return EQuadrant.Q1;
			}
			return EQuadrant.Q4;
		}
		if (num2 > 0)
		{
			return EQuadrant.Q2;
		}
		return EQuadrant.Q3;
	}

	public static EQuadrant Add(EQuadrant q)
	{
		int num = (int)(q + 1);
		if (num > 4)
		{
			return EQuadrant.Q1;
		}
		return (EQuadrant)num;
	}

	public static EQuadrant Minus(EQuadrant q)
	{
		int num = (int)(q - 1);
		if (num <= 0)
		{
			return EQuadrant.Q4;
		}
		return (EQuadrant)num;
	}
}
