using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Down", 23)]
public class Down : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public Down()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Vector3 vector = Vector3.down;
		if (!V.value.isNull)
		{
			vector = V.value.value_q * Vector3.down;
		}
		return vector;
	}
}
