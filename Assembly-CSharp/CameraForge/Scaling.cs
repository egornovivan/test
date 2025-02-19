using UnityEngine;

namespace CameraForge;

[Menu("Input/Transform/Scaling", 3)]
public class Scaling : InputNode
{
	public Slot O;

	public override Slot[] slots => new Slot[1] { O };

	public Scaling()
	{
		O = new Slot("Object");
	}

	public override Var Calculate()
	{
		O.Calculate();
		if (O.value.isNull)
		{
			return Var.Null;
		}
		string value_str = O.value.value_str;
		if (string.IsNullOrEmpty(value_str))
		{
			return Var.Null;
		}
		if (modifier == null)
		{
			return Var.Null;
		}
		if (modifier.controller == null)
		{
			return Var.Null;
		}
		if (modifier.controller.executor == null)
		{
			return Var.Null;
		}
		Transform transform = CameraController.GetTransform(value_str);
		if (transform == null)
		{
			return Var.Null;
		}
		return transform.localScale;
	}
}
