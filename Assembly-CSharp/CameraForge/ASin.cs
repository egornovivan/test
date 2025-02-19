using UnityEngine;

namespace CameraForge;

[Menu("Math/Trigon/ASin", 10)]
public class ASin : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public ASin()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		if (!X.value.isNull)
		{
			return Mathf.Asin(Mathf.Clamp(X.value.value_f, -1f, 1f));
		}
		return Var.Null;
	}
}
