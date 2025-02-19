using UnityEngine;

namespace CameraForge;

[Menu("Input/Pose/FOV", 3)]
public class InputFOV : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		if (modifier == null)
		{
			return Vector3.zero;
		}
		return modifier.Prev.value.fov;
	}
}
