using UnityEngine;

namespace CameraForge;

[Menu("Math/Trigon/Sin", 0)]
public class Sin : FunctionNode
{
	public Slot A;

	public Slot W;

	public Slot X;

	public Slot Y;

	public Slot B;

	public override Slot[] slots => new Slot[5] { A, W, X, Y, B };

	public Sin()
	{
		A = new Slot("Scale");
		W = new Slot("Omega");
		X = new Slot("X");
		Y = new Slot("Phi");
		B = new Slot("Offset");
	}

	public override Var Calculate()
	{
		A.Calculate();
		W.Calculate();
		X.Calculate();
		Y.Calculate();
		B.Calculate();
		Vector4 vector = Vector4.one;
		Vector4 vector2 = Vector4.one;
		Vector4 vector3 = Vector4.zero;
		Vector4 vector4 = Vector4.zero;
		Vector4 vector5 = Vector4.zero;
		if (!A.value.isNull)
		{
			vector = A.value.value_v;
		}
		if (!W.value.isNull)
		{
			vector2 = W.value.value_v;
		}
		if (!X.value.isNull)
		{
			vector5 = X.value.value_v;
		}
		if (!Y.value.isNull)
		{
			vector3 = Y.value.value_v;
		}
		if (!B.value.isNull)
		{
			vector4 = B.value.value_v;
		}
		if (A.value.type == EVarType.Vector || A.value.type == EVarType.Color || W.value.type == EVarType.Vector || W.value.type == EVarType.Color || X.value.type == EVarType.Vector || X.value.type == EVarType.Color || Y.value.type == EVarType.Vector || Y.value.type == EVarType.Color || B.value.type == EVarType.Vector || B.value.type == EVarType.Color)
		{
			Vector4 zero = Vector4.zero;
			zero.x = vector.x * Mathf.Sin(vector2.x * vector5.x + vector3.x) + vector4.x;
			zero.y = vector.y * Mathf.Sin(vector2.y * vector5.y + vector3.y) + vector4.y;
			zero.z = vector.z * Mathf.Sin(vector2.z * vector5.z + vector3.z) + vector4.z;
			zero.w = vector.w * Mathf.Sin(vector2.w * vector5.w + vector3.w) + vector4.w;
			return zero;
		}
		float num = vector.x * Mathf.Sin(vector2.x * vector5.x + vector3.x) + vector4.x;
		return num;
	}
}
