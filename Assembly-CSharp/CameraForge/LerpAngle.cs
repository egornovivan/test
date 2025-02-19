using UnityEngine;

namespace CameraForge;

[Menu("Math/Standard/LerpAngle", 10)]
public class LerpAngle : FunctionNode
{
	public Slot A;

	public Slot B;

	public Slot T;

	public override Slot[] slots => new Slot[3] { A, B, T };

	public LerpAngle()
	{
		A = new Slot("A");
		B = new Slot("B");
		T = new Slot("T");
	}

	public override Var Calculate()
	{
		T.Calculate();
		if (!T.value.isNull)
		{
			float value_f = T.value.value_f;
			if (value_f == 0f)
			{
				A.Calculate();
				return A.value;
			}
			if (value_f == 1f)
			{
				B.Calculate();
				return B.value;
			}
			A.Calculate();
			B.Calculate();
			Var value = A.value;
			Var value2 = B.value;
			Var result = Var.Null;
			if (!value.isNull && !value2.isNull)
			{
				if (value.type == EVarType.Bool && value2.type == EVarType.Bool)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Bool && value2.type == EVarType.Int)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Int && value2.type == EVarType.Bool)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Int && value2.type == EVarType.Int)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Int && value2.type == EVarType.Float)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Float && value2.type == EVarType.Int)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Float && value2.type == EVarType.Float)
				{
					result = Mathf.LerpAngle(value.value_f, value2.value_f, value_f);
				}
				else if (value.type == EVarType.Vector && value2.type == EVarType.Vector)
				{
					Vector4 zero = Vector4.zero;
					zero.x = Mathf.LerpAngle(value.value_v.x, value2.value_v.x, value_f);
					zero.y = Mathf.LerpAngle(value.value_v.y, value2.value_v.y, value_f);
					zero.z = Mathf.LerpAngle(value.value_v.z, value2.value_v.z, value_f);
					zero.w = Mathf.LerpAngle(value.value_v.w, value2.value_v.w, value_f);
					result = zero;
				}
				else if (value.type == EVarType.Quaternion && value2.type == EVarType.Quaternion)
				{
					result = Quaternion.Slerp(value.value_q, value2.value_q, value_f);
				}
				else if (value.type == EVarType.Color && value2.type == EVarType.Color)
				{
					result = value.value_c * (1f - value_f) + value2.value_c * value_f;
				}
				else if (value.type == EVarType.String && value2.type == EVarType.String)
				{
					result = ((!(value_f < 0.5f)) ? value2.value_str : value.value_str);
				}
			}
			return result;
		}
		return Var.Null;
	}
}
