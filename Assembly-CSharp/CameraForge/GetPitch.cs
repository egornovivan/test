namespace CameraForge;

[Menu("Component/Get Pitch", 11)]
public class GetPitch : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetPitch()
	{
		V = new Slot("Euler");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return 0f - V.value.value_v.x;
		}
		return Var.Null;
	}
}
