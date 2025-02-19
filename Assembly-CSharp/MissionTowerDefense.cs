using Pathea;

public class MissionTowerDefense
{
	public static PlayerMission playerMission => MissionManager.Instance.m_PlayerMission;

	public static void ProcessMission(int missionID, int targetID)
	{
		if (!PeGameMgr.IsMulti)
		{
			SceneEntityCreator.self.AddMissionPoint(missionID, targetID);
		}
		else
		{
			NetworkManager.WaitCoroutine(PlayerNetwork.RequestTowerDefense(missionID, targetID));
		}
	}
}
