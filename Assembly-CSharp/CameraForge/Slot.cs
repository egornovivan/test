namespace CameraForge;

public class Slot
{
	public string name;

	public Node input;

	public Var value;

	public Slot(string _name)
	{
		name = _name;
		input = null;
		value = Var.Null;
	}

	public void Calculate()
	{
		if (input != null)
		{
			value = input.Calculate();
		}
	}
}
