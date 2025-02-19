using System;

namespace CameraForge;

[Menu("Math/Constant/Deg2Rad", 2)]
public class Deg2Rad : FunctionNode
{
	public Slot X;

	public override Slot[] slots => new Slot[1] { X };

	public Deg2Rad()
	{
		X = new Slot("Deg");
	}

	public override Var Calculate()
	{
		X.Calculate();
		if (X.value.isNull)
		{
			return Var.Null;
		}
		if (X.value.type == EVarType.Vector)
		{
			return X.value.value_v * ((float)Math.PI / 180f);
		}
		if (X.value.type == EVarType.Color)
		{
			return X.value.value_c * ((float)Math.PI / 180f);
		}
		return X.value.value_f * ((float)Math.PI / 180f);
	}
}
