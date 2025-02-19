namespace CameraForge;

[Menu("Input/Time/ControllerTime", 1)]
public class ControllerTime : InputNode
{
	public override Slot[] slots => new Slot[0];

	public override Var Calculate()
	{
		if (modifier != null && modifier.controller != null)
		{
			return modifier.controller.time;
		}
		return 0f;
	}
}
