using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.SqliteClient;
using UnityEngine;

public class ColonyMgr : MonoBehaviour
{
	public const int VERSION000 = 2821;

	public const int VERSION001 = 20160914;

	public const int VERSION002 = 2016102100;

	public const int CUR_VERSION = 2016102100;

	public bool InTest;

	public static Dictionary<int, Dictionary<int, List<ColonyBase>>> ColonyMap = new Dictionary<int, Dictionary<int, List<ColonyBase>>>();

	public static ColonyMgr _Instance;

	public static Dictionary<int, List<int>> addedStoreId = new Dictionary<int, List<int>>();

	public static Dictionary<int, int> colonyMoney = new Dictionary<int, int>();

	public static Dictionary<int, List<CSTreatment>> TreatmentMap = new Dictionary<int, List<CSTreatment>>();

	private double lastCycle = -9999.0;

	private int counter;

	private static List<ColonyBase> AllPowerPlant(int teamId)
	{
		List<ColonyBase> list = new List<ColonyBase>();
		if (ColonyMap.ContainsKey(teamId))
		{
			Dictionary<int, List<ColonyBase>> dictionary = ColonyMap[teamId];
			if (dictionary.ContainsKey(1128))
			{
				list.AddRange(dictionary[1128]);
			}
			if (dictionary.ContainsKey(1558))
			{
				list.AddRange(dictionary[1558]);
			}
		}
		return list;
	}

	public static List<ColonyFarm> AllWorkingMachine(int colonyIdInfo, Vector3 pos)
	{
		List<ColonyFarm> list = new List<ColonyFarm>();
		foreach (int key in ColonyMap.Keys)
		{
			if (!ColonyMap[key].ContainsKey(1127) || ColonyMap[key][1127].Count == 0)
			{
				continue;
			}
			ColonyAssembly colonyAssembly = ColonyMap[key][1127][0] as ColonyAssembly;
			if (!colonyAssembly.InRange(pos) || !ColonyMap[key].ContainsKey(colonyIdInfo))
			{
				continue;
			}
			foreach (ColonyBase item in ColonyMap[key][colonyIdInfo])
			{
				if (item is ColonyFarm colonyFarm && colonyFarm.IsWorking())
				{
					list.Add(colonyFarm);
				}
			}
		}
		return list;
	}

	public static void AddStore(List<int> storeIdList, int teamId)
	{
		if (!addedStoreId.ContainsKey(teamId))
		{
			addedStoreId.Add(teamId, new List<int>());
			if (!colonyMoney.ContainsKey(teamId))
			{
				colonyMoney.Add(teamId, 5000);
			}
		}
		List<int> list = addedStoreId[teamId];
		List<int> list2 = new List<int>();
		foreach (int storeId in storeIdList)
		{
			List<int> allIdOfSameItem = ShopRespository.GetAllIdOfSameItem(storeId);
			int limitNum = ShopRespository.GetLimitNum(storeId);
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			foreach (int item in allIdOfSameItem)
			{
				if (item == storeId)
				{
					list3.Add(item);
				}
				else if (ShopRespository.GetLimitNum(item) >= limitNum)
				{
					list3.Add(item);
				}
				else if (ShopRespository.GetLimitNum(item) < limitNum)
				{
					list4.Add(item);
				}
			}
			bool flag = true;
			foreach (int item2 in list3)
			{
				if (list.Contains(item2))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			foreach (int item3 in list4)
			{
				if (list.Contains(item3))
				{
					list.Remove(item3);
					break;
				}
			}
			list.Add(storeId);
			list2.Add(storeId);
		}
		if (list2.Count > 0)
		{
			SyncSave(teamId);
		}
		if (list2.Count > 0 && _Instance != null)
		{
			List<ColonyBase> colonyItemsByItemId = _Instance.GetColonyItemsByItemId(teamId, 1357);
			if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
			{
				ColonyTrade colonyTrade = colonyItemsByItemId[0] as ColonyTrade;
				colonyTrade.UpdateShopList();
			}
		}
	}

	public static void SetColonyMoney(int money, int teamId)
	{
		colonyMoney[teamId] = money;
	}

	public static int GetColonyMoney(int teamId)
	{
		if (colonyMoney.ContainsKey(teamId))
		{
			return colonyMoney[teamId];
		}
		return 0;
	}

	public void RefreshColonyMoney()
	{
		List<int> list = colonyMoney.Keys.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			colonyMoney[list[i]] = 5000;
			if (_Instance != null)
			{
				List<ColonyBase> colonyItemsByItemId = _Instance.GetColonyItemsByItemId(list[i], 1357);
				if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
				{
					ColonyTrade colonyTrade = colonyItemsByItemId[0] as ColonyTrade;
					colonyTrade.RefreshMoney(5000);
				}
			}
			SyncSave(list[i]);
		}
	}

