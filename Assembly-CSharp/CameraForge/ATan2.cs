using UnityEngine;

namespace CameraForge;

[Menu("Math/Trigon/ATan2", 13)]
public class ATan2 : FunctionNode
{
	public Slot X;

	public Slot Y;

	public override Slot[] slots => new Slot[2] { X, Y };

	public ATan2()
	{
		X = new Slot("X");
		Y = new Slot("Y");
	}

	public override Var Calculate()
	{
		X.Calculate();
		Y.Calculate();
		if (!X.value.isNull && !Y.value.isNull)
		{
			return Mathf.Atan2(Y.value.value_f, X.value.value_f);
		}
		return Var.Null;
	}
}
