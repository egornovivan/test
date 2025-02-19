namespace CameraForge;

[Menu("Component/Get Yaw", 10)]
public class GetYaw : FunctionNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetYaw()
	{
		V = new Slot("Euler");
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
