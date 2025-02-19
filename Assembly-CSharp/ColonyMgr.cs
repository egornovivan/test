using System.Collections.Generic;

public class ColonyMgr
{
	private static Dictionary<int, Dictionary<int, List<ColonyBase>>> ColonyMap = new Dictionary<int, Dictionary<int, List<ColonyBase>>>();

	public static void AddColonyItem(ColonyBase item)
	{
		if (item != null && !(item._Network == null) && item != null && !(item._Network == null))
		{
			if (!ColonyMap.ContainsKey(item._Network.TeamId))
			{
				ColonyMap[item._Network.TeamId] = new Dictionary<int, List<ColonyBase>>();
			}
			if (!ColonyMap[item._Network.TeamId].ContainsKey(item._Network.ExternId))
			{
				ColonyMap[item._Network.TeamId][item._Network.ExternId] = new List<ColonyBase>();
			}
			if (!ColonyMap[item._Network.TeamId][item._Network.ExternId].Contains(item))
			{
				ColonyMap[item._Network.TeamId][item._Network.ExternId].Add(item);
			}
		}
	}

	public static void RemoveColonyItem(ColonyBase item)
	{
		if (item != null && !(item._Network == null) && ColonyMap.ContainsKey(item._Network.TeamId) && ColonyMap[item._Network.TeamId].ContainsKey(item._Network.ExternId))
		{
			ColonyMap[item._Network.TeamId][item._Network.ExternId].Remove(item);
		}
	}

	public static int GetColonyItemAmount(int teamNum, int colonyType)
	{
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(colonyType))
		{
			return 0;
		}
		return ColonyMap[teamNum][colonyType].Count;
	}

	public bool HavePower(int teamNum)
	{
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(1128))
		{
			return false;
		}
		for (int i = 0; i < ColonyMap[teamNum][1128].Count; i++)
		{
			if (((ColonyPPCoal)ColonyMap[teamNum][1128][i]).IsWorking())
			{
				return true;
			}
		}
		return false;
	}

	public static ColonyBase GetColonyItemByObjId(int objId)
	{
		foreach (KeyValuePair<int, Dictionary<int, List<ColonyBase>>> item in ColonyMap)
		{
			foreach (KeyValuePair<int, List<ColonyBase>> item2 in item.Value)
			{
				foreach (ColonyBase item3 in item2.Value)
				{
					if (item3._Network.Id == objId)
					{
						return item3;
					}
				}
			}
		}
		return null;
	}
}
