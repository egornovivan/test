using UnityEngine;

namespace CameraForge;

[Menu("Input/Time/DeltaTime", 3)]
public class DeltaTime : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		return Time.deltaTime;
	}
}
