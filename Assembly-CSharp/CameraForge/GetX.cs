namespace CameraForge;

[Menu("Component/Get X", 0)]
public class GetX : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetX()
	{
		V = new Slot("Vector");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return V.value.value_v.x;
		}
		return Var.Null;
	}
}
