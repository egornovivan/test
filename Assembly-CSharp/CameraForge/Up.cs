using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Up", 22)]
public class Up : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public Up()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Vector3 vector = Vector3.up;
		if (!V.value.isNull)
		{
			vector = V.value.value_q * Vector3.up;
		}
		return vector;
	}
}
