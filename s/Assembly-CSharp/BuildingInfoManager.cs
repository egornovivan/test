using System.Collections.Generic;
using TownData;
using uLink;
using UnityEngine;

public class BuildingInfoManager
{
	private static BuildingInfoManager mInstance;

	public Dictionary<BuildingID, BuildingInfo> mBuildingInfoMap;

	public Dictionary<BuildingID, int> GeneratedBuildings;

	public Dictionary<BuildingID, int> mCreatedNativeTower;

	protected List<TownDoodad> mTownDoodads;

	public Dictionary<int, List<int>> mAliveBuildings;

	public Dictionary<int, Vector3> allTownPos;

	public static BuildingInfoManager Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new BuildingInfoManager();
			}
			return mInstance;
		}
	}

	private BuildingInfoManager()
	{
		mBuildingInfoMap = new Dictionary<BuildingID, BuildingInfo>();
		GeneratedBuildings = new Dictionary<BuildingID, int>();
		mCreatedNativeTower = new Dictionary<BuildingID, int>();
		mTownDoodads = new List<TownDoodad>();
		mAliveBuildings = new Dictionary<int, List<int>>();
		allTownPos = new Dictionary<int, Vector3>();
	}

	public void Clear()
	{
		mBuildingInfoMap.Clear();
		GeneratedBuildings.Clear();
		mCreatedNativeTower.Clear();
	}

	public BuildingInfo GetBuildingInfoByNo(BuildingID No)
	{
		if (!mBuildingInfoMap.ContainsKey(No))
		{
			return null;
		}
		return mBuildingInfoMap[No];
	}

	public bool GeneratetdBuilding(BuildingID bId)
	{
		if (!GeneratedBuildings.ContainsKey(bId))
		{
			return false;
		}
		return true;
	}

	public void AddBuilding(BuildingID bId)
	{
		if (!GeneratedBuildings.ContainsKey(bId))
		{
			GeneratedBuildings.Add(bId, 0);
		}
	}

	public static bool AddTownDoodad(TownDoodad doodad)
	{
		if (!Instance.mTownDoodads.Contains(doodad))
		{
			Instance.mTownDoodads.Add(doodad);
			return true;
		}
		return false;
	}

	public void AddAliveBuilding(int townId, int entityId)
	{
		if (!mAliveBuildings.ContainsKey(townId))
		{
			mAliveBuildings.Add(townId, new List<int>());
		}
		mAliveBuildings[townId].Add(entityId);
	}

	public void OnTownDestroyed(int townId, int worldId)
	{
		if (LogFilter.logDebug)
		{
			Debug.Log("OnTownDestroyed id:" + townId);
		}
		ChannelNetwork.SyncChannel(worldId, EPacketType.PT_Common_TownDestroyed, townId);
	}

	public void OnBuildingDeath(int townId, int entityId, int worldId)
	{
		if (LogFilter.logDebug)
		{
			Debug.Log("OnBuildingDeath id:" + townId + " ," + entityId);
		}
		if (mAliveBuildings.ContainsKey(townId))
		{
			List<int> list = mAliveBuildings[townId];
			list.Remove(entityId);
			if (list.Count == 0)
			{
				OnTownDestroyed(townId, worldId);
			}
			ServerConfig.SyncSave();
		}
	}

	public List<int> GetAllDestroyedTowns()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, List<int>> mAliveBuilding in mAliveBuildings)
		{
			if (mAliveBuilding.Value.Count == 0)
			{
				list.Add(mAliveBuilding.Key);
			}
		}
		return list;
	}

	public void InitAllTownPos(int[] idList, Vector3[] posList)
	{
		if (allTownPos.Count == 0)
		{
			for (int i = 0; i < idList.Length; i++)
			{
				allTownPos.Add(idList[i], posList[i]);
			}
			if (LogFilter.logDebug)
			{
				Debug.Log("allTownPos Init!");
			}
		}
	}

	public Vector3 GetTownPos(int townId)
	{
		if (!allTownPos.ContainsKey(townId))
		{
			return Vector3.zero;
		}
		return allTownPos[townId];
	}

	public static void RPC_C2S_InitTownPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] idList = stream.Read<int[]>(new object[0]);
		Vector3[] posList = stream.Read<Vector3[]>(new object[0]);
		Instance.InitAllTownPos(idList, posList);
	}
}
