using UnityEngine;

namespace CameraForge;

[Menu("Variable/Color", 5)]
public class ColorVar : VarNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public ColorVar()
	{
		V = new Slot("Value");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			if (V.value.type == EVarType.Vector)
			{
				return new Color(V.value.value_v.x, V.value.value_v.y, V.value.value_v.z, V.value.value_v.w);
			}
			return V.value.value_c;
		}
		return Var.Null;
	}
}
