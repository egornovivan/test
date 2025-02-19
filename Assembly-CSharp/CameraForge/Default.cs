namespace CameraForge;

[Menu("Logic/Default", 1)]
public class Default : FunctionNode
{
	public Slot A;

	public Slot B;

	public override Slot[] slots => new Slot[2] { A, B };

	public Default()
	{
		A = new Slot("Var");
		B = new Slot("Default");
	}

	public override Var Calculate()
	{
		A.Calculate();
		if (A.value.isNull)
		{
			B.Calculate();
			return B.value;
		}
		return A.value;
	}
}
