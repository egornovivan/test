using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Round", 8)]
public class Round : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Round()
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
			if (value.type == EVarType.Float)
			{
				result = func(value.value_f);
			}
			else if (value.type == EVarType.Vector)
			{
				result = new Vector4(func(value.value_v.x), func(value.value_v.y), func(value.value_v.z), func(value.value_v.w));
			}
			else if (value.type == EVarType.Color)
			{
				result = new Color(func(value.value_c.r), func(value.value_c.g), func(value.value_c.b), func(value.value_c.a));
			}
		}
		return result;
	}

	private float func(float x)
	{
		return Mathf.Round(x);
	}
}
