using UnityEngine;

namespace CameraForge;

[Menu("Input/Pose/Near Clip", 4)]
public class InputNearclip : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		if (modifier == null)
		{
			return Vector3.zero;
		}
		return modifier.Prev.value.nearClip;
	}
}
