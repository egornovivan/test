using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Power", 4)]
public class Power : FunctionNode
{
	public Slot X;

	public Slot P;

	public override Slot[] slots => new Slot[2] { X, P };

	public Power()
	{
		X = new Slot("X");
		P = new Slot("P");
	}

	public override Var Calculate()
	{
		X.Calculate();
		P.Calculate();
		Var value = X.value;
		Var value2 = P.value;
		Var result = value;
		if (!value.isNull && !value2.isNull)
		{
			float value_f = value2.value_f;
			if (value.type == EVarType.Bool || value.type == EVarType.Int || value.type == EVarType.Float)
			{
				float num = 0f;
				try
				{
					num = Mathf.Pow(value.value_f, value_f);
				}
				catch
				{
					num = 0f;
				}
				if (float.IsNaN(num))
				{
					num = 0f;
				}
				result = num;
			}
			if (value.type == EVarType.Vector)
			{
				Vector4 value_v = value.value_v;
				try
				{
					value_v.x = Mathf.Pow(value_v.x, value_f);
				}
				catch
				{
					value_v.x = 0f;
				}
				if (float.IsNaN(value_v.x))
				{
					value_v.x = 0f;
				}
				try
				{
					value_v.y = Mathf.Pow(value_v.y, value_f);
				}
				catch
				{
					value_v.y = 0f;
				}
				if (float.IsNaN(value_v.y))
				{
					value_v.y = 0f;
				}
				try
				{
					value_v.z = Mathf.Pow(value_v.z, value_f);
				}
				catch
				{
					value_v.z = 0f;
				}
				if (float.IsNaN(value_v.z))
				{
					value_v.z = 0f;
				}
				try
				{
					value_v.w = Mathf.Pow(value_v.w, value_f);
				}
				catch
				{
					value_v.w = 0f;
				}
				if (float.IsNaN(value_v.w))
				{
					value_v.w = 0f;
				}
				result = value_v;
			}
			if (value.type == EVarType.Color)
			{
				Color value_c = value.value_c;
				try
				{
					value_c.r = Mathf.Pow(value_c.r, value_f);
				}
				catch
				{
					value_c.r = 0f;
				}
				if (float.IsNaN(value_c.r))
				{
					value_c.r = 0f;
				}
				try
				{
					value_c.g = Mathf.Pow(value_c.g, value_f);
				}
				catch
				{
					value_c.g = 0f;
				}
				if (float.IsNaN(value_c.g))
				{
					value_c.g = 0f;
				}
				try
				{
					value_c.b = Mathf.Pow(value_c.b, value_f);
				}
				catch
				{
					value_c.b = 0f;
				}
				if (float.IsNaN(value_c.b))
				{
					value_c.b = 0f;
				}
				try
				{
					value_c.a = Mathf.Pow(value_c.a, value_f);
				}
				catch
				{
					value_c.a = 0f;
				}
				if (float.IsNaN(value_c.a))
				{
					value_c.a = 0f;
				}
				result = value_c;
			}
		}
		return result;
	}
}
