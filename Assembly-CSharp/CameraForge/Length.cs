using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Length", 0)]
public class Length : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Length()
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
			if (value.type == EVarType.Bool)
			{
				result = value.value_i;
			}
			else if (value.type == EVarType.Int)
			{
				result = Mathf.Abs(value.value_i);
			}
			else if (value.type == EVarType.Float)
			{
				result = Mathf.Abs(value.value_f);
			}
			else if (value.type == EVarType.Vector)
			{
				result = value.value_v.magnitude;
			}
			else if (value.type == EVarType.Quaternion)
			{
				result = 1f;
			}
			else if (value.type == EVarType.Color)
			{
				result = value.value_c.grayscale * value.value_c.a;
			}
			else if (value.type == EVarType.String)
			{
				result = value.value_str.Length;
			}
		}
		return result;
	}
}
