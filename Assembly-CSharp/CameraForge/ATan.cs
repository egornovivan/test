using UnityEngine;

namespace CameraForge;

[Menu("Math/Trigon/ATan", 12)]
public class ATan : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public ATan()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		if (!X.value.isNull)
		{
			return Mathf.Atan(X.value.value_f);
		}
		return Var.Null;
	}
}
