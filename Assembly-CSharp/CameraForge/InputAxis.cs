namespace CameraForge;

[Menu("Input/InputAxis", -1)]
public class InputAxis : InputNode
{
	public Slot A;

	public override Slot[] slots => new Slot[1] { A };

	public InputAxis()
	{
		A = new Slot("Axis");
	}

	public override Var Calculate()
	{
		A.Calculate();
		if (A.value.isNull)
		{
			return 0f;
		}
		string value_str = A.value.value_str;
		if (string.IsNullOrEmpty(value_str))
		{
			return 0f;
		}
		return InputModule.Axis(value_str);
	}
}
