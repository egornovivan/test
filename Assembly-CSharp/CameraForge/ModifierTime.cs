namespace CameraForge;

[Menu("Input/Time/ModifierTime", 2)]
public class ModifierTime : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		return (modifier == null) ? 0f : modifier.time;
	}
}
