using UnityEngine;

namespace CameraForge;

[Menu("Math/Quaternion/LookAt", 60)]
public class LookAt : FunctionNode
{
	public Slot F;

	public Slot U;

	public override Slot[] slots => new Slot[2] { F, U };

	public LookAt()
	{
		F = new Slot("Forward");
		U = new Slot("Up");
		U.value = Vector3.up;
	}

	public override Var Calculate()
	{
		F.Calculate();
		if (!F.value.isNull)
		{
			Vector3 vector = F.value.value_v;
			if (vector == Vector3.zero)
			{
				return Quaternion.identity;
			}
			return Quaternion.LookRotation(F.value.value_v, U.value.value_v);
		}
		return Var.Null;
	}
}
