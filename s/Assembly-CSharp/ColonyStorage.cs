using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ColonyStorage : ColonyBase
{
	public const int CYCLE_PROTOID_LIFE = 916;

	public const int CYCLE_PROTOID_STAMINA = 388;

	public const int CYCLE_PROTOID_COMFORT = 1479;

	public const int CYCLE_PROTOID_OTHER_0 = 49;

	public const int CYCLE_PROTOID_OTHER_1 = 50;

	public const int CYCLE_PROTOID_BATTERY = 228;

	public const int CYCLE_PROTOID_OTHER_3 = 1582;

	public const int CYCLE_MIN_PER_NPC_LIFE = 5;

	public const int CYCLE_MIN_PER_NPC_STAMINA = 5;

	public const int CYCLE_MIN_PER_NPC_COMFORT = 2;

	public const int CYCLE_MIN_OTHER_0 = 400;

	public const int CYCLE_MIN_OTHER_1 = 140;

	public const int CYCLE_MIN_BATTERY = 2;

	public const float BATTERY_AVAILABLE = 0.2f;

	public const int CYCLE_MIN_OTHER_3 = 25;

	public const int CYCLE_MIN_PPCOAL = 50;

	public const int CYCLE_MIN_PPFUSION = 50;

	public const int CYCLE_MIN_WATER = 100;

	public const int CYCLE_MIN_INSECTICIDE = 100;

	public const int CYCLE_ADD_MIN_LIFE = 20;

	public const int CYCLE_ADD_MIN_STAMINA = 50;

	public const int CYCLE_ADD_MIN_COMFORT = 10;

	public const int CYCLE_ADD_MIN_OTHER_0 = 400;

	public const int CYCLE_ADD_MIN_OTHER_1 = 140;

	public const int CYCLE_ADD_MIN_BATTERY = 1;

	public const int CYCLE_ADD_MIN_OTHER_3 = 25;

	public const int CYCLE_ADD_MIN_PPCOAL = 50;

	public const int CYCLE_ADD_MIN_PPFUSION = 100;

	public const int CYCLE_ADD_MIN_WATER = 88;

	public const int CYCLE_ADD_MIN_INSECTICIDE = 88;

	public const int CYCLE_DESIRE_MIN_COUNT = 10;

	public const int CYCLE_DESIRE_ADD_COUNT = 20;

	public const float MAX_ADD_PERCENT_FOR_PER_NPC = 0.4f;

	public const int MAX_WORKER_COUNT = 4;

	public static int CYCLE_PROTOID_PPCOAL = CSPPCoalInfo.ppCoalInfo.m_WorkedTimeItemID[0];

	public static int CYCLE_PROTOID_PPFUSION = CSPPCoalInfo.ppFusionInfo.m_WorkedTimeItemID[0];

	public static int CYCLE_PROTOID_WATER = 1003;

	public static int CYCLE_PROTOID_INSECTICIDE = 1002;

	private CSStorageData _MyData;

	internal ItemPackage _Items = new ItemPackage(CSStorageInfo.m_MaxItem, CSStorageInfo.m_MaxEquip, CSStorageInfo.m_MaxRecource, CSStorageInfo.m_MaxArmor);

	public override int MaxWorkerCount => 4;

	public ColonyStorage(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSStorageData();
		_MyData = (CSStorageData)_RecordData;
		LoadData();
	}

	public override void MyUpdate()
	{
	}

	public override void CombomData(BinaryWriter writer)
	{
		CSStorageData cSStorageData = (CSStorageData)_RecordData;
		BufferHelper.Serialize(writer, cSStorageData.m_Items.Count);
		foreach (KeyValuePair<int, int> item in cSStorageData.m_Items)
		{
			BufferHelper.Serialize(writer, item.Key);
			BufferHelper.Serialize(writer, item.Value);
		}
		BufferHelper.Serialize(writer, cSStorageData.m_History.Count);
		foreach (HistoryStruct item2 in cSStorageData.m_History)
		{
			BufferHelper.Serialize(writer, item2.m_Day);
			BufferHelper.Serialize(writer, item2.m_Value);
		}
	}

	public override void ParseData(byte[] data, int ver)
	{
		CSStorageData cSStorageData = (CSStorageData)_RecordData;
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			int num2 = BufferHelper.ReadInt32(reader);
			int num3 = BufferHelper.ReadInt32(reader);
			ItemObject itemByID = ItemManager.GetItemByID(num3);
			if (itemByID != null)
			{
				cSStorageData.m_Items[num2] = num3;
				int index = DataIdxToPackageIdx(num2);
				_Items.AddItem(itemByID, index);
			}
		}
		num = BufferHelper.ReadInt32(reader);
		for (int j = 0; j < num; j++)
		{
			HistoryStruct historyStruct = new HistoryStruct();
			historyStruct.m_Day = BufferHelper.ReadInt32(reader);
			historyStruct.m_Value = BufferHelper.ReadString(reader);
			cSStorageData.m_History.Enqueue(historyStruct);
		}
	}

	public override void InitMyData()
	{
		InitPackage();
	}

	public void InitPackage()
	{
		foreach (KeyValuePair<int, int> item in _MyData.m_Items)
		{
			ItemObject itemByID = ItemManager.GetItemByID(item.Value);
			if (itemByID != null)
			{
				int count = _Items.GetItemList(0).Count;
				int index = item.Key % count;
				_Items.GetItemList(itemByID.protoData.tabIndex)[index] = itemByID;
			}
		}
	}

	public int PackageIdxToDataIdx(int index, int tableIdx)
	{
		int count = _Items.GetItemList(0).Count;
		return tableIdx * count + index;
	}

	public int DataIdxToPackageIdx(int index)
	{
		int count = _Items.GetItemList(0).Count;
		return index % count;
	}

	public bool Delete(int objId)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return false;
		}
		DeleteItem(itemByID);
		ItemManager.RemoveItem(objId);
		return true;
	}

	public int Store(int index, int objId, Player player)
	{
		if (!IsWorking())
		{
			return -1;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return -1;
		}
		if (index == -1)
		{
			index = _Items.GetEmptyIndex(itemByID.protoData.tabIndex);
			if (index == -1)
			{
				return -1;
			}
		}
		if (_Items.GetItemByIndex(index, itemByID.GetTabIndex()) != null)
		{
			return -1;
		}
		int num = PackageIdxToDataIdx(index, itemByID.GetTabIndex());
		if (_MyData.m_Items.ContainsKey(num))
		{
			return -1;
		}
		if (_Items.GetItemById(objId) != null)
		{
			return -1;
		}
		if (player != null && !player.RemoveEquipment(itemByID))
		{
			if (!player.Package.ExistID(itemByID))
			{
				return -1;
			}
			player.Package.RemoveItem(itemByID);
			player.SyncPackageIndex();
		}
		AddItem(num, itemByID);
		SyncSave();
		return index;
	}

	public bool Fetch(int objId, Player player, int index)
	{
		if (!IsWorking())
		{
			return false;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			return false;
		}
		if (!_MyData.m_Items.ContainsValue(objId))
		{
			return false;
		}
		if (_Items.GetItemById(objId) == null)
		{
			return false;
		}
		if (player.Package.GetEmptyGridCount(itemByID.protoData) <= 0)
		{
			return false;
		}
		ItemObject itemByIndex = player.Package.GetItemByIndex(index, itemByID.protoData);
		DeleteItem(itemByID);
		if (itemByIndex != null)
		{
			player.Package.AddItem(itemByID);
		}
		else
		{
			player.Package.AddItem(itemByID, index);
		}
		player.SyncPackageIndex();
		SyncSave();
		return true;
	}

	public bool Exchange(int objID, int originIndex, int destIndex)
	{
		if (!IsWorking())
		{
			return false;
		}
		int num = -1;
		int num2 = -1;
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null)
		{
			return false;
		}
		if (!_Items.ExistID(itemByID))
		{
			return false;
		}
		DeleteItem(itemByID);
		ItemObject itemByIndex = _Items.GetItemByIndex(destIndex, itemByID.protoData.tabIndex);
		if (itemByIndex != null)
		{
			DeleteItem(itemByIndex);
			num = PackageIdxToDataIdx(originIndex, itemByIndex.GetTabIndex());
			AddItem(num, itemByIndex);
		}
		num2 = PackageIdxToDataIdx(destIndex, itemByID.GetTabIndex());
		AddItem(num2, itemByID);
		_Network.RPCOthers(EPacketType.PT_CL_STO_Exchange, objID, destIndex, itemByIndex?.instanceId ?? (-1), originIndex, true);
		return true;
	}

	public bool Split(Player player, int objID, int num)
	{
		if (!IsWorking())
		{
			return false;
		}
		ItemObject itemByID = ItemManager.GetItemByID(objID);
		if (itemByID == null || null == player)
		{
			return false;
		}
		if (num >= itemByID.stackCount)
		{
			return false;
		}
		ItemPackage items = _Items;
		if (!items.ExistID(itemByID))
		{
			return false;
		}
		int emptyIndex = items.GetEmptyIndex(itemByID.protoData.tabIndex);
		if (emptyIndex == -1)
		{
			return false;
		}
		ItemObject itemObject = ItemManager.CreateFromItem(itemByID.protoId, num, itemByID);
		itemByID.CountDown(num);
		AddItem(PackageIdxToDataIdx(emptyIndex, itemObject.GetTabIndex()), itemObject);
		player.SyncItemList(new ItemObject[2] { itemByID, itemObject });
		_Network.RPCOthers(EPacketType.PT_CL_STO_Split, itemObject.instanceId, emptyIndex, true);
		return true;
	}

	public bool Sort(Player player, int tabIndex)
	{
		if (!IsWorking())
		{
			return false;
		}
		ItemPackage items = _Items;
		ItemObject[] items2 = items.Sort(tabIndex);
		ChannelNetwork.SyncItemList(_Network.WorldId, items2);
		Reset();
		IEnumerable<int> itemObjIDs = items.GetItemObjIDs(tabIndex);
		_Network.RPCOthers(EPacketType.PT_CL_STO_Sort, true, tabIndex, itemObjIDs.ToArray());
		return true;
	}

	private void AddItem(int index, ItemObject item)
	{
		int index2 = DataIdxToPackageIdx(index);
		_Items.AddItem(item, index2);
		_MyData.m_Items[index] = item.instanceId;
	}

	private void DeleteItem(ItemObject item)
	{
		_Items.RemoveItem(item);
		int index = GetIndex(item.instanceId);
		_MyData.m_Items.Remove(index);
	}

	private int GetIndex(int objId)
	{
		foreach (KeyValuePair<int, int> item in _MyData.m_Items)
		{
			if (item.Value == objId)
			{
				return item.Key;
			}
		}
		return -1;
	}

	private void Reset()
	{
		for (int i = 0; i < 3; i++)
		{
			List<ItemObject> itemList = _Items.GetItemList(i);
			for (int j = 0; j < itemList.Count; j++)
			{
				int key = i * itemList.Count + j;
				if (_MyData.m_Items.ContainsKey(key))
				{
					if (itemList[j] == null)
					{
						_MyData.m_Items.Remove(key);
					}
					else
					{
						_MyData.m_Items[key] = itemList[j].instanceId;
					}
				}
				else if (itemList[j] != null)
				{
					_MyData.m_Items.Add(key, itemList[j].instanceId);
				}
			}
		}
	}

	public int GetItemObjByItemId(int itemId)
	{
		foreach (KeyValuePair<int, int> item in _MyData.m_Items)
		{
			ItemObject itemByID = ItemManager.GetItemByID(item.Value);
			if (itemByID.protoId == itemId)
			{
				return item.Value;
			}
		}
		return 0;
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	internal IEnumerable<ItemObject> DeleteItemWithItemID(ItemProto item, int num)
	{
		if (item == null)
		{
			yield break;
		}
		ItemObject[] items = _Items.GetSameItems(item.id).ToArray();
		for (int i = 0; i < items.Length; i++)
		{
			if (num <= 0)
			{
				break;
			}
			int subNum = Mathf.Min(num, items[i].stackCount);
			if (!items[i].CountDown(subNum))
			{
				_Items.DeleteItem(items[i]);
				foreach (KeyValuePair<int, int> iter in _MyData.m_Items)
				{
					if (iter.Value == items[i].instanceId)
					{
						_MyData.m_Items.Remove(iter.Key);
						break;
					}
				}
			}
			else
			{
				yield return items[i];
			}
			num -= subNum;
		}
	}

	public int GetEmptyAmount(int tabidx)
	{
		return _Items.GetEmptyGridCount(tabidx);
	}

	public int GetItemCount(int protoId)
	{
		return _Items.GetItemCount(protoId);
	}

	public void CountDownItem(int protoId, int count)
	{
		List<int> list = new List<int>();
		List<ItemObject> list2 = _Items.GetSameItems(protoId).ToList();
		KeyValuePair<int, int> item;
		foreach (KeyValuePair<int, int> item2 in _MyData.m_Items)
		{
			item = item2;
			if (item.Value != -1 && list2.Find((ItemObject it) => it.instanceId == item.Value) != null)
			{
				list.Add(item.Key);
			}
		}
		ItemObject[] items = DeleteItemWithItemID(ItemProto.Mgr.Instance.Get(protoId), count).ToArray();
		ChannelNetwork.SyncItemList(_Network.WorldId, items);
		SyncDeleteItem(list);
	}

	public bool CanAdd(int protoId, int count)
	{
		return _Items.CanAdd(protoId, count);
	}

	public bool CanAdd(List<ItemIdCount> itemList)
	{
		List<ItemSample> items = CSUtils.ItemIdCountToSampleItems(itemList);
		return _Items.CanAdd(items);
	}

	public void Add(int protoId, int count)
	{
		List<ItemObject> effItems = new List<ItemObject>();
		_Items.AddSameItems(protoId, count, ref effItems);
		if (effItems.Count > 0)
		{
			ChannelNetwork.SyncItemList(_Network.WorldId, effItems);
		}
		Reset();
		SyncSave();
		List<int> list = new List<int>();
		foreach (ItemObject item in effItems)
		{
			list.Add(item.instanceId);
		}
		SyncAddItemList(list);
	}

	public void Add(List<ItemIdCount> itemList)
	{
		List<ItemSample> items = CSUtils.ItemIdCountToSampleItems(itemList);
		ItemObject[] array = _Items.AddSameItems(items);
		if (array != null && array.Length > 0)
		{
			ChannelNetwork.SyncItemList(_Network.WorldId, array);
		}
		Reset();
		SyncSave();
		List<int> list = new List<int>();
		ItemObject[] array2 = array;
		foreach (ItemObject itemObject in array2)
		{
			list.Add(itemObject.instanceId);
		}
		SyncAddItemList(list);
	}

	public void SyncAddItemList(List<int> instanceIdList)
	{
		if (instanceIdList == null || instanceIdList.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (KeyValuePair<int, int> item in _MyData.m_Items)
		{
			if (instanceIdList.Contains(item.Value))
			{
				list.Add(item.Key);
				list2.Add(item.Value);
			}
		}
		_Network.RPCOthers(EPacketType.PT_CL_STO_SyncItemList, list.ToArray(), list2.ToArray());
	}

	public void SyncDeleteItem(List<int> keyList)
	{
		if (keyList == null || keyList.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (int key in keyList)
		{
			if (_MyData.m_Items.ContainsKey(key))
			{
				list.Add(key);
				list2.Add(_MyData.m_Items[key]);
			}
			else
			{
				list.Add(key);
				list2.Add(-1);
			}
		}
		_Network.RPCOthers(EPacketType.PT_CL_STO_SyncItemList, list.ToArray(), list2.ToArray());
	}

	public override List<ItemIdCount> GetRequirements()
	{
		List<ColonyNpc> teamNpcs = ColonyNpcMgr.GetTeamNpcs(base.TeamId);
		int num = 0;
		if (teamNpcs != null)
		{
			num = teamNpcs.Count;
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int[] eatIDs = NpcEatDb.GetEatIDs(EEatType.Hp);
		int[] eatIDs2 = NpcEatDb.GetEatIDs(EEatType.Hunger);
		int[] eatIDs3 = NpcEatDb.GetEatIDs(EEatType.Comfort);
		int[] array = eatIDs;
		foreach (int protoId in array)
		{
			num2 += CSUtils.GetItemCountFromAllStorage(protoId, base.TeamId);
		}
		int[] array2 = eatIDs2;
		foreach (int protoId2 in array2)
		{
			num3 += CSUtils.GetItemCountFromAllStorage(protoId2, base.TeamId);
		}
		int[] array3 = eatIDs3;
		foreach (int protoId3 in array3)
		{
			num4 += CSUtils.GetItemCountFromAllStorage(protoId3, base.TeamId);
		}
		List<ItemIdCount> list = new List<ItemIdCount>();
		if (num2 < 5 * num)
		{
			list.Add(new ItemIdCount(916, Mathf.Max(20, Mathf.CeilToInt((float)(5 * num) * 0.4f))));
		}
		if (num3 < 5 * num)
		{
			list.Add(new ItemIdCount(388, Mathf.Max(50, Mathf.CeilToInt((float)(5 * num) * 0.4f))));
		}
		if (num4 < 2 * num)
		{
			list.Add(new ItemIdCount(1479, Mathf.Max(10, Mathf.CeilToInt((float)(2 * num) * 0.4f))));
		}
		if (400 > CSUtils.GetItemCountFromAllStorage(49, base.TeamId))
		{
			list.Add(new ItemIdCount(49, 400));
		}
		if (140 > CSUtils.GetItemCountFromAllStorage(50, base.TeamId))
		{
			list.Add(new ItemIdCount(50, 140));
		}
		List<ItemObject> itemListInStorage = CSUtils.GetItemListInStorage(228, base.TeamId);
		if (itemListInStorage.Count < 2)
		{
			list.Add(new ItemIdCount(228, 1));
		}
		else
		{
			int num5 = 0;
			foreach (ItemObject item in itemListInStorage)
			{
				Energy cmpt = item.GetCmpt<Energy>();
				if (cmpt != null && cmpt.energy.percent > 0.2f)
				{
					num5++;
				}
			}
			if (num5 < 2)
			{
				list.Add(new ItemIdCount(228, 1));
			}
		}
		if (25 > CSUtils.GetItemCountFromAllStorage(1582, base.TeamId))
		{
			list.Add(new ItemIdCount(1582, 25));
		}
		if (50 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_PPCOAL, base.TeamId))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_PPCOAL, 50));
		}
		if (50 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_PPFUSION, base.TeamId))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_PPFUSION, 100));
		}
		if (100 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_WATER, base.TeamId))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_WATER, 88));
		}
		if (100 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_INSECTICIDE, base.TeamId))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_INSECTICIDE, 88));
		}
		return list;
	}

	public override bool MeetDemand(int protoId, int count)
	{
		if (CSUtils.CanAddToStorage(protoId, count, base.TeamId))
		{
			CSUtils.AddToStorage(protoId, count, base.TeamId);
			return true;
		}
		ColonyMgr._Instance.GetColonyAssembly(base.TeamId)?.ShowTips(ETipType.storage_full);
		return false;
	}

	public override void MeetDemands(List<ItemIdCount> supplyItems)
	{
		if (CSUtils.CanAddListToStorage(supplyItems, base.TeamId))
		{
			CSUtils.AddItemListToStorage(supplyItems, base.TeamId);
		}
		else
		{
			ColonyMgr._Instance.GetColonyAssembly(base.TeamId)?.ShowTips(ETipType.storage_full);
		}
	}

	public static List<int> GetAutoProtoIdList()
	{
		List<int> list = new List<int>();
		list.Add(916);
		int[] eatIDs = NpcEatDb.GetEatIDs(EEatType.Hunger);
		list.AddRange(eatIDs);
		list.Add(1479);
		list.Add(49);
		list.Add(50);
		list.Add(228);
		list.Add(1582);
		list.Add(CYCLE_PROTOID_PPCOAL);
		list.Add(CYCLE_PROTOID_PPFUSION);
		list.Add(CYCLE_PROTOID_WATER);
		list.Add(CYCLE_PROTOID_INSECTICIDE);
		return list;
	}

	public override List<ItemIdCount> GetDesires()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		int[] eatIDs = NpcEatDb.GetEatIDs(EEatType.Hunger);
		int[] array = eatIDs;
		foreach (int protoId in array)
		{
			if (CSUtils.GetItemCountFromAllStorage(protoId, base.TeamId) < 10)
			{
				list.Add(new ItemIdCount(protoId, 20));
			}
		}
		return list;
	}
}
