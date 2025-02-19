using System.Collections.Generic;
using CSRecord;
using CustomData;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using UnityEngine;

public class CSStorage : CSElectric
{
	public const int CYCLE_PROTOID_LIFE = 916;

	public const int CYCLE_PROTOID_STAMINA = 388;

	public const int CYCLE_PROTOID_COMFORT = 1479;

	public const int CYCLE_PROTOID_OTHER_0 = 49;

	public const int CYCLE_PROTOID_OTHER_1 = 50;

	public const int CYCLE_PROTOID_BATTERY = 228;

	public const int CYCLE_PROTOID_OTHER_3 = 1582;

	public const int CYCLE_PROTOID_CHARCOAL = 1001;

	public const int CYCLE_PROTOID_TORCH = 10;

	public const int CYCLE_PROTOID_FLOUR = 1024;

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

	public const float MAX_ADD_PERCENT_FOR_PER_NPC = 0.4f;

	public const int CYCLE_DESIRE_MIN_COUNT = 10;

	public const int CYCLE_DESIRE_ADD_COUNT = 20;

	private const int c_HistorMaxCount = 20;

	public const string c_TimeColorStr = "[00FFFF]";

	public const string c_NameColorStr = "[FFFF00]";

	public static int CYCLE_PROTOID_PPCOAL = CSInfoMgr.m_ppCoal.m_WorkedTimeItemID[0];

	public static int CYCLE_PROTOID_PPFUSION = CSInfoMgr.m_ppFusion.m_WorkedTimeItemID[0];

	public static int CYCLE_PROTOID_WATER = 1003;

	public static int CYCLE_PROTOID_INSECTICIDE = 1002;

	private CSStorageData m_SData;

	public CSStorageInfo m_SInfo;

	public ItemPackage m_Package;

	public CSStorageData Data
	{
		get
		{
			if (m_SData == null)
			{
				m_SData = m_Data as CSStorageData;
			}
			return m_SData;
		}
	}

	public CSStorageInfo Info
	{
		get
		{
			if (m_SInfo == null)
			{
				m_SInfo = m_Info as CSStorageInfo;
			}
			return m_SInfo;
		}
	}

	public CSStorage()
	{
		m_Type = 2;
		m_Package = new ItemPackage();
		m_Workers = new CSPersonnel[2];
		m_WorkSpaces = new PersonnelSpace[2];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
		{
			m_WorkSpaces[i] = new PersonnelSpace(this);
		}
		m_Grade = 3;
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	public HistoryStruct[] GetHistory()
	{
		return Data.m_History.ToArray();
	}

	public void AddHistory(CSStorageHistoryAttr historyAttr)
	{
		HistoryStruct historyStruct = new HistoryStruct();
		historyStruct.m_Day = historyAttr.m_Day;
		switch (historyAttr.m_Type)
		{
		case CSStorageHistoryAttr.EType.NpcAddSth:
		{
			string source2 = historyAttr.m_TimeStrColor + historyAttr.m_TimeStr + UIMsgBoxInfo.mStorageHistory_1.GetString();
			historyStruct.m_Value = CSUtils.GetNoFormatString(source2, historyAttr.m_NameColorStr + historyAttr.m_NpcName + "[FFFFFF]", historyAttr.m_ItemStr);
			break;
		}
		case CSStorageHistoryAttr.EType.NpcUseSth:
		{
			string source = historyAttr.m_TimeStrColor + historyAttr.m_TimeStr + UIMsgBoxInfo.mStorageHistory_2.GetString();
			historyStruct.m_Value = CSUtils.GetNoFormatString(source, historyAttr.m_NameColorStr + historyAttr.m_NpcName + "[FFFFFF]", historyAttr.m_ItemStr);
			break;
		}
		}
		Data.m_History.Enqueue(historyStruct);
		ExcuteEvent(3002, historyStruct);
		if (Data.m_History.Count > 20)
		{
			HistoryStruct historyStruct2 = Data.m_History.Dequeue();
			ExcuteEvent(3001, historyStruct2);
			Debug.Log(string.Concat("Storage history [", historyStruct2, "] remove."));
		}
	}

	public ItemObject FindSpecifiedItem()
	{
		List<ItemObject> list = new List<ItemObject>(m_Package.GetSlotList());
		return list.Find(delegate(ItemObject item0)
		{
			if (item0 == null)
			{
				return false;
			}
			Consume cmpt = item0.GetCmpt<Consume>();
			return null != cmpt;
		});
	}

	public void Remove(ItemObject item)
	{
		m_Package.RemoveItem(item);
		ExcuteEvent(3003, item);
	}

	public bool AddItemObj(int instanceId)
	{
		bool result = false;
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject != null && m_Package.CanAdd(itemObject.protoId, 1))
		{
			m_Package.AddItem(itemObject);
			result = true;
		}
		return result;
	}

