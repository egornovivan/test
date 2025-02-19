using ScenarioRTL;

namespace PeCustom;

[Statement("END MISSION", true)]
public class EndMissionAction : Action
{
	private int missionId;

	private OBJECT player;

	private EMissionResult result;

	private bool owner;

	protected override void OnCreate()
	{
		missionId = Utility.ToEnumInt(base.parameters["mission"]);
		player = Utility.ToObject(base.parameters["player"]);
		result = (EMissionResult)Utility.ToEnumInt(base.parameters["result"]);
		if (missionId == 0)
		{
			missionId = ((base.mission == null) ? (-1) : base.mission.dataId);
		}
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
			if (result == EMissionResult.Any)
			{
				result = EMissionResult.Accomplished;
			}
			PeCustomScene.Self.scenario.missionMgr.CloseMission(missionId, result);
		}
		return true;
	}
}
