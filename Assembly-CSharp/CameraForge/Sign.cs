using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Sign", 20)]
public class Sign : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Sign()
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
				if (value.value_i > 0)
				{
					return 1;
				}
				if (value.value_i < 0)
				{
					return -1;
				}
				return 0;
			}
			if (value.type == EVarType.Float)
			{
				return sign(value.value_f);
			}
			if (value.type == EVarType.Vector)
			{
				result = new Vector4(sign(value.value_v.x), sign(value.value_v.y), sign(value.value_v.z), sign(value.value_v.w));
			}
			else if (value.type == EVarType.Color)
			{
				result = new Color(sign(value.value_c.r), sign(value.value_c.g), sign(value.value_c.b), sign(value.value_c.a));
			}
		}
		return result;
	}

	private float sign(float x)
	{
		if (x > 0f)
		{
			return 1f;
		}
		if (x < 0f)
		{
			return -1f;
		}
		return 0f;
	}
}
