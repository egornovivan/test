using ScenarioRTL;

namespace PeCustom;

[Statement("RESUME CONTROLLER", true)]
public class ResumeControllerAction : Action
{
	protected override void OnCreate()
	{
	}

	public override bool Logic()
	{
		PeInput.enable = true;
		return true;
	}
}
