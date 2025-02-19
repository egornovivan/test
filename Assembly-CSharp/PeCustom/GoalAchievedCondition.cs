using ScenarioRTL;
using UnityEngine;

namespace PeCustom;

[Statement("GOAL ACHIEVED")]
public class GoalAchievedCondition : Condition
{
	private int goalId;

	private int missionId;

	private bool state;

	protected override void OnCreate()
	{
		goalId = Utility.ToInt(base.missionVars, base.parameters["id"]);
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		state = Utility.ToBool(base.missionVars, base.parameters["bool"]);
		if (missionId == 0)
		{
			missionId = base.mission.dataId;
		}
	}

	public override bool? Check()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			bool? flag = PeCustomScene.Self.scenario.missionMgr.GoalAchieved(goalId, missionId);
			if (!flag.HasValue)
			{
				Debug.LogError("Goal is not set");
			}
			return (flag.HasValue && flag.Value) == state;
		}
		Debug.LogError("MissionMgr is not ready");
		return false;
	}
}
