using System.Collections.Generic;
using UnityEngine;

public class RandomDunGenMgr : MonoBehaviour
{
	private const int DISTANCE_Y = 200;

	private const int START_Y = -512;

	public const int AREA_RADIUS = 8;

	[HideInInspector]
	public int IdGenerator;

	private static RandomDunGenMgr mInstance;

	private int seed;

	private List<RandomDungeonData> readyToClose = new List<RandomDungeonData>();

	private int frameCount;

	[HideInInspector]
	public int genWorld => 200;

	public static RandomDunGenMgr Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
		frameCount++;
		if (frameCount > 7200)
		{
			CheckDungeonClose(readyToClose);
			readyToClose = FindReadyToClose();
			frameCount = 0;
		}
		if (frameCount % 120 == 0)
		{
			CheckReadyList(readyToClose);
		}
	}

	public bool EnterDungeon(Vector3 entrancePos, Vector3 enterPos, out Vector3 genPos, out int seed, out int dungeonId, out int dungeonDataId)
	{
		RandomDungeonData dungeonData = RandomDunGenMgrData.GetDungeonData(entrancePos);
		if (dungeonData == null)
		{
			genPos = Vector3.zero;
			seed = -1;
			dungeonId = -1;
			dungeonDataId = -1;
			return false;
		}
		seed = dungeonData.seed;
		if (dungeonData.seed == -1)
		{
			if (dungeonData.IsOpen)
			{
				genPos = Vector3.zero;
				seed = -1;
				dungeonId = -1;
				dungeonDataId = -1;
				return false;
			}
			InitDungeonData(dungeonData, enterPos);
		}
		genPos = dungeonData.revivePos;
		dungeonId = dungeonData.id;
		dungeonDataId = dungeonData.dungeonBaseDataId;
		RemoveReadyDungeon(dungeonData);
		return true;
	}

	public void ExitDungeon()
	{
	}

	public void SetSeed(Vector3 entrancePos, int seed)
	{
		RandomDungeonData dungeonData = RandomDunGenMgrData.GetDungeonData(entrancePos);
		if (dungeonData != null)
		{
			dungeonData.seed = seed;
		}
	}

	public void InitDungeonData(RandomDungeonData rdd, Vector3 enterPos)
	{
		for (int i = 0; i < 9999; i++)
		{
			int num = -512 - i * 200;
			bool flag = true;
			foreach (RandomDungeonData value in RandomDunGenMgrData.allDungeons.Values)
			{
				if (value.genDunPos.y < (float)(num + 10) && value.genDunPos.y > (float)(num - 10))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				rdd.SetPosByY(num, enterPos);
				break;
			}
		}
	}

	private void RemoveReadyDungeon(RandomDungeonData rdd)
	{
		readyToClose.Remove(rdd);
	}

	private void CheckReadyList(List<RandomDungeonData> readyList)
	{
		List<RandomDungeonData> removeList = new List<RandomDungeonData>();
		RandomDungeonData rddValue;
		foreach (RandomDungeonData ready in readyList)
		{
			rddValue = ready;
			if (rddValue.IsOpen)
			{
				List<Player> list = ObjNetInterface.Get<Player>();
				if (null != list.Find((Player it) => it.WorldId == genWorld && it.Pos.y < rddValue.revivePos.y + 100f && it.Pos.y > rddValue.revivePos.y - 100f))
				{
					removeList.Add(rddValue);
				}
			}
		}
		readyList.RemoveAll((RandomDungeonData it) => removeList.Contains(it));
	}

	private List<RandomDungeonData> FindReadyToClose()
	{
		List<RandomDungeonData> list = new List<RandomDungeonData>();
		RandomDungeonData[] allDungeons = RandomDunGenMgrData.AllDungeons;
		RandomDungeonData rddValue;
		for (int i = 0; i < allDungeons.Length; i++)
		{
			rddValue = allDungeons[i];
			if ((!rddValue.IsTaskDungeon || Player.CheckRemoveMissionDun()) && rddValue.IsOpen)
			{
				List<Player> list2 = ObjNetInterface.Get<Player>();
				if (null == list2.Find((Player it) => it.WorldId == genWorld && it.Pos.y < rddValue.revivePos.y + 100f && it.Pos.y > rddValue.revivePos.y - 100f))
				{
					list.Add(rddValue);
				}
			}
		}
		return list;
	}

	private void CheckDungeonClose(List<RandomDungeonData> readyCloseList)
	{
		List<RandomDungeonData> removeList = new List<RandomDungeonData>();
		RandomDungeonData rddValue;
		foreach (RandomDungeonData readyClose in readyCloseList)
		{
			rddValue = readyClose;
			if (rddValue.IsOpen)
			{
				List<Player> list = ObjNetInterface.Get<Player>();
				if (null == list.Find((Player it) => it.WorldId == genWorld && it.Pos.y < rddValue.revivePos.y + 100f && it.Pos.y > rddValue.revivePos.y - 100f))
				{
					removeList.Add(rddValue);
				}
			}
		}
		if (removeList.Count <= 0)
		{
			return;
		}
		List<Vector3> list2 = new List<Vector3>();
		foreach (RandomDungeonData item in removeList)
		{
			list2.Add(item.entrancePos);
		}
		ChannelNetwork.SyncPlayerChannel(EPacketType.PT_InGame_RemoveDunEntrance, list2.ToArray());
		RandomDunGenMgrData.CloseDungeon(removeList);
		readyCloseList.RemoveAll((RandomDungeonData it) => removeList.Contains(it));
	}

	private static bool IsAreaAvalable(Vector3 entrancePos)
	{
		IntVector2 intVector = new IntVector2(Mathf.RoundToInt(entrancePos.x) >> 8, Mathf.RoundToInt(entrancePos.z) >> 8);
		for (int i = intVector.x - 1; i < intVector.x + 2; i++)
		{
			for (int j = intVector.y - 1; j < intVector.y + 2; j++)
			{
				if (RandomDunGenMgrData.areaDungeons.ContainsKey(new IntVector2(i, j)))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool GenEntrance(Vector3 entrancePos, DungeonBaseData dbd)
	{
		if (dbd.level < 100 && !IsAreaAvalable(entrancePos))
		{
			return false;
		}
		if (!RandomDunGenMgrData.allDungeons.ContainsKey(entrancePos))
		{
			RandomDungeonData randomDungeonData = new RandomDungeonData();
			randomDungeonData.level = dbd.level;
			randomDungeonData.dungeonBaseDataId = dbd.id;
			randomDungeonData.entrancePos = entrancePos;
			RandomDunGenMgrData.AddDungenData(entrancePos, randomDungeonData);
			return true;
		}
		return false;
	}

	public void ReceiveIsoObject(int dungeonId, ulong isoId, int instanceId)
	{
		Debug.Log("rare item ready!");
		List<RandomItemObj> rareItem = RandomDunGenMgrData.GetRareItem(dungeonId);
		if (rareItem != null)
		{
			if (rareItem.Count == 0)
			{
				RandomDunGenMgrData.RemoveRareItemList(dungeonId);
			}
			rareItem[0].AddRareInstance(instanceId);
			ChannelNetwork.SyncPlayerChannel(EPacketType.PT_InGame_RandomIsoObj, rareItem[0].Pos, instanceId);
			rareItem.RemoveAt(0);
		}
	}
}
