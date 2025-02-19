using System.Collections.Generic;

internal class SPTowerDefenceManager
{
	private static Dictionary<int, SPTowerDefence> SPTowerDefenceDic = new Dictionary<int, SPTowerDefence>();

	public static void AddSPTowerDefence(int nMissionID, SPTowerDefence obj)
	{
		if (null != obj && SPTowerDefenceDic.ContainsKey(nMissionID))
		{
			DestroySPTowerDefence(nMissionID);
			SPTowerDefenceDic[nMissionID] = obj;
		}
	}

	public static void DestroySPTowerDefence(int nMissionID)
	{
		if (SPTowerDefenceDic.ContainsKey(nMissionID))
		{
			SPTowerDefenceDic[nMissionID].NetWorkDestroyObject();
		}
	}
}
