namespace CameraForge;

[Menu("Variable/Float", 2)]
public class FloatVar : VarNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public FloatVar()
	{
		V = new Slot("Value");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			return V.value.value_f;
		}
		return Var.Null;
	}
}
