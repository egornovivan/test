using UnityEngine;

namespace CameraForge;

[Menu("Variable/RGBA", 7)]
public class RGBAVar : VarNode
{
	public Slot R;

	public Slot G;

	public Slot B;

	public Slot A;

	public override Slot[] slots => new Slot[4] { R, G, B, A };

	public RGBAVar()
	{
		R = new Slot("R");
		G = new Slot("G");
		B = new Slot("B");
		A = new Slot("A");
	}

	public override Var Calculate()
	{
		R.Calculate();
		G.Calculate();
		B.Calculate();
		A.Calculate();
		Color clear = Color.clear;
		if (!R.value.isNull)
		{
			clear.r = R.value.value_f;
		}
		if (!G.value.isNull)
		{
			clear.g = G.value.value_f;
		}
		if (!B.value.isNull)
		{
			clear.b = B.value.value_f;
		}
		if (!A.value.isNull)
		{
			clear.a = A.value.value_f;
		}
		return clear;
	}
}
