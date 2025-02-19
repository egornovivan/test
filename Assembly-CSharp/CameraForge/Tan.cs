using UnityEngine;

namespace CameraForge;

[Menu("Math/Trigon/Tan", 2)]
public class Tan : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Tan()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		if (!X.value.isNull)
		{
			float num = Mathf.Tan(X.value.value_f);
			if (float.IsNaN(num))
			{
				num = 0f;
			}
			return num;
		}
		return Var.Null;
	}
}
