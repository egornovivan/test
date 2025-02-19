namespace CameraForge;

[Menu("Variable/SetUserVar", 11)]
public class SetUserVar : MediaNode
{
	public Slot V;

	public Slot Value;

	public override Slot[] slots => new Slot[2] { V, Value };

	public SetUserVar()
	{
		V = new Slot("Var");
		Value = new Slot("Value");
	}

	public override Var Calculate()
	{
		V.Calculate();
		Value.Calculate();
		if (V.value.isNull)
		{
			return Value.value;
		}
		string text = V.value.value_str.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return Value.value;
		}
		if (modifier == null)
		{
			return Value.value;
		}
		if (modifier.controller == null)
		{
			return Value.value;
		}
		if (modifier.controller.executor == null)
		{
			return Value.value;
		}
		modifier.controller.executor.SetVar(text, Value.value);
		return Value.value;
	}
}
