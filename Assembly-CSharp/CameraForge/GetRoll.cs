namespace CameraForge;

[Menu("Component/Get Roll", 12)]
public class GetRoll : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetRoll()
	{
		V = new Slot("Euler");
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
