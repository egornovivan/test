using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Distance", 4)]
public class Distance : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public Distance()
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
		Var result = Var.Null;
		if (!value.isNull && !value2.isNull)
		{
			if (value.type == EVarType.Bool && value2.type == EVarType.Bool)
			{
				result = Mathf.Abs(value.value_i - value2.value_i);
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Int)
			{
				result = Mathf.Abs(value.value_i - value2.value_i);
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Float)
			{
				result = Mathf.Abs(value.value_f - value2.value_f);
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Int)
			{
				result = Mathf.Abs(value.value_f - value2.value_f);
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Float)
			{
				result = Mathf.Abs(value.value_f - value2.value_f);
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Vector)
			{
				result = Vector3.Distance(value.value_v, value2.value_v);
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Color)
			{
				result = Vector3.Distance(value.value_v, value2.value_v);
			}
		}
		return result;
	}
}
