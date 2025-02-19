using UnityEngine;

namespace CameraForge;

[Menu("Input/Pose/EulerAngles", 1)]
public class InputEulerAngles : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		if (modifier == null)
		{
			return Vector3.zero;
		}
		return modifier.Prev.value.eulerAngles;
	}
}
