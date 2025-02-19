using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/Multiply", 2)]
public class Multiply : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public Multiply()
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
			else if (value.type == EVarType.Bool && value2.type == EVarType.Float)
			{
				result = value.value_f * value2.value_f;
			}
			else if (value.type == EVarType.Float && value2.type == EVarType.Bool)
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
				Vector4 vector = default(Vector4);
				vector.x = value.value_v.x * value2.value_v.x;
				vector.y = value.value_v.y * value2.value_v.y;
				vector.z = value.value_v.z * value2.value_v.z;
				vector.w = value.value_v.w * value2.value_v.w;
				result = vector;
			}
			else if (value.type == EVarType.Vector && value2.type == EVarType.Quaternion)
			{
				result = value2.value_q * value.value_v;
			}
			else if (value.type == EVarType.Quaternion && value2.type == EVarType.Vector)
			{
				result = value.value_q * value2.value_v;
			}
			else if (value.type == EVarType.Quaternion && value2.type == EVarType.Quaternion)
			{
				result = value.value_q * value2.value_q;
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
				result = value.value_c * value2.value_c;
			}
		}
		return result;
	}
}
