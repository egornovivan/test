using UnityEngine;

namespace CameraForge;

[Menu("Math/Vector/ClampLength", 6)]
public class ClampLength : FunctionNode
{
	public Slot V;

	public Slot Length;

	public override Slot[] slots => new Slot[2] { V, Length };

	public ClampLength()
	{
		V = new Slot("Vector");
		Length = new Slot("Length");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Length.Calculate();
		Var value = V.value;
		Var value2 = Length.value;
		Var result = value;
		if (!value.isNull && !value2.isNull)
		{
			float value_f = value2.value_f;
			if (value.type == EVarType.Vector)
			{
				Vector4 vector = value.value_v;
				if (vector.magnitude > value_f)
				{
					vector = vector.normalized * value_f;
				}
				result = vector;
			}
			if (value.type == EVarType.Color)
			{
				Color value_c = value.value_c;
				float grayscale = value_c.grayscale;
				if (grayscale > value_f)
				{
					value_c /= grayscale;
					value_c *= value_f;
				}
				result = value_c;
			}
		}
		return result;
	}
}
