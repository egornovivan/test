using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SET KILL GOAL", true)]
public class SetKillGoalAction : Action
{
	private int goalId;

	private string text;

	private int missionId;

	private OBJECT monster;

	private ECompare comp;

	private int amt;

	protected override void OnCreate()
	{
		goalId = Utility.ToInt(base.missionVars, base.parameters["id"]);
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		monster = Utility.ToObject(base.parameters["monster"]);
		comp = Utility.ToCompare(base.parameters["compare"]);
		amt = Utility.ToInt(base.missionVars, base.parameters["amount"]);
		if (missionId == 0)
		{
			missionId = base.mission.dataId;
		}
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.SetKillGoal(goalId, text, missionId, monster, comp, amt);
		}
		else
		{
			Debug.LogError("MissionMgr is not ready");
		}
		return true;
	}
}
