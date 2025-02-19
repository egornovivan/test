using System.Collections.Generic;
using System.IO;
using ItemAsset;
using UnityEngine;

public class ColonyFarm : ColonyBase
{
	public const int TOOL_INDEX_WATER = 0;

	public const int TOOL_INDEX_INSECTICIDE = 1;

	public const int CYCLE_MIN_WATER = 100;

	public const int CYCLE_MIN_INSECTICIDE = 100;

	public const int CYCLE_ADD_WATER = 88;

	public const int CYCLE_ADD_INSECTICIDE = 88;

	public const int PLANTS_SEEDS_COUNT = 12;

	public const int PLANTS_TOOLS_COUNT = 2;

	public const int MAX_WORKER_COUNT = 8;

	private CSFarmData _MyData;

	private int m_PlantSequence;

	public float curFarmerGrowRate;

	public override int MaxWorkerCount => 8;

	public ColonyFarm(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSFarmData();
		_MyData = (CSFarmData)_RecordData;
		LoadData();
	}

	public void UpdateFarmerGrowRate()
	{
		curFarmerGrowRate = GetWorkerParam();
	}

	public override void MyUpdate()
	{
		UpdateFarmerGrowRate();
	}

	public float GetWorkerParam()
	{
		float num = 0f;
		foreach (ColonyNpc value in _worker.Values)
		{
			if (value != null)
			{
				num += value.GetFarmingSkill;
			}
		}
		return num;
	}

