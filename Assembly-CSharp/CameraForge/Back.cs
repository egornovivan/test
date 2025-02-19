using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Back", 21)]
public class Back : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public Back()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Vector3 vector = Vector3.back;
		if (!V.value.isNull)
		{
			vector = V.value.value_q * Vector3.back;
		}
		return vector;
	}
}
