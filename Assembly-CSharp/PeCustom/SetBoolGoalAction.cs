using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("SET BOOL GOAL", true)]
public class SetBoolGoalAction : Action
{
	private int goalId;

	private string text;

	private int missionId;

	private bool state;

	protected override void OnCreate()
	{
		goalId = Utility.ToInt(base.missionVars, base.parameters["id"]);
		text = Utility.ToText(base.missionVars, base.parameters["text"]);
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		state = Utility.ToBool(base.missionVars, base.parameters["bool"]);
		if (missionId == 0)
		{
			missionId = base.mission.dataId;
		}
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.SetBoolGoal(goalId, text, missionId, state);
		}
		else
		{
			Debug.LogError("MissionMgr is not ready");
		}
		return true;
	}
}
