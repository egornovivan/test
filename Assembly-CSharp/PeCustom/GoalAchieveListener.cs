using ScenarioRTL;

namespace PeCustom;

[Statement("GOAL ACHIEVE")]
public class GoalAchieveListener : EventListener
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

	public override void Listen()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve += OnResponse;
		}
	}

	public override void Close()
	{
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.OnGoalAchieve -= OnResponse;
		}
	}

	private void OnResponse(int id, int mid)
	{
		if (id == goalId && mid == missionId)
		{
			Post();
		}
	}
}
