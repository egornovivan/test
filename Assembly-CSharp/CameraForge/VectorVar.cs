namespace CameraForge;

[Menu("Variable/Vector", 3)]
public class VectorVar : VarNode
{
	public Slot V;

	public override Slot[] slots => new Slot[1] { V };

	public VectorVar()
	{
		V = new Slot("Value");
	}

	public override Var Calculate()
	{
		V.Calculate();
		if (!V.value.isNull)
		{
			if (V.value.type == EVarType.Quaternion)
			{
				return V.value.value_q.eulerAngles;
			}
			return V.value.value_v;
		}
		return Var.Null;
	}
}