	public bool RemoveItemObj(int instanceId)
	{
		bool result = false;
		ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(instanceId);
		if (itemObject != null)
		{
			result = m_Package.RemoveItem(itemObject);
			ExcuteEvent(3003, itemObject);
		}
		return result;
	}

	public int GetItemCount(int protoId)
	{
		return m_Package.GetCount(protoId);
	}

	public bool CountDownItem(int protoId, int count)
	{
		bool result = m_Package.Destroy(protoId, count);
		UpdateDataToUI();
		return result;
	}

	public bool CanAdd(int protoId, int count)
	{
		return m_Package.CanAdd(protoId, count);
	}

	public bool CanAdd(List<ItemIdCount> itemList)
	{
		List<MaterialItem> list = CSUtils.ItemIdCountToMaterialItem(itemList);
		return m_Package.CanAdd(list);
	}

	public bool Add(int protoId, int count)
	{
		bool result = m_Package.Add(protoId, count);
		UpdateDataToUI();
		return result;
	}

	public bool Add(List<ItemIdCount> itemList)
	{
		List<MaterialItem> list = CSUtils.ItemIdCountToMaterialItem(itemList);
		bool result = m_Package.Add(list);
		UpdateDataToUI();
		return result;
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
		list.Add(1001);
		list.Add(10);
		list.Add(1024);
		return list;
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 2, ref refData) : MultiColonyManager.Instance.AssignData(ID, 2, ref refData, _ColonyObj));
		m_Data = refData as CSStorageData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			return;
		}
		StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
		StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		foreach (KeyValuePair<int, int> item in Data.m_Items)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(item.Value);
			if (itemObject != null)
			{
				SlotList slotList = m_Package.GetSlotList(itemObject.protoId);
				int count = slotList.Count;
				int index = item.Key % count;
				slotList[index] = itemObject;
			}
		}
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
		foreach (KeyValuePair<int, int> item in Data.m_Items)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(item.Key);
		}
	}

	public override void Update()
	{
		base.Update();
		for (int i = 0; i < 4; i++)
		{
			SlotList slotList = m_Package.GetSlotList((ItemPackage.ESlotType)i);
			for (int j = 0; j < slotList.Count; j++)
			{
				int key = i * slotList.Count + j;
				if (Data.m_Items.ContainsKey(key))
				{
					if (slotList[j] == null)
					{
						Data.m_Items.Remove(key);
					}
					else
					{
						Data.m_Items[key] = slotList[j].instanceId;
					}
				}
				else if (slotList[j] != null)
				{
					Data.m_Items.Add(key, slotList[j].instanceId);
				}
			}
		}
	}

	public override void UpdateDataToUI()
	{
		if (GameUI.Instance != null)
		{
			GameUI.Instance.mCSUI_MainWndCtrl.StorageUI.StorageMainUI.RestItems();
		}
	}

	public override List<ItemIdCount> GetRequirements()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int[] eatIDs = NpcEatDb.GetEatIDs(EEatType.Hp);
		int[] eatIDs2 = NpcEatDb.GetEatIDs(EEatType.Hunger);
		int[] eatIDs3 = NpcEatDb.GetEatIDs(EEatType.Comfort);
		int[] array = eatIDs;
		foreach (int protoId in array)
		{
			num += CSUtils.GetItemCountFromAllStorage(protoId, base.Assembly);
		}
		int[] array2 = eatIDs2;
		foreach (int protoId2 in array2)
		{
			num2 += CSUtils.GetItemCountFromAllStorage(protoId2, base.Assembly);
		}
		int[] array3 = eatIDs3;
		foreach (int protoId3 in array3)
		{
			num3 += CSUtils.GetItemCountFromAllStorage(protoId3, base.Assembly);
		}
		List<ItemIdCount> list = new List<ItemIdCount>();
		if (num < 5 * base.m_MgCreator.GetNpcCount)
		{
			list.Add(new ItemIdCount(916, Mathf.Max(20, Mathf.CeilToInt((float)(5 * base.m_MgCreator.GetNpcCount) * 0.4f))));
		}
		if (num2 < 5 * base.m_MgCreator.GetNpcCount)
		{
			list.Add(new ItemIdCount(388, Mathf.Max(50, Mathf.CeilToInt((float)(5 * base.m_MgCreator.GetNpcCount) * 0.4f))));
		}
		if (num3 < 2 * base.m_MgCreator.GetNpcCount)
		{
			list.Add(new ItemIdCount(1479, Mathf.Max(10, Mathf.CeilToInt((float)(2 * base.m_MgCreator.GetNpcCount) * 0.4f))));
		}
		if (400 > CSUtils.GetItemCountFromAllStorage(49, base.Assembly))
		{
			list.Add(new ItemIdCount(49, 400));
		}
		if (140 > CSUtils.GetItemCountFromAllStorage(50, base.Assembly))
		{
			list.Add(new ItemIdCount(50, 140));
		}
		List<ItemObject> itemListInStorage = CSUtils.GetItemListInStorage(228, base.Assembly);
		if (itemListInStorage.Count < 2)
		{
			list.Add(new ItemIdCount(228, 1));
		}
		else
		{
			int num4 = 0;
			foreach (ItemObject item in itemListInStorage)
			{
				Energy cmpt = item.GetCmpt<Energy>();
				if (cmpt != null && cmpt.energy.percent > 0.2f)
				{
					num4++;
				}
			}
			if (num4 < 2)
			{
				list.Add(new ItemIdCount(228, 1));
			}
		}
		if (25 > CSUtils.GetItemCountFromAllStorage(1582, base.Assembly))
		{
			list.Add(new ItemIdCount(1582, 25));
		}
		if (50 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_PPCOAL, base.Assembly))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_PPCOAL, 50));
		}
		if (50 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_PPFUSION, base.Assembly))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_PPFUSION, 100));
		}
		if (100 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_WATER, base.Assembly))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_WATER, 88));
		}
		if (100 > CSUtils.GetItemCountFromAllStorage(CYCLE_PROTOID_INSECTICIDE, base.Assembly))
		{
			list.Add(new ItemIdCount(CYCLE_PROTOID_INSECTICIDE, 88));
		}
		return list;
	}

	public override bool MeetDemand(int protoId, int count)
	{
		if (CSUtils.CanAddToStorage(protoId, count, base.Assembly))
		{
			CSUtils.AddToStorage(protoId, count, base.Assembly);
			return true;
		}
		CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
		return false;
	}

	public override bool MeetDemand(ItemIdCount supplyItem)
	{
		if (CSUtils.CanAddToStorage(supplyItem.protoId, supplyItem.count, base.Assembly))
		{
			CSUtils.AddToStorage(supplyItem.protoId, supplyItem.count, base.Assembly);
			return true;
		}
		CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
		return false;
	}

	public override bool MeetDemands(List<ItemIdCount> supplyItems)
	{
		if (CSUtils.CanAddListToStorage(supplyItems, base.Assembly))
		{
			CSUtils.AddItemListToStorage(supplyItems, base.Assembly);
			return true;
		}
		CSAutocycleMgr.Instance.ShowTips(ETipType.storage_full);
		return false;
	}

	public override List<ItemIdCount> GetDesires()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		int[] eatIDs = NpcEatDb.GetEatIDs(EEatType.Hunger);
		int[] array = eatIDs;
		foreach (int protoId in array)
		{
			if (CSUtils.GetItemCountFromAllStorage(protoId, base.Assembly) < 10)
			{
				list.Add(new ItemIdCount(protoId, 20));
			}
		}
		return list;
	}
}
