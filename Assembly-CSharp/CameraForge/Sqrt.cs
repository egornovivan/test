using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Sqrt", 5)]
public class Sqrt : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Sqrt()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		Var value = X.value;
		Var result = value;
		if (!value.isNull)
		{
			if (value.type == EVarType.Int)
			{
				result = sqrt(value.value_i);
			}
			else if (value.type == EVarType.Float)
			{
				result = sqrt(value.value_f);
			}
			else if (value.type == EVarType.Vector)
			{
				result = new Vector4(sqrt(value.value_v.x), sqrt(value.value_v.y), sqrt(value.value_v.z), sqrt(value.value_v.w));
			}
			else if (value.type == EVarType.Color)
			{
				result = new Color(sqrt(value.value_c.r), sqrt(value.value_c.g), sqrt(value.value_c.b), sqrt(value.value_c.a));
			}
		}
		return result;
	}

	private float sqrt(float x)
	{
		if (x < 0f)
		{
			return 0f;
		}
		return Mathf.Sqrt(x);
	}
}
