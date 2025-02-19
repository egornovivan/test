using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/DeltaAngle", 21)]
public class DeltaAngle : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public DeltaAngle()
	{
		A = new Slot("A");
		B = new Slot("B");
	}

	public override Var Calculate()
	{
		A.Calculate();
		B.Calculate();
		Var value = A.value;
		Var value2 = B.value;
		if (!value.isNull && !value2.isNull)
		{
			if (value.type == EVarType.Vector && value2.type == EVarType.Vector)
			{
				float x = Mathf.DeltaAngle(value.value_v.x, value2.value_v.x);
				float y = Mathf.DeltaAngle(value.value_v.y, value2.value_v.y);
				float z = Mathf.DeltaAngle(value.value_v.z, value2.value_v.z);
				float w = Mathf.DeltaAngle(value.value_v.w, value2.value_v.w);
				return new Vector4(x, y, z, w);
			}
			return Mathf.DeltaAngle(value.value_f, value2.value_f);
		}
		return Var.Null;
	}
}
