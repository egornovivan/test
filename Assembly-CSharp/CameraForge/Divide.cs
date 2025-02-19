using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Divide", 3)]
public class Divide : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public Divide()
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
			if (Mathf.Abs(value2.value_f) < 1E-05f)
			{
				return Var.Null;
			}
			if (value.type == EVarType.Bool && value2.type == EVarType.Int)
			{
				result = value.value_i / value2.value_i;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Bool)
			{
				result = value.value_i / value2.value_i;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Int)
			{
				result = value.value_i / value2.value_i;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Float)
			{
				result = value.value_f / value2.value_f;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Int)
			{
				result = value.value_f / value2.value_f;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Float)
			{
				result = value.value_f / value2.value_f;
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Int)
			{
				result = value.value_v / value2.value_f;
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Float)
			{
				result = value.value_v / value2.value_f;
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Int)
			{
				result = value.value_c / value2.value_f;
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Float)
			{
				result = value.value_c / value2.value_f;
			}
		}
		return result;
	}
}
