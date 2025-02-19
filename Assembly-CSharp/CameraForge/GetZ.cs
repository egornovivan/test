namespace CameraForge;

[Menu("Component/Get Z", 2)]
public class GetZ : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetZ()
	{
		V = new Slot("Vector");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return V.value.value_v.z;
		}
		return Var.Null;
	}
}
