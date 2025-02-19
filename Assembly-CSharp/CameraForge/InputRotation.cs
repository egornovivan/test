using UnityEngine;

namespace CameraForge;

[Menu("Input/Pose/Rotation", 2)]
public class InputRotation : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		if (modifier == null)
		{
			return Vector3.zero;
		}
		return modifier.Prev.value.rotation;
	}
}
