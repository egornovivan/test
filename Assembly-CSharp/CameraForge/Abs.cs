using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Abs", 20)]
public class Abs : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Abs()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		Var result = X.value;
		if (!X.value.isNull)
		{
			if (X.value.type == EVarType.Int)
			{
				result = Mathf.Abs(X.value.value_i);
			}
			else if (X.value.type == EVarType.Float)
			{
				result = Mathf.Abs(X.value.value_f);
			}
			else if (X.value.type == EVarType.Vector)
			{
				result = new Vector4(Mathf.Abs(X.value.value_v.x), Mathf.Abs(X.value.value_v.y), Mathf.Abs(X.value.value_v.z), Mathf.Abs(X.value.value_v.w));
			}
			else if (X.value.type == EVarType.Color)
			{
				result = new Color(Mathf.Abs(X.value.value_c.r), Mathf.Abs(X.value.value_c.g), Mathf.Abs(X.value.value_c.b), Mathf.Abs(X.value.value_c.a));
			}
		}
		return result;
	}
}
