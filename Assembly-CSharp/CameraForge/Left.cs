using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Left", 25)]
public class Left : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public Left()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Vector3 vector = Vector3.left;
		if (!V.value.isNull)
		{
			vector = V.value.value_q * Vector3.left;
		}
		return vector;
	}
}
