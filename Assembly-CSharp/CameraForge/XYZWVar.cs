using UnityEngine;

namespace CameraForge;

[Menu("Variable/XYZW", 6)]
public class XYZWVar : VarNode
{
	public Slot X;

	public Slot Y;

	public Slot Z;

	public Slot W;

	public override Slot[] slots => new Slot[4] { X, Y, Z, W };

	public XYZWVar()
	{
		X = new Slot("X");
		Y = new Slot("Y");
		Z = new Slot("Z");
		W = new Slot("W");
	}

	public override Var Calculate()
	{
		X.Calculate();
		Y.Calculate();
		Z.Calculate();
		W.Calculate();
		Vector4 zero = Vector4.zero;
		if (!X.value.isNull)
		{
			zero.x = X.value.value_f;
		}
		if (!Y.value.isNull)
		{
			zero.y = Y.value.value_f;
		}
		if (!Z.value.isNull)
		{
			zero.z = Z.value.value_f;
		}
		if (!W.value.isNull)
		{
			zero.w = W.value.value_f;
		}
		return zero;
	}
}
