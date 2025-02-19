using UnityEngine;

namespace CameraForge;

[Menu("Input/Time/GlobalTime", 0)]
public class GlobalTime : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		return Time.time;
	}
}
