using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Forward", 20)]
public class Forward : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public Forward()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Vector3 vector = Vector3.forward;
		if (!V.value.isNull)
		{
			vector = V.value.value_q * Vector3.forward;
		}
		return vector;
	}
}
