namespace CameraForge;

[Menu("Component/Get W", 3)]
public class GetW : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetW()
	{
		V = new Slot("Vector");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return V.value.value_v.w;
		}
		return Var.Null;
	}
}
