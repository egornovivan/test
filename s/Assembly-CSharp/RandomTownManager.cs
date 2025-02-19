using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using RandomTownXML;
using TownData;
using uLink;
using UnityEngine;

public class RandomTownManager
{
	public RandomTownDsc randomTownInfo;

	private Town town;

	private int townTypeNum;

	private Dictionary<int, int> pool;

	private int weightSum;

	private NpcIdNum[] npcIdNum;

	private BuildingNum[] buildingNum;

	private Cell[] cell;

	private Dictionary<IntVector2, TownInfo> townPosInfo;

	private int seed;

	private System.Random myRand;

	public static readonly int minX = -20000;

	public static readonly int minZ = -20000;

	public static readonly int maxX = 20000;

	public static readonly int maxZ = 20000;

	public Dictionary<int, int> mCreatedTownId;

	public Dictionary<int, int> capturedCampId;

	private static RandomTownManager instance;

	public Dictionary<IntVector2, TownInfo> TownPosInfo => townPosInfo;

	public static RandomTownManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new RandomTownManager();
			}
			return instance;
		}
	}

	public RandomTownManager()
	{
		townPosInfo = new Dictionary<IntVector2, TownInfo>();
		pool = new Dictionary<int, int>();
		mCreatedTownId = new Dictionary<int, int>();
		capturedCampId = new Dictionary<int, int>();
		weightSum = 0;
	}

	public void Clear()
	{
		townPosInfo = new Dictionary<IntVector2, TownInfo>();
		pool = new Dictionary<int, int>();
		mCreatedTownId = new Dictionary<int, int>();
		capturedCampId = new Dictionary<int, int>();
		weightSum = 0;
	}

	public void LoadXMLAtPath()
	{
		Clear();
		string path = "RandomTown/RandomTown";
		TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
		StringReader stringReader = new StringReader(textAsset.text);
		if (stringReader == null)
		{
			return;
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(RandomTownDsc));
		randomTownInfo = (RandomTownDsc)xmlSerializer.Deserialize(stringReader);
		stringReader.Close();
		if (LogFilter.logDebug)
		{
			Debug.Log("RandSeed: " + ServerConfig.MapSeed);
		}
		seed = ServerConfig.GenerateMapSeed(ServerConfig.MapSeed);
		townTypeNum = randomTownInfo.town.Count();
		if (LogFilter.logDebug)
		{
			Debug.Log("The amount of town's type: " + townTypeNum);
		}
		for (int i = 0; i < randomTownInfo.town.Count(); i++)
		{
			for (int j = 0; j < randomTownInfo.town[i].weight; j++)
			{
				pool.Add(weightSum + j, i);
			}
			weightSum += randomTownInfo.town[i].weight;
		}
	}

	public void CreateBuildings()
	{
		townPosInfo = new Dictionary<IntVector2, TownInfo>();
		int num = 0;
		myRand = new System.Random(seed);
		for (int i = minX; i < maxX; i += randomTownInfo.distanceX * 2)
		{
			for (int j = minZ; j < maxZ; j += randomTownInfo.distanceZ * 2)
			{
				int num2 = myRand.Next(randomTownInfo.distanceX);
				int num3 = myRand.Next(randomTownInfo.distanceZ);
				IntVector2 intVector = new IntVector2(i + num2, j + num3);
				int aRandomID = GetARandomID(pool, weightSum, myRand);
				town = randomTownInfo.town[aRandomID];
				if (intVector.x > 0 && intVector.x < 4000 && intVector.y > 0 && intVector.y < 4000 && LogFilter.logDebug)
				{
					Debug.Log(string.Concat("TownPosStart: ", intVector, " TownID: ", aRandomID));
				}
				npcIdNum = town.npcIdNum;
				buildingNum = town.buildingNum;
				cell = town.cell;
				TownInfo townInfo = new TownInfo(intVector, town.cellNumX, town.cellNumZ, town.cellSizeX, town.cellSizeZ, aRandomID);
				townInfo.Id = num++;
				townPosInfo.Add(townInfo.PosCenter, townInfo);
				int num4 = cell.Count();
				List<int> list = new List<int>();
				int num5 = 0;
				int num6 = buildingNum.Count();
				for (int k = 0; k < num6; k++)
				{
					for (int l = 0; l < town.buildingNum[k].num; l++)
					{
						list.Add(town.buildingNum[k].bid);
					}
					num5 += town.buildingNum[k].num;
				}
				if (num4 > num5)
				{
					for (int m = 0; m < num4 - num5; m++)
					{
						list.Add(-1);
					}
				}
				RandomTownUtil.Instance.shuffle(list, myRand);
				CreateBuildingInfo(aRandomID, intVector, townInfo.PosCenter, list);
				CreateTownNPC(town, intVector, townInfo.PosCenter, npcIdNum);
			}
		}
	}

	public void CreateBuildingInfo(int townId, IntVector2 townPosStart, IntVector2 townPosCenter, List<int> bid)
	{
		Cell[] array = randomTownInfo.town[townId].cell;
		int num = array.Count();
		int cellSizeX = randomTownInfo.town[townId].cellSizeX;
		int cellSizeZ = randomTownInfo.town[townId].cellSizeZ;
		for (int i = 0; i < num; i++)
		{
			if (bid[i] != -1)
			{
				int x_ = townPosStart.x + array[i].x * cellSizeX;
				int y_ = townPosStart.y + array[i].z * cellSizeZ;
				IntVector2 pos = new IntVector2(x_, y_);
				int rot = array[i].rot;
				int id = bid[i];
				BuildingInfo buildingInfo = new BuildingInfo(pos, id, rot, cellSizeX, cellSizeZ);
				townPosInfo[townPosCenter].BuildingIdList.Add(buildingInfo.buildingNo);
				townPosInfo[townPosCenter].AddBuildingCell(array[i].x, array[i].z);
			}
		}
		townPosInfo[townPosCenter].AddStreetCell();
	}

	private void CreateTownNPC(Town town, IntVector2 townPosStart, IntVector2 townPosCenter, NpcIdNum[] npcIdNum)
	{
		List<IntVector2> streetCellList = townPosInfo[townPosCenter].StreetCellList;
		int cellSizeX = townPosInfo[townPosCenter].CellSizeX;
		int cellSizeZ = townPosInfo[townPosCenter].CellSizeZ;
		for (int i = 0; i < npcIdNum.Count(); i++)
		{
			int nid = npcIdNum[i].nid;
			int num = npcIdNum[i].num;
			for (int j = 0; j < num; j++)
			{
				IntVector2 intVector = streetCellList[myRand.Next(streetCellList.Count)];
				IntVector2 intVector2 = townPosStart + new IntVector2(intVector.x * cellSizeX, intVector.y * cellSizeZ);
				IntVector2 intVector3 = intVector2 + new IntVector2(myRand.Next(cellSizeX), myRand.Next(cellSizeZ));
				if (TownNpcManager.Instance.npcInfoMap.ContainsKey(intVector3))
				{
					j--;
				}
				else if (!NpcManager.HasTownNpc(new Vector2(intVector3.x, intVector3.y)))
				{
					TownNpcManager.Instance.AddNpc(intVector3, nid);
					townPosInfo[townPosCenter].TownNpcPosList.Add(intVector3);
				}
			}
		}
	}

	public int GetARandomID(Dictionary<int, int> pool, int maxNum, System.Random rand)
	{
		int key = rand.Next(maxNum);
		if (!pool.ContainsKey(key))
		{
			return 0;
		}
		return pool[key];
	}

	public void SetCaptured(int townId)
	{
		if (!capturedCampId.ContainsKey(townId))
		{
			capturedCampId.Add(townId, 0);
		}
	}

	public bool IsCaptured(int campId)
	{
		return capturedCampId.ContainsKey(campId);
	}

	public static void SyncRandomTown(uLink.NetworkPlayer peer, BaseNetwork net)
	{
		int[] array = BuildingInfoManager.Instance.GetAllDestroyedTowns().ToArray();
		net.RPCPeer(peer, EPacketType.PT_Common_RandomTownData, array);
	}

	public static void RPC_C2S_TownCreate(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		if (LogFilter.logDebug)
		{
			Debug.Log("Created town: " + value);
		}
		if (!Instance.mCreatedTownId.ContainsKey(value))
		{
			Instance.mCreatedTownId.Add(value, 0);
		}
	}

	public static void RPC_C2S_NativeTowerDestroyed(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		stream.TryRead<int>(out var value);
		if (LogFilter.logDebug)
		{
			Debug.Log("Captured camp: " + value);
		}
		if (!Instance.capturedCampId.ContainsKey(value))
		{
			Instance.capturedCampId.Add(value, 0);
		}
		Player player = Player.GetPlayer(info.sender);
		if (null != player)
		{
			ChannelNetwork.SyncChannel(player.WorldId, EPacketType.PT_Common_NativeTowerDestroyed, value);
		}
	}
}
