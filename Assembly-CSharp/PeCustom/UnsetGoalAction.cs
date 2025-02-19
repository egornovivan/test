using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("UNSET GOAL", true)]
public class UnsetGoalAction : Action
{
	private int goalId;

	private int missionId;

	protected override void OnCreate()
	{
		goalId = Utility.ToInt(base.missionVars, base.parameters["id"]);
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		if (missionId == 0)
		{
			missionId = base.mission.dataId;
		}
	}

	public override bool Logic()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.UnsetGoal(goalId, missionId);
		}
		else
		{
			Debug.LogError("MissionMgr is not ready");
		}
		return true;
	}
}
