using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Max", 11)]
public class Max : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public Max()
	{
		A = new Slot("A");
		B = new Slot("B");
	}

	public override Var Calculate()
	{
		A.Calculate();
		B.Calculate();
		Var value = A.value;
		Var value2 = B.value;
		if (!value.isNull && !value2.isNull)
		{
			Var result = value;
			if (value.type == EVarType.Bool && value2.type == EVarType.Bool)
			{
				result = value.value_b || value2.value_b;
			}
			else if (value.type == EVarType.Bool && value2.type == EVarType.Int)
			{
				result = ((value.value_i <= value2.value_i) ? value2.value_i : value.value_i);
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Bool)
			{
				result = ((value.value_i <= value2.value_i) ? value2.value_i : value.value_i);
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Int)
			{
				result = ((value.value_i <= value2.value_i) ? value2.value_i : value.value_i);
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Float)
			{
				result = ((!(value.value_f > value2.value_f)) ? value2.value_f : value.value_f);
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Int)
			{
				result = ((!(value.value_f > value2.value_f)) ? value2.value_f : value.value_f);
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Float)
			{
				result = ((!(value.value_f > value2.value_f)) ? value2.value_f : value.value_f);
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Vector)
			{
				Vector4 zero = Vector4.zero;
				zero.x = ((!(value.value_v.x > value2.value_v.x)) ? value2.value_v.x : value.value_v.x);
				zero.y = ((!(value.value_v.y > value2.value_v.y)) ? value2.value_v.y : value.value_v.y);
				zero.z = ((!(value.value_v.z > value2.value_v.z)) ? value2.value_v.z : value.value_v.z);
				zero.w = ((!(value.value_v.w > value2.value_v.w)) ? value2.value_v.w : value.value_v.w);
				result = zero;
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Color)
			{
				Color clear = Color.clear;
				clear.r = ((!(value.value_c.r > value2.value_c.r)) ? value2.value_c.r : value.value_c.r);
				clear.g = ((!(value.value_c.g > value2.value_c.g)) ? value2.value_c.g : value.value_c.g);
				clear.b = ((!(value.value_c.b > value2.value_c.b)) ? value2.value_c.b : value.value_c.b);
				clear.a = ((!(value.value_c.a > value2.value_c.a)) ? value2.value_c.a : value.value_c.a);
				result = clear;
			}
			else if (value.type == EVarType.String && value2.type == EVarType.String)
			{
				result = ((string.Compare(value.value_str, value2.value_str) <= 0) ? value2.value_str : value.value_str);
			}
			return result;
		}
		if (!value.isNull)
		{
			return value;
		}
		if (!value2.isNull)
		{
			return value2;
		}
		return Var.Null;
	}
}
