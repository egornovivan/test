using Pathea;

public class GRVcomputerHit : PEHitDetector
{
	protected override void OnHit()
	{
		if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(10046))
		{
			MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(10046);
			if (missionCommonData != null)
			{
				PeEntity npc = PeSingleton<EntityMgr>.Instance.Get(missionCommonData.m_iNpc);
				MissionManager.Instance.SetGetTakeMission(10046, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
			}
		}
	}
}
