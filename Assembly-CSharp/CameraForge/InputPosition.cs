using UnityEngine;

namespace CameraForge;

[Menu("Input/Pose/Position", 0)]
public class InputPosition : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		if (modifier == null)
		{
			return Vector3.zero;
		}
		return modifier.Prev.value.position;
	}
}
