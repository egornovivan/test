using System;

namespace CameraForge;

[Menu("Math/Constant/PI", 0)]
public class PI : FunctionNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		return (float)Math.PI;
	}
}