	public override void InitNpc()
	{
		List<ColonyNpc> teamNpcs = ColonyNpcMgr.GetTeamNpcs(base.TeamId);
		if (teamNpcs == null || teamNpcs.Count <= 0)
		{
			return;
		}
		foreach (ColonyNpc item in teamNpcs)
		{
			if (item.m_Occupation == 3 && AddWorker(item))
			{
				item.m_WorkRoomID = base.Id;
				item.Save();
			}
		}
		SyncSave();
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_PlantSeeds.Count);
		foreach (KeyValuePair<int, int> plantSeed in _MyData.m_PlantSeeds)
		{
			BufferHelper.Serialize(writer, plantSeed.Key);
			BufferHelper.Serialize(writer, plantSeed.Value);
		}
		BufferHelper.Serialize(writer, _MyData.m_Tools.Count);
		foreach (KeyValuePair<int, int> tool in _MyData.m_Tools)
		{
			BufferHelper.Serialize(writer, tool.Key);
			BufferHelper.Serialize(writer, tool.Value);
		}
		BufferHelper.Serialize(writer, _MyData.m_AutoPlanting);
		BufferHelper.Serialize(writer, _MyData.m_SequentialPlanting);
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			int key = BufferHelper.ReadInt32(reader);
			int value = BufferHelper.ReadInt32(reader);
			_MyData.m_PlantSeeds[key] = value;
		}
		num = BufferHelper.ReadInt32(reader);
		for (int j = 0; j < num; j++)
		{
			int key2 = BufferHelper.ReadInt32(reader);
			int value2 = BufferHelper.ReadInt32(reader);
			_MyData.m_Tools[key2] = value2;
		}
		_MyData.m_AutoPlanting = BufferHelper.ReadBoolean(reader);
		_MyData.m_SequentialPlanting = BufferHelper.ReadBoolean(reader);
	}

	public override void InitMyData()
	{
		_MyData.m_AutoPlanting = false;
		_MyData.m_SequentialPlanting = false;
	}

	public bool SetPlantSeed(Player player, int index, ItemObject item)
	{
		if (index < 0 || index >= 12)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Index is out of range!");
			}
			return false;
		}
		if (item != null)
		{
			_MyData.m_PlantSeeds[index] = item.instanceId;
			player.Package.RemoveItem(item);
			player.SyncPackageIndex();
		}
		else
		{
			_MyData.m_PlantSeeds.Remove(index);
		}
		SyncSave();
		return true;
	}

	public ItemObject GetPlantSeed(int index)
	{
		if (index < 0 || index >= 12)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Index is out of range!");
			}
			return null;
		}
		if (_MyData.m_PlantSeeds.ContainsKey(index))
		{
			return ItemManager.GetItemByID(_MyData.m_PlantSeeds[index]);
		}
		return null;
	}

	public bool SetPlantTool(Player player, int index, ItemObject item)
	{
		if (index < 0 || index >= 2)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Index is out of range");
			}
			return false;
		}
		if (item != null)
		{
			_MyData.m_Tools[index] = item.instanceId;
			player.Package.RemoveItem(item);
			player.SyncPackageIndex();
		}
		else
		{
			_MyData.m_Tools.Remove(index);
		}
		SyncSave();
		return true;
	}

	public bool SetPlantTool(int index, ItemObject item)
	{
		if (index < 0 || index >= 2)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Index is out of range");
			}
			return false;
		}
		if (item != null)
		{
			_MyData.m_Tools[index] = item.instanceId;
		}
		SyncSave();
		return true;
	}

	public ItemObject GetPlantTool(int index)
	{
		if (index < 0 || index >= 2)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Index is out of range!");
			}
			return null;
		}
		if (_MyData.m_Tools.ContainsKey(index))
		{
			return ItemManager.GetItemByID(_MyData.m_Tools[index]);
		}
		return null;
	}

	public bool DeletePlantTool(int itemObjID, bool bSync = true)
	{
		foreach (KeyValuePair<int, int> tool in _MyData.m_Tools)
		{
			if (tool.Value == itemObjID)
			{
				_MyData.m_Tools.Remove(tool.Key);
				SyncSave();
				if (bSync)
				{
					_Network.RPCOthers(EPacketType.PT_CL_FARM_DeletePlantTool, itemObjID);
				}
				return true;
			}
		}
		return false;
	}

	public bool HasPlantSeed()
	{
		ItemObject itemObject = null;
		for (int i = 0; i < 12; i++)
		{
			ItemObject plantSeed = GetPlantSeed(i);
			if (plantSeed != null)
			{
				itemObject = plantSeed;
				break;
			}
		}
		return itemObject != null;
	}

	public void SetSequentialActive(bool bActive)
	{
		_MyData.m_SequentialPlanting = bActive;
		SyncSave();
	}

	public void SetAutoPlanting(bool bAuto)
	{
		_MyData.m_AutoPlanting = bAuto;
		SyncSave();
	}

	public bool HaveItem(int itemObjID)
	{
		foreach (KeyValuePair<int, int> plantSeed in _MyData.m_PlantSeeds)
		{
			if (plantSeed.Value == itemObjID)
			{
				return true;
			}
		}
		return false;
	}

	public bool DeleteSeed(int itemObjID, bool bSync = true)
	{
		foreach (KeyValuePair<int, int> plantSeed in _MyData.m_PlantSeeds)
		{
			if (plantSeed.Value == itemObjID)
			{
				_MyData.m_PlantSeeds.Remove(plantSeed.Key);
				SyncSave();
				if (bSync)
				{
					_Network.RPCOthers(EPacketType.PT_CL_FARM_DeleteSeed, itemObjID);
				}
				return true;
			}
		}
		return false;
	}

	public int GetWaterCount(out int toolId)
	{
		foreach (KeyValuePair<int, int> tool in _MyData.m_Tools)
		{
			ItemObject itemByID = ItemManager.GetItemByID(tool.Value);
			if (itemByID != null && itemByID.protoId == 1003)
			{
				toolId = itemByID.instanceId;
				return itemByID.stackCount;
			}
		}
		toolId = -1;
		return 0;
	}

	public int GetCleanCount(out int toolId)
	{
		foreach (KeyValuePair<int, int> tool in _MyData.m_Tools)
		{
			ItemObject itemByID = ItemManager.GetItemByID(tool.Value);
			if (itemByID != null && itemByID.protoId == 1002)
			{
				toolId = itemByID.instanceId;
				return itemByID.stackCount;
			}
		}
		toolId = -1;
		return 0;
	}

	internal IEnumerable<ItemObject> DeleteItemWithItemObjID(int itemObjID, int num, int index = -1)
	{
		List<ItemObject> list = new List<ItemObject>();
		ItemObject itemByID = ItemManager.GetItemByID(itemObjID);
		if (itemByID == null)
		{
			return list;
		}
		if (index != -1)
		{
			if (itemByID.CountDown(num))
			{
				list.Add(itemByID);
			}
			else
			{
				ItemManager.RemoveItem(itemByID.instanceId);
			}
		}
		ChannelNetwork.SyncItemList(_Network.WorldId, list.ToArray());
		return list;
	}

	public bool FetchSeedItem(Player player, int index)
	{
		ItemObject plantSeed = GetPlantSeed(index);
		if (plantSeed != null && player != null && player.Package.AddItem(plantSeed) != -1)
		{
			player.SyncPackageIndex();
			return DeleteSeed(plantSeed.instanceId, bSync: false);
		}
		return false;
	}

	public bool FetchPlantTool(Player player, int index)
	{
		ItemObject plantTool = GetPlantTool(index);
		if (plantTool != null && player != null && player.Package.AddItem(plantTool) != -1)
		{
			player.SyncPackageIndex();
			return DeletePlantTool(plantTool.instanceId, bSync: false);
		}
		return false;
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HaveCore(_Network.TeamId, _Network.transform.position) && ColonyMgr._Instance.HavePower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public int NpcGetPlantSeedId()
	{
		if (_MyData.m_SequentialPlanting)
		{
			for (int i = m_PlantSequence; i < 12; i++)
			{
				if (_MyData.m_PlantSeeds.ContainsKey(i))
				{
					m_PlantSequence = i;
					if (m_PlantSequence == 11)
					{
						m_PlantSequence = 0;
					}
					else
					{
						m_PlantSequence++;
					}
					return _MyData.m_PlantSeeds[i];
				}
			}
			for (int j = 0; j < m_PlantSequence; j++)
			{
				if (_MyData.m_PlantSeeds.ContainsKey(j))
				{
					m_PlantSequence = j;
					if (m_PlantSequence == 11)
					{
						m_PlantSequence = 0;
					}
					else
					{
						m_PlantSequence++;
					}
					return _MyData.m_PlantSeeds[j];
				}
			}
		}
		else
		{
			for (int k = 0; k < 12; k++)
			{
				if (_MyData.m_PlantSeeds.ContainsKey(k))
				{
					return _MyData.m_PlantSeeds[k];
				}
			}
		}
		return -1;
	}

	public override List<ItemIdCount> GetRequirements()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		if (GetPlantTool(0) == null || GetPlantTool(0).stackCount < 100)
		{
			list.Add(new ItemIdCount(1003, 88));
		}
		if (GetPlantTool(1) == null || GetPlantTool(1).stackCount < 100)
		{
			list.Add(new ItemIdCount(1002, 88));
		}
		return list;
	}

	public override bool MeetDemand(int protoId, int count)
	{
		if (protoId == 1003)
		{
			ItemObject plantTool = GetPlantTool(0);
			if (plantTool != null)
			{
				plantTool.CountUp(count);
				ChannelNetwork.SyncItem(_Network.WorldId, plantTool);
			}
			else
			{
				ItemObject itemObject = ItemManager.CreateItem(protoId, count);
				ChannelNetwork.SyncItem(_Network.WorldId, itemObject);
				if (SetPlantTool(0, itemObject))
				{
					_Network.RPCOthers(EPacketType.PT_CL_FARM_SetPlantTool, 0, itemObject.instanceId, true);
				}
			}
		}
		if (protoId == 1002)
		{
			ItemObject plantTool2 = GetPlantTool(1);
			if (plantTool2 != null)
			{
				plantTool2.CountUp(count);
				ChannelNetwork.SyncItem(_Network.WorldId, plantTool2);
			}
			else
			{
				ItemObject itemObject2 = ItemManager.CreateItem(protoId, count);
				ChannelNetwork.SyncItem(_Network.WorldId, itemObject2);
				if (SetPlantTool(1, itemObject2))
				{
					_Network.RPCOthers(EPacketType.PT_CL_FARM_SetPlantTool, 1, itemObject2.instanceId, true);
				}
			}
		}
		return true;
	}

	public override void MeetDemands(List<ItemIdCount> supplyItems)
	{
		foreach (ItemIdCount supplyItem in supplyItems)
		{
			if (supplyItem.protoId == 1003)
			{
				ItemObject plantTool = GetPlantTool(0);
				if (plantTool != null)
				{
					plantTool.CountUp(supplyItem.count);
					ChannelNetwork.SyncItem(_Network.WorldId, plantTool);
				}
				else
				{
					ItemObject itemObject = ItemManager.CreateItem(supplyItem.protoId, supplyItem.count);
					ChannelNetwork.SyncItem(_Network.WorldId, itemObject);
					if (SetPlantTool(0, itemObject))
					{
						_Network.RPCOthers(EPacketType.PT_CL_FARM_SetPlantTool, 0, itemObject.instanceId, true);
					}
				}
			}
			if (supplyItem.protoId != 1002)
			{
				continue;
			}
			ItemObject plantTool2 = GetPlantTool(1);
			if (plantTool2 != null)
			{
				plantTool2.CountUp(supplyItem.count);
				ChannelNetwork.SyncItem(_Network.WorldId, plantTool2);
				continue;
			}
			ItemObject itemObject2 = ItemManager.CreateItem(supplyItem.protoId, supplyItem.count);
			ChannelNetwork.SyncItem(_Network.WorldId, itemObject2);
			if (SetPlantTool(1, itemObject2))
			{
				_Network.RPCOthers(EPacketType.PT_CL_FARM_SetPlantTool, 1, itemObject2.instanceId, true);
			}
		}
	}
}
