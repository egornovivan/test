using ScenarioRTL;

namespace PeCustom;

[Statement("PAUSE CONTROLLER", true)]
public class PauseControllerAction : Action
{
	protected override void OnCreate()
	{
	}

	public override bool Logic()
	{
		PeInput.enable = false;
		return true;
	}
}
