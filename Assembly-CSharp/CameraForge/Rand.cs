using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Rand", 50)]
public class Rand : FunctionNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		float value = Random.value;
		Var var = new Var();
		return value;
	}
}
