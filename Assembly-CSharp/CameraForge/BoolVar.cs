using UnityEngine;

namespace CameraForge;

[Menu("Variable/Bool", 0)]
public class BoolVar : VarNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public BoolVar()
	{
		V = new Slot("Value");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return Mathf.Abs(V.value.value_f) < 1E-05f;
		}
		return Var.Null;
	}
}
