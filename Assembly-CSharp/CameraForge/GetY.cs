namespace CameraForge;

[Menu("Component/Get Y", 1)]
public class GetY : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetY()
	{
		V = new Slot("Vector");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return V.value.value_v.y;
		}
		return Var.Null;
	}
}
