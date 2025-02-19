namespace CameraForge;

[Menu("Variable/String", 8)]
public class StringVar : VarNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public StringVar()
	{
		V = new Slot("Value");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return V.value.value_str;
		}
		return Var.Null;
	}
}
