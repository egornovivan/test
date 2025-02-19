using UnityEngine;

namespace CameraForge;

[Menu("Math/Trigon/ACos", 11)]
public class ACos : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public ACos()
	{
		X = new Slot("X");
	}

	public override Var Calculate()
	{
		X.Calculate();
		if (!X.value.isNull)
		{
			return Mathf.Acos(Mathf.Clamp(X.value.value_f, -1f, 1f));
		}
		return Var.Null;
	}
}
