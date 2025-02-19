using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/Cross", 3)]
public class Cross : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public Cross()
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
				result = value.value_b && value2.value_b;
			}
			else if (value.type == EVarType.Bool && value2.type == EVarType.Int)
			{
				result = value.value_i * value2.value_i;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Bool)
			{
				result = value.value_i * value2.value_i;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Int)
			{
				result = value.value_i * value2.value_i;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Float)
			{
				result = value.value_f * value2.value_f;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Int)
			{
				result = value.value_f * value2.value_f;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Float)
			{
				result = value.value_f * value2.value_f;
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Int)
			{
				result = value.value_v * value2.value_f;
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Float)
			{
				result = value.value_v * value2.value_f;
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Vector)
			{
				result = value.value_f * value2.value_v;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Vector)
			{
				result = value.value_f * value2.value_v;
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Vector)
			{
				result = Vector3.Cross(new Vector3(value.value_v.x, value.value_v.y, value.value_v.z), new Vector3(value2.value_v.x, value2.value_v.y, value2.value_v.z));
			}
			else if (value.type == EVarType.Int && value2.type == EVarType.Color)
			{
				result = value.value_f * value2.value_c;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Color)
			{
				result = value.value_f * value2.value_c;
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Int)
			{
				result = value.value_c * value2.value_f;
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Float)
			{
				result = value.value_c * value2.value_f;
			}
			else if (value.type == EVarType.Color && value2.type == EVarType.Color)
			{
				result = Vector3.Cross(new Vector3(value.value_v.x, value.value_v.y, value.value_v.z), new Vector3(value2.value_v.x, value2.value_v.y, value2.value_v.z));
			}
		}
		return result;
	}
}
