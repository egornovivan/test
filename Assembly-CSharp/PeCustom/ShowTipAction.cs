using ScenarioRTL;

namespace PeCustom;

[Statement("SHOW TIP", true)]
public class ShowTipAction : Action
{
	private string text;

	protected override void OnCreate()
	{
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
	}

	public override bool Logic()
	{
		new PeTipMsg(text, PeTipMsg.EMsgLevel.Norm);
		return true;
	}
}
