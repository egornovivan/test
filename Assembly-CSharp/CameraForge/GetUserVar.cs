namespace CameraForge;

[Menu("Variable/GetUserVar", 10)]
public class GetUserVar : MediaNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public GetUserVar()
	{
		V = new Slot("Var");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (V.value.isNull)
		{
			return Var.Null;
		}
		string text = V.value.value_str.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return Var.Null;
		}
		if (modifier == null)
		{
			return Var.Null;
		}
		if (modifier.controller == null)
		{
			return Var.Null;
		}
		if (modifier.controller.executor == null)
		{
			return Var.Null;
		}
		return modifier.controller.executor.GetVar(text);
	}
}
