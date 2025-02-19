using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Right", 24)]
public class Right : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public Right()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Vector3 vector = Vector3.right;
		if (!V.value.isNull)
		{
			vector = V.value.value_q * Vector3.right;
		}
		return vector;
	}
}
