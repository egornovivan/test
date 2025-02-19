using Pathea;
using ScenarioRTL;

namespace PeCustom;

[Statement("ADD CHOICE", true)]
public class AddChoiceAction : Action
{
	private int id;

	private string text;

	protected override void OnCreate()
	{
		id = Utility.ToInt(base.missionVars, base.parameters["id"]);
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
	}

	public override bool Logic()
	{
		if (PeGameMgr.IsMulti)
		{
			PlayerNetwork.RequestServer(EPacketType.PT_Custom_AddChoice, id, text);
		}
		else if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null)
		{
			PeCustomScene.Self.scenario.dialogMgr.AddChoose(id, text);
		}
		return true;
	}
}