	public static void AddTreatment(int teamId, List<CSTreatment> cstl)
	{
		if (!TreatmentMap.ContainsKey(teamId))
		{
			TreatmentMap.Add(teamId, new List<CSTreatment>());
		}
		TreatmentMap[teamId].AddRange(cstl);
		SyncSave(teamId);
	}

	public static void RemoveNpcTreatment(int teamId, int npcId)
	{
		if (TreatmentMap.ContainsKey(teamId))
		{
			TreatmentMap[teamId].RemoveAll((CSTreatment it) => it.npcId == npcId);
			SyncSave(teamId);
		}
	}

	public static CSTreatment FindTreatment(int teamId, int id, bool needTreat = false)
	{
		if (!TreatmentMap.ContainsKey(teamId))
		{
			return null;
		}
		foreach (CSTreatment item in TreatmentMap[teamId])
		{
			if (item.npcId == id)
			{
				if (!needTreat)
				{
					return item;
				}
				if (item.needTreatTimes > 0)
				{
					return item;
				}
			}
		}
		return null;
	}

	public static void UpdateTreatment(int teamId)
	{
		if (TreatmentMap.ContainsKey(teamId))
		{
			TreatmentMap[teamId].RemoveAll((CSTreatment it) => it.needTreatTimes <= 0);
			if (TreatmentMap[teamId].Count <= 0)
			{
				TreatmentMap.Remove(teamId);
			}
			SyncSave(teamId);
		}
	}

	public static void SyncSave(int teamId)
	{
		ColonyMgrData colonyMgrData = new ColonyMgrData();
		colonyMgrData.ExportData(teamId);
		AsyncSqlite.AddRecord(colonyMgrData);
	}

	public static void CombomData(BinaryWriter writer, int teamId)
	{
		if (!colonyMoney.ContainsKey(teamId))
		{
			colonyMoney[teamId] = 5000;
		}
		BufferHelper.Serialize(writer, colonyMoney[teamId]);
		if (!addedStoreId.ContainsKey(teamId))
		{
			addedStoreId[teamId] = new List<int>();
		}
		List<int> list = addedStoreId[teamId];
		BufferHelper.Serialize(writer, list.Count);
		foreach (int item in list)
		{
			BufferHelper.Serialize(writer, item);
		}
	}

