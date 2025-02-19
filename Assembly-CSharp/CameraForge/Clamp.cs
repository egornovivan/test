using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Clamp", 13)]
public class Clamp : FunctionNode
{
	public Slot X;

	public Slot Min;

	public Slot Max;

	public override Slot[] slots => new Slot[3] { X, Min, Max };

	public Clamp()
	{
		X = new Slot("X");
		Min = new Slot("Min");
		Max = new Slot("Max");
	}

	public override Var Calculate()
	{
		X.Calculate();
		Min.Calculate();
		Max.Calculate();
		Var value = X.value;
		Var value2 = Min.value;
		Var value3 = Max.value;
		Var var = value;
		if (!value.isNull && !value2.isNull && !value3.isNull)
		{
			if (value.type == EVarType.Bool || value.type == EVarType.Int)
			{
				if ((value2.type == EVarType.Bool || value2.type == EVarType.Int) && (value3.type == EVarType.Bool || value3.type == EVarType.Int))
				{
					var = ((var.value_i < value2.value_i) ? ((Var)value2.value_i) : ((var.value_i <= value3.value_i) ? ((Var)var.value_i) : ((Var)value3.value_i)));
				}
			}
			else if (value.type == EVarType.Float)
			{
				if ((value2.type == EVarType.Bool || value2.type == EVarType.Int || value2.type == EVarType.Float) && (value3.type == EVarType.Bool || value3.type == EVarType.Int || value3.type == EVarType.Float))
				{
					var = ((var.value_f < value2.value_f) ? ((Var)value2.value_f) : ((!(var.value_f > value3.value_f)) ? ((Var)var.value_f) : ((Var)value3.value_f)));
				}
			}
			else if (value.type == EVarType.Vector)
			{
				if (value2.type == EVarType.Vector && value3.type == EVarType.Vector)
				{
					Vector4 value_v = var.value_v;
					if (value_v.x < value2.value_v.x)
					{
						value_v.x = value2.value_v.x;
					}
					else if (value_v.x > value3.value_v.x)
					{
						value_v.x = value3.value_v.x;
					}
					if (value_v.y < value2.value_v.y)
					{
						value_v.y = value2.value_v.y;
					}
					else if (value_v.y > value3.value_v.y)
					{
						value_v.y = value3.value_v.y;
					}
					if (value_v.z < value2.value_v.z)
					{
						value_v.z = value2.value_v.z;
					}
					else if (value_v.z > value3.value_v.z)
					{
						value_v.z = value3.value_v.z;
					}
					if (value_v.w < value2.value_v.w)
					{
						value_v.w = value2.value_v.w;
					}
					else if (value_v.w > value3.value_v.w)
					{
						value_v.w = value3.value_v.w;
					}
					var = value_v;
				}
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Color && value3.type == EVarType.Color)
			{
				Color value_c = var.value_c;
				if (value_c.r < value2.value_c.r)
				{
					value_c.r = value2.value_c.r;
				}
				else if (value_c.r > value3.value_c.r)
				{
					value_c.r = value3.value_c.r;
				}
				if (value_c.g < value2.value_c.g)
				{
					value_c.g = value2.value_c.g;
				}
				else if (value_c.g > value3.value_c.g)
				{
					value_c.g = value3.value_c.g;
				}
				if (value_c.b < value2.value_c.b)
				{
					value_c.b = value2.value_c.b;
				}
				else if (value_c.b > value3.value_c.b)
				{
					value_c.b = value3.value_c.b;
				}
				if (value_c.a < value2.value_c.a)
				{
					value_c.a = value2.value_c.a;
				}
				else if (value_c.a > value3.value_c.a)
				{
					value_c.a = value3.value_c.a;
				}
				var = value_c;
			}
		}
		return var;
	}
}
