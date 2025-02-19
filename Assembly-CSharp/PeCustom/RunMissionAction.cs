using ScenarioRTL;

namespace PeCustom;

[Statement("RUN MISSION", true)]
public class RunMissionAction : Action
{
	private int missionId;

	private OBJECT player;

	private bool owner;

	protected override void OnCreate()
	{
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		player = Utility.ToObject(base.parameters["player"]);
		if (PeCustomScene.Self != null && PeCustomScene.Self.scenario != null)
		{
			int playerId = PeCustomScene.Self.scenario.playerId;
			owner = PeScenarioUtility.OwnerCheck(playerId, playerId, player);
		}
		else
		{
			owner = false;
		}
	}

	public override bool Logic()
	{
		if (owner && missionId > 0 && PeCustomScene.Self != null && PeCustomScene.Self.scenario != null && PeCustomScene.Self.scenario.missionMgr != null)
		{
			PeCustomScene.Self.scenario.missionMgr.RunMission(missionId);
		}
		return true;
	}
}