	public static byte[] PackData(int teamId)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(memoryStream);
		CombomData(writer, teamId);
		return memoryStream.ToArray();
	}

	public static void ParseData(byte[] data, int teamId, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader binaryReader = new BinaryReader(input);
		if (ver < 2016102100)
		{
			int num = BufferHelper.ReadInt32(binaryReader);
			List<CSTreatment> list = new List<CSTreatment>();
			for (int i = 0; i < num; i++)
			{
				list.Add(CSTreatment._readTreatmentData(binaryReader, 2016102100));
			}
			TreatmentMap.Add(teamId, list);
			return;
		}
		colonyMoney[teamId] = BufferHelper.ReadInt32(binaryReader);
		int num2 = BufferHelper.ReadInt32(binaryReader);
		List<int> list2 = new List<int>();
		for (int j = 0; j < num2; j++)
		{
			list2.Add(BufferHelper.ReadInt32(binaryReader));
		}
		addedStoreId[teamId] = list2;
	}

	public static void Load()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM colonydata;");
			pEDbOp.BindReaderHandler(LoadComplete);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	public static void LoadComplete(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			int int2 = reader.GetInt32(reader.GetOrdinal("id"));
			byte[] data = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			ParseData(data, int2, @int);
		}
	}

	public static void SyncData(Player player)
	{
		player.RPCOwner(EPacketType.PT_CL_MGR_InitData, PackData(player.TeamId));
	}

	public void Awake()
	{
		_Instance = this;
		StartCoroutine("MyUpdate");
	}

	private void Update()
	{
		counter++;
		if (counter > 240)
		{
			double cycleInDay = GameTime.Timer.CycleInDay;
			if (lastCycle > -2.0 && lastCycle < -0.25 && cycleInDay >= -0.25)
			{
				RefreshColonyMoney();
			}
			lastCycle = GameTime.Timer.CycleInDay;
			counter = 0;
		}
	}

	public IEnumerator MyUpdate()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			foreach (KeyValuePair<int, Dictionary<int, List<ColonyBase>>> item in ColonyMap)
			{
				foreach (KeyValuePair<int, List<ColonyBase>> iter2 in item.Value)
				{
					for (int i = 0; i < iter2.Value.Count; i++)
					{
						int count = iter2.Value.Count;
						if (iter2.Value[i] != null)
						{
							iter2.Value[i].Update();
						}
						if (count < iter2.Value.Count)
						{
							i--;
						}
					}
				}
			}
			GMCommand.Self.MyUpdate();
		}
	}

	public void AddColonyItem(ColonyBase item)
	{
		if (item != null && !(item._Network == null))
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

	public void AddCSTreatment(int teamID, CSTreatment treatment)
	{
		if (!TreatmentMap.ContainsKey(teamID))
		{
			TreatmentMap.Add(teamID, new List<CSTreatment>());
		}
		TreatmentMap[teamID].Add(treatment);
	}

	public void RemoveCSTreatment(int teamID, CSTreatment treatment)
	{
		if (TreatmentMap.ContainsKey(teamID))
		{
			TreatmentMap[teamID].Remove(treatment);
		}
	}

	public void RemoveColonyItem(ColonyBase item)
	{
		if (item != null && !(item._Network == null) && ColonyMap.ContainsKey(item._Network.TeamId) && ColonyMap[item._Network.TeamId].ContainsKey(item._Network.ExternId))
		{
			ColonyMap[item._Network.TeamId][item._Network.ExternId].Remove(item);
		}
	}

	public bool AddNpc(AiAdNpcNetwork npcR, ColonyDwellings dwelling, int teamId)
	{
		ColonyNpc colonyNpc = new ColonyNpc();
		if (dwelling.CanAddNpc(npcR.Id))
		{
			ColonyNpcMgr.Add(teamId, npcR.Id, colonyNpc);
			dwelling.AddNpcs(colonyNpc._npcID);
			colonyNpc.Save();
			npcR.SyncTeamId();
			return true;
		}
		return false;
	}

	public bool CheckMax(int teamNum, int itemId)
	{
		if (itemId == 1127)
		{
			int colonyItemAmount = GetColonyItemAmount(teamNum, itemId);
			if (colonyItemAmount > 0)
			{
				return false;
			}
			return true;
		}
		List<ColonyBase> colonyItemsByItemId = GetColonyItemsByItemId(teamNum, 1127);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count != 1)
		{
			return false;
		}
		ColonyAssembly colonyAssembly = (ColonyAssembly)colonyItemsByItemId[0];
		int colonyItemCountInAssembly = GetColonyItemCountInAssembly(teamNum, itemId, colonyAssembly);
		if (colonyAssembly != null)
		{
			CSAssemblyData cSAssemblyData = (CSAssemblyData)colonyAssembly._RecordData;
			switch (itemId)
			{
			case 1128:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].coalPlantCnt)
				{
					return false;
				}
				break;
			case 1129:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].storageCnt)
				{
					return false;
				}
				break;
			case 1130:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].RepairMachineCnt)
				{
					return false;
				}
				break;
			case 1131:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].dwellingsCnt)
				{
					return false;
				}
				break;
			case 1132:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].EnhanceMachineCnt)
				{
					return false;
				}
				break;
			case 1133:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].RecycleMachineCnt)
				{
					return false;
				}
				break;
			case 1134:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].farmCnt)
				{
					return false;
				}
				break;
			case 1135:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].factoryCnt)
				{
					return false;
				}
				break;
			case 1356:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].processingCnt)
				{
					return false;
				}
				break;
			case 1357:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].tradePostCnt)
				{
					return false;
				}
				break;
			case 1424:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].medicalCheckCnt)
				{
					return false;
				}
				break;
			case 1422:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].medicalTreatCnt)
				{
					return false;
				}
				break;
			case 1421:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].medicalTentCnt)
				{
					return false;
				}
				break;
			case 1423:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].trainCenterCnt)
				{
					return false;
				}
				break;
			case 1558:
				if (colonyItemCountInAssembly >= CSAssemblyInfo.m_Levels[cSAssemblyData.m_Level].fusionPlantCnt)
				{
					return false;
				}
				break;
			default:
				return false;
			}
			return true;
		}
		return false;
	}

	public ColonyAssembly GetColonyAssembly(int teamId)
	{
		if (!ColonyMap.ContainsKey(teamId) || !ColonyMap[teamId].ContainsKey(1127))
		{
			return null;
		}
		return ColonyMap[teamId][1127][0] as ColonyAssembly;
	}

	public int GetColonyItemAmount(int teamNum, int colonyType)
	{
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(colonyType))
		{
			return 0;
		}
		return ColonyMap[teamNum][colonyType].Count;
	}

	public int GetColonyItemCountInAssembly(int teamNum, int colonyType, ColonyAssembly obj)
	{
		Vector3 position = obj._Network.transform.position;
		int level = ((CSAssemblyData)obj._RecordData).m_Level;
		List<ColonyBase> list = new List<ColonyBase>();
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(colonyType))
		{
			return 0;
		}
		foreach (ColonyBase item in ColonyMap[teamNum][colonyType])
		{
			Vector3 position2 = item._Network.transform.position;
			if (Vector3.Distance(position, position2) <= CSAssemblyInfo.m_Levels[level].radius)
			{
				list.Add(item);
			}
		}
		return list.Count;
	}

	public List<ColonyBase> GetColonyItemsByItemId(int teamNum, int colonyType)
	{
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(colonyType))
		{
			return null;
		}
		return ColonyMap[teamNum][colonyType];
	}

	public List<ColonyBase> GetColonyItemsInCore(int teamNum, int colonyType)
	{
		List<ColonyBase> colonyItemsByItemId = GetColonyItemsByItemId(teamNum, colonyType);
		List<ColonyBase> list = new List<ColonyBase>();
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			if (HaveCore(teamNum, item._Network.transform.position))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<ColonyBase> GetColonyItemsIsWorking(int teamNum, int colonyType)
	{
		List<ColonyBase> colonyItemsByItemId = GetColonyItemsByItemId(teamNum, colonyType);
		List<ColonyBase> list = new List<ColonyBase>();
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			if (item.IsWorking())
			{
				list.Add(item);
			}
		}
		return list;
	}

	public ColonyBase GetOneColonyItemIsWorkingPrefered(int teamNum, int colonyType)
	{
		List<ColonyBase> colonyItemsByItemId = GetColonyItemsByItemId(teamNum, colonyType);
		if (colonyItemsByItemId == null)
		{
			return null;
		}
		List<ColonyBase> list = new List<ColonyBase>();
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			if (item.IsWorking())
			{
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			return list[0];
		}
		if (colonyItemsByItemId.Count > 0)
		{
			return colonyItemsByItemId[0];
		}
		return null;
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

	public bool HavePower(int teamNum, Vector3 pos)
	{
		List<ColonyBase> list = AllPowerPlant(teamNum);
		if (list.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ColonyPPCoal colonyPPCoal = (ColonyPPCoal)list[i];
			if (colonyPPCoal != null && !(colonyPPCoal._Network == null))
			{
				Vector3 position = colonyPPCoal._Network.transform.position;
				if (colonyPPCoal.IsWorking() && Vector2.Distance(position, pos) <= colonyPPCoal.PowerRadius)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HaveCore(int teamNum, Vector3 pos)
	{
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(1127))
		{
			return false;
		}
		for (int i = 0; i < ColonyMap[teamNum][1127].Count; i++)
		{
			if ((ColonyAssembly)ColonyMap[teamNum][1127][i] != null && !(((ColonyAssembly)ColonyMap[teamNum][1127][i])._Network == null))
			{
				Vector3 position = ((ColonyAssembly)ColonyMap[teamNum][1127][i])._Network.transform.position;
				ColonyAssembly colonyAssembly = (ColonyAssembly)ColonyMap[teamNum][1127][i];
				int level = ((CSAssemblyData)colonyAssembly._RecordData).m_Level;
				if (Vector3.Distance(position, pos) <= CSAssemblyInfo.m_Levels[level].radius)
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector3 GetAssemblyPos(int teamNum)
	{
		if (!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(1127))
		{
			return Vector3.zero;
		}
		for (int i = 0; i < ColonyMap[teamNum][1127].Count; i++)
		{
			if ((ColonyAssembly)ColonyMap[teamNum][1127][i] != null && !(((ColonyAssembly)ColonyMap[teamNum][1127][i])._Network == null))
			{
				return ((ColonyAssembly)ColonyMap[teamNum][1127][i])._Network.transform.position;
			}
		}
		return Vector3.zero;
	}

	public bool HasCoreAndPower(int teamNum, Vector3 pos)
	{
		if (HaveCore(teamNum, pos) && HavePower(teamNum, pos))
		{
			return true;
		}
		return false;
	}

	public bool CanAddNpc(int teamNum)
	{
		if (ColonyMap.ContainsKey(teamNum) && ColonyMap[teamNum].ContainsKey(1127) && ColonyMap[teamNum][1127].Count > 0)
		{
			return true;
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

	public static ColonyBase GetAvailableItemByObjId(int objId)
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

	public static ColonyBase FindDwellingWithEmptyBed()
	{
		return null;
	}

	public static ColonyDwellings FindNewBed(int teamId, int oldBedId)
	{
		List<ColonyBase> bedIsWorking = _Instance.GetColonyItemsIsWorking(teamId, 1131);
		if (bedIsWorking.Count > 0)
		{
			foreach (ColonyBase item in bedIsWorking)
			{
				ColonyDwellings colonyDwellings = item as ColonyDwellings;
				if (colonyDwellings.Id != oldBedId && colonyDwellings.HaveEmpty())
				{
					return colonyDwellings;
				}
			}
		}
		List<ColonyBase> bedInCore = _Instance.GetColonyItemsInCore(teamId, 1131);
		bedInCore.RemoveAll((ColonyBase it) => bedIsWorking.Contains(it));
		if (bedInCore.Count > 0)
		{
			foreach (ColonyBase item2 in bedIsWorking)
			{
				ColonyDwellings colonyDwellings2 = item2 as ColonyDwellings;
				if (colonyDwellings2.Id != oldBedId && colonyDwellings2.HaveEmpty())
				{
					return colonyDwellings2;
				}
			}
		}
		List<ColonyBase> colonyItemsByItemId = _Instance.GetColonyItemsByItemId(teamId, 1131);
		colonyItemsByItemId.RemoveAll((ColonyBase it) => bedIsWorking.Contains(it));
		colonyItemsByItemId.RemoveAll((ColonyBase it) => bedInCore.Contains(it));
		if (colonyItemsByItemId.Count > 0)
		{
			foreach (ColonyBase item3 in colonyItemsByItemId)
			{
				ColonyDwellings colonyDwellings3 = item3 as ColonyDwellings;
				if (colonyDwellings3.Id != oldBedId && colonyDwellings3.HaveEmpty())
				{
					return colonyDwellings3;
				}
			}
		}
		return null;
	}

	public static CSInfo GetInfo(int protoId)
	{
		switch (protoId)
		{
		case 1127:
			return CSAssemblyInfo._Self;
		case 1128:
			return CSPPCoalInfo.ppCoalInfo;
		case 1129:
			return CSStorageInfo._Self;
		case 1130:
			return CSRepairInfo._Self;
		case 1131:
			return CSDwellingsInfo._Self;
		case 1132:
			return CSEnhanceInfo._Self;
		case 1133:
			return CSRecycleInfo._Self;
		case 1134:
			return CSFarmInfo._Self;
		case 1135:
			return CSFactoryInfo._Self;
		case 1356:
			return CSProcessingInfo._Self;
		case 1357:
			return CSTradeInfo._Self;
		case 1423:
			return CSTrainingInfo._Self;
		case 1424:
			return CSCheckInfo._Self;
		case 1422:
			return CSTreatInfo._Self;
		case 1421:
			return CSTentInfo._Self;
		case 1558:
			return CSPPCoalInfo.ppFusionInfo;
		default:
			if (LogFilter.logDebug)
			{
				Debug.LogError("ColonySystem itemid is wrong protoId = " + protoId);
			}
			return null;
		}
	}

	public static void LoadInfo()
	{
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("colonyinfo");
		while (sqliteDataReader.Read())
		{
			int num = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("id")));
			PublicAttr(GetInfo(num), sqliteDataReader);
			string @string = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_property"));
			if (@string != "0")
			{
				SplitInfo(@string, num);
			}
		}
	}

	private static void PublicAttr(CSInfo info, SqliteDataReader reader)
	{
		info.m_Durability = Convert.ToSingle(reader.GetString(reader.GetOrdinal("durability")));
		info.m_RepairTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("repairtime")));
		info.m_DeleteTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("deletetime")));
		info.m_Power = Convert.ToSingle(reader.GetString(reader.GetOrdinal("power")));
		info.m_WorkersCnt = Convert.ToInt32(reader.GetString(reader.GetOrdinal("workerscnt")));
	}

	private static void SplitInfo(string str, int id)
	{
		if (str.Length == 0)
		{
			return;
		}
		switch (id)
		{
		case 1127:
			LoadAssemblyInfo(str);
			return;
		case 1128:
			LoadPPCoalInfo(str);
			return;
		case 1129:
			LoadStorageInfo(str);
			return;
		case 1130:
			LoadRepairInfo(str);
			return;
		case 1131:
			LoadDwellingsInfo(str);
			return;
		case 1132:
			LoadEnhanceInfo(str);
			return;
		case 1133:
			LoadRecycleInfo(str);
			return;
		case 1134:
			LoadFarmInfo(str);
			return;
		case 1135:
			LoadFactoryInfo(str);
			return;
		case 1356:
			LoadProcessingInfo(str);
			return;
		case 1357:
			LoadTradeInfo(str);
			return;
		case 1423:
			LoadTrainInfo(str);
			return;
		case 1424:
			LoadCheckInfo(str);
			return;
		case 1422:
			LoadTreatInfo(str);
			return;
		case 1421:
			LoadTentInfo(str);
			return;
		case 1558:
			LoadFusionInfo(str);
			return;
		}
		if (LogFilter.logDebug)
		{
			Debug.LogError("ColonyMgr itemid is wrong id = " + id);
		}
	}

	private static void LoadAssemblyInfo(string str)
	{
		string[] array = str.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			CSAssemblyInfo.LevelData levelData = new CSAssemblyInfo.LevelData();
			string[] array2 = array[i].Split(',');
			if (array2.Length < 19)
			{
				if (LogFilter.logDebug)
				{
					Debug.LogError("LoadAssemblyInfo load error");
				}
				break;
			}
			levelData.radius = Convert.ToSingle(array2[0]);
			levelData.dwellingsCnt = Convert.ToInt32(array2[1]);
			levelData.storageCnt = Convert.ToInt32(array2[2]);
			levelData.farmCnt = Convert.ToInt32(array2[3]);
			levelData.factoryCnt = Convert.ToInt32(array2[4]);
			levelData.EngineeringCnt = Convert.ToInt32(array2[5]);
			levelData.RepairMachineCnt = Convert.ToInt32(array2[6]);
			levelData.EnhanceMachineCnt = Convert.ToInt32(array2[7]);
			levelData.RecycleMachineCnt = Convert.ToInt32(array2[8]);
			levelData.coalPlantCnt = Convert.ToInt32(array2[9]);
			levelData.processingCnt = Convert.ToInt32(array2[10]);
			levelData.tradePostCnt = Convert.ToInt32(array2[11]);
			levelData.trainCenterCnt = Convert.ToInt32(array2[12]);
			levelData.medicalCheckCnt = Convert.ToInt32(array2[13]);
			levelData.medicalTreatCnt = Convert.ToInt32(array2[14]);
			levelData.medicalTentCnt = Convert.ToInt32(array2[15]);
			levelData.fusionPlantCnt = Convert.ToInt32(array2[16]);
			string[] array3 = array2[17].Split('|');
			for (int j = 0; j < array3.Length; j++)
			{
				levelData.itemIDList.Add(Convert.ToInt32(array3[j]));
			}
			string[] array4 = array2[18].Split('|');
			for (int k = 0; k < array4.Length; k++)
			{
				levelData.itemCnt.Add(Convert.ToInt32(array4[k]));
			}
			levelData.upgradeTime = Convert.ToSingle(array2[19]);
			CSAssemblyInfo.m_Levels.Add(levelData);
		}
	}

	private static void LoadDwellingsInfo(string str)
	{
	}

	private static void LoadEnhanceInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("LoadEnhanceInfo load error");
			}
		}
		else
		{
			CSEnhanceInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadFarmInfo(string str)
	{
	}

	private static void LoadPowerPlantInfo(CSPPCoalInfo csppcInfo, string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 5)
		{
			Debug.LogError("LoadPowerPlantInfo load error");
			return;
		}
		csppcInfo.m_Radius = Convert.ToSingle(array[0]);
		csppcInfo.m_WorkedTime = Convert.ToSingle(array[1]);
		csppcInfo.m_ChargingRate = Convert.ToSingle(array[2]);
		string[] array2 = array[3].Split(',');
		for (int i = 0; i < array2.Length; i++)
		{
			csppcInfo.m_WorkedTimeItemID.Add(Convert.ToInt32(array2[i]));
		}
		string[] array3 = array[4].Split(',');
		for (int j = 0; j < array3.Length; j++)
		{
			csppcInfo.m_WorkedTimeItemCnt.Add(Convert.ToInt32(array3[j]));
		}
	}

	private static void LoadRecycleInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("LoadRecycleInfo load error");
			}
		}
		else
		{
			CSRecycleInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadRepairInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("LoadRepairInfo load error");
			}
		}
		else
		{
			CSRepairInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadStorageInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 3)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("LoadStorageInfo load error");
			}
		}
		else
		{
			CSStorageInfo.m_MaxItem = Convert.ToInt32(array[0]);
			CSStorageInfo.m_MaxEquip = Convert.ToInt32(array[1]);
			CSStorageInfo.m_MaxRecource = Convert.ToInt32(array[2]);
			CSStorageInfo.m_MaxArmor = Convert.ToInt32(array[3]);
		}
	}

	private static void LoadPPCoalInfo(string str)
	{
		LoadPowerPlantInfo(CSPPCoalInfo.ppCoalInfo, str);
	}

	private static void LoadFusionInfo(string str)
	{
		LoadPowerPlantInfo(CSPPCoalInfo.ppFusionInfo, str);
	}

	private static void LoadFactoryInfo(string str)
	{
	}

	private static void LoadProcessingInfo(string str)
	{
	}

	private static void LoadTradeInfo(string str)
	{
	}

	private static void LoadTrainInfo(string str)
	{
		string[] array = str.Split(';');
		if (array.Length < 1)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("LoadTrainInfo load error");
			}
		}
		else
		{
			CSTrainingInfo.m_BaseTime = Convert.ToSingle(array[0]);
		}
	}

	private static void LoadCheckInfo(string str)
	{
	}

	private static void LoadTreatInfo(string str)
	{
	}

	private static void LoadTentInfo(string str)
	{
	}

	public static ColonyDwellings GetOneBedSpace(int teamNum)
	{
		List<ColonyBase> colonyItemsByItemId = _Instance.GetColonyItemsByItemId(teamNum, 1131);
		foreach (ColonyBase item in colonyItemsByItemId)
		{
			if (item != null)
			{
				ColonyDwellings colonyDwellings = item as ColonyDwellings;
				if (colonyDwellings.HaveEmpty() && colonyDwellings.IsWorking())
				{
					return (ColonyDwellings)item;
				}
			}
		}
		return null;
	}
}
