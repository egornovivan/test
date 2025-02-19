using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using uLink;
using UnityEngine;

public class ColonyAssembly : ColonyBase
{
	private CSAssemblyData _MyData;

	private int counter;

	public Dictionary<ColonyBase, List<ItemIdCount>> ppcoalRequirements = new Dictionary<ColonyBase, List<ItemIdCount>>();

	public Dictionary<ColonyBase, List<ItemIdCount>> farmRequirements = new Dictionary<ColonyBase, List<ItemIdCount>>();

	public Dictionary<ColonyBase, List<ItemIdCount>> storageRequirements = new Dictionary<ColonyBase, List<ItemIdCount>>();

	public Dictionary<ColonyBase, List<ItemIdCount>> storageDesires = new Dictionary<ColonyBase, List<ItemIdCount>>();

	private double medicineSupplyTime = 7200.0;

	private double lastTime = -1.0;

	public override int MaxWorkerCount => 4;

	public float Radius => CSAssemblyInfo.m_Levels[_MyData.m_Level].radius;

	public double MedicineResearchState
	{
		get
		{
			return _MyData.m_MedicineResearchState;
		}
		set
		{
			_MyData.m_MedicineResearchState = value;
		}
	}

	public int MedicineResearchTimes
	{
		get
		{
			return _MyData.m_MedicineResearchTimes;
		}
		set
		{
			_MyData.m_MedicineResearchTimes = value;
		}
	}

	public ColonyCheck MedicalCheck
	{
		get
		{
			List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1424);
			if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
			{
				return null;
			}
			return colonyItemsByItemId[0] as ColonyCheck;
		}
	}

	public ColonyTreat MedicalTreat
	{
		get
		{
			List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1422);
			if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
			{
				return null;
			}
			return colonyItemsByItemId[0] as ColonyTreat;
		}
	}

	public ColonyTent MedicalTent
	{
		get
		{
			List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1421);
			if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
			{
				return null;
			}
			return colonyItemsByItemId[0] as ColonyTent;
		}
	}

	public ColonyAssembly(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSAssemblyData();
		_MyData = (CSAssemblyData)_RecordData;
		LoadData();
	}

	public override void MyUpdate()
	{
		if (_MyData.m_CurUpgradeTime != -1f)
		{
			if (_MyData.m_CurUpgradeTime >= _MyData.m_UpgradeTime)
			{
				_MyData.m_CurUpgradeTime = -1f;
				_MyData.m_UpgradeTime = -1f;
				_MyData.m_Level++;
				SyncSave();
				_Network.RPCOthers(EPacketType.PT_CL_ASB_LevelUp, _MyData.m_Level);
			}
			else
			{
				if (ColonyMgr._Instance.InTest)
				{
					_MyData.m_CurUpgradeTime += 30f;
				}
				else
				{
					_MyData.m_CurUpgradeTime += 1f;
				}
				UpdateTimeTick(_MyData.m_CurUpgradeTime);
			}
		}
		UpdateAutocycle();
	}

	protected override void UpdateTimeTick(float curTime)
	{
		if ((int)curTime % 5 == 0)
		{
			_Network.RPCOthers(EPacketType.PT_CL_Counter_Tick, curTime);
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_ShowShield);
		BufferHelper.Serialize(writer, _MyData.m_Level);
		BufferHelper.Serialize(writer, _MyData.m_CurUpgradeTime);
		BufferHelper.Serialize(writer, _MyData.m_UpgradeTime);
		BufferHelper.Serialize(writer, _MyData.m_TimeTicks);
		BufferHelper.Serialize(writer, _MyData.m_MedicineResearchState);
		BufferHelper.Serialize(writer, _MyData.m_MedicineResearchTimes);
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		_MyData.m_ShowShield = BufferHelper.ReadBoolean(reader);
		_MyData.m_Level = BufferHelper.ReadInt32(reader);
		_MyData.m_CurUpgradeTime = BufferHelper.ReadSingle(reader);
		_MyData.m_UpgradeTime = BufferHelper.ReadSingle(reader);
		_MyData.m_TimeTicks = BufferHelper.ReadInt64(reader);
		if (ver >= 20160914)
		{
			_MyData.m_MedicineResearchState = BufferHelper.ReadDouble(reader);
			_MyData.m_MedicineResearchTimes = BufferHelper.ReadInt32(reader);
		}
	}

	public override void InitMyData()
	{
		_MyData.m_Level = 0;
		_MyData.m_CurUpgradeTime = -1f;
		_MyData.m_UpgradeTime = -1f;
		_MyData.m_ShowShield = true;
	}

	public bool LevelUp(Player player)
	{
		if (_MyData.m_CurUpgradeTime >= 0f && _MyData.m_CurUpgradeTime < _MyData.m_UpgradeTime)
		{
			return false;
		}
		if (_MyData.m_Level + 1 >= CSAssemblyInfo.m_Levels.Count)
		{
			return false;
		}
		if (!CheckLevelUpItems(player))
		{
			return false;
		}
		if (!DeleteLevelUpItems(player))
		{
			return false;
		}
		_MyData.m_CurUpgradeTime = 0f;
		_MyData.m_UpgradeTime = CSAssemblyInfo.m_Levels[_MyData.m_Level].upgradeTime;
		_Network.RPCOthers(EPacketType.PT_CL_ASB_LevelUpStart, _MyData.m_CurUpgradeTime, _MyData.m_UpgradeTime, player.roleName, true);
		SyncSave();
		return true;
	}

	public bool InRange(Vector3 pos)
	{
		return Radius >= Vector3.Distance(pos, _Network.transform.position);
	}

	internal bool CheckLevelUpItems(Player player)
	{
		for (int i = 0; i < CSAssemblyInfo.m_Levels[_MyData.m_Level].itemIDList.Count; i++)
		{
			int itemID = CSAssemblyInfo.m_Levels[_MyData.m_Level].itemIDList[i];
			if (player.GetItemNum(itemID) < CSAssemblyInfo.m_Levels[_MyData.m_Level].itemCnt[i])
			{
				return false;
			}
		}
		return true;
	}

	internal bool DeleteLevelUpItems(Player player)
	{
		if (!CheckLevelUpItems(player))
		{
			return false;
		}
		List<ItemObject> effItems = new List<ItemObject>(10);
		for (int i = 0; i < CSAssemblyInfo.m_Levels[_MyData.m_Level].itemIDList.Count; i++)
		{
			int itemID = CSAssemblyInfo.m_Levels[_MyData.m_Level].itemIDList[i];
			int count = CSAssemblyInfo.m_Levels[_MyData.m_Level].itemCnt[i];
			player.Package.RemoveItem(itemID, count, ref effItems);
		}
		player.SyncItemList(effItems);
		player.SyncPackageIndex();
		return true;
	}

	public void UpdateAutocycle()
	{
		counter++;
		if (counter % 10 != 0)
		{
			return;
		}
		counter = 0;
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1129);
		List<ColonyBase> colonyItemsByItemId2 = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1135);
		ColonyFactory colonyFactory = ((colonyItemsByItemId2 != null && colonyItemsByItemId2.Count != 0) ? (colonyItemsByItemId2[0] as ColonyFactory) : null);
		if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
		{
			ClearRequirements();
			List<ColonyBase> colonyItemsByItemId3 = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1128);
			if (colonyItemsByItemId3 != null)
			{
				foreach (ColonyBase item in colonyItemsByItemId3)
				{
					List<ItemIdCount> requirements = item.GetRequirements();
					if (requirements != null && requirements.Count > 0 && item is ColonyPPCoal)
					{
						ppcoalRequirements.Add(item, requirements);
					}
				}
			}
			List<ColonyBase> colonyItemsByItemId4 = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1134);
			if (colonyItemsByItemId4 != null)
			{
				foreach (ColonyBase item2 in colonyItemsByItemId4)
				{
					List<ItemIdCount> requirements2 = item2.GetRequirements();
					if (requirements2 != null && requirements2.Count > 0 && item2 is ColonyFarm)
					{
						farmRequirements.Add(item2, requirements2);
					}
				}
			}
			bool transferedFactoryItem = false;
			if (colonyItemsByItemId != null)
			{
				if (colonyFactory != null)
				{
					List<ItemIdCount> compoudingEndItem = colonyFactory.GetCompoudingEndItem();
					if (compoudingEndItem.Count > 0)
					{
						List<int> autoProtoIdList = ColonyStorage.GetAutoProtoIdList();
						foreach (ItemIdCount item3 in compoudingEndItem)
						{
							if (autoProtoIdList.Contains(item3.protoId) && CSUtils.AddToStorage(item3.protoId, item3.count, base.TeamId))
							{
								colonyFactory.CountDownItem(item3.protoId, item3.count);
								transferedFactoryItem = true;
							}
						}
					}
				}
				List<ItemIdCount> requirements3 = colonyItemsByItemId[0].GetRequirements();
				if (requirements3 != null && requirements3.Count > 0)
				{
					storageRequirements.Add(colonyItemsByItemId[0], requirements3);
				}
			}
			List<ItemIdCount> itemsNeedToGet = new List<ItemIdCount>();
			GetItemFromStorageAndFactory(ppcoalRequirements, colonyItemsByItemId, colonyFactory, ref itemsNeedToGet);
			GetItemFromStorageAndFactory(farmRequirements, colonyItemsByItemId, colonyFactory, ref itemsNeedToGet);
			GetItemFromStorageAndFactory(storageRequirements, colonyItemsByItemId, colonyFactory, ref itemsNeedToGet);
			if (itemsNeedToGet.Count > 0)
			{
				if (colonyFactory != null)
				{
					List<ItemIdCount> list = new List<ItemIdCount>();
					List<ItemIdCount> compoudingItem = colonyFactory.GetCompoudingItem();
					ItemIdCount needItem;
					foreach (ItemIdCount item4 in itemsNeedToGet)
					{
						needItem = item4;
						ItemIdCount itemIdCount = compoudingItem.Find((ItemIdCount it) => it.protoId == needItem.protoId);
						if (itemIdCount != null)
						{
							if (itemIdCount.count >= needItem.count)
							{
								list.Add(new ItemIdCount(itemIdCount.protoId, needItem.count));
							}
							else
							{
								list.Add(new ItemIdCount(itemIdCount.protoId, itemIdCount.count));
							}
						}
					}
					if (list.Count > 0)
					{
						ItemIdCount ic;
						foreach (ItemIdCount item5 in list)
						{
							ic = item5;
							ItemIdCount itemIdCount2 = itemsNeedToGet.Find((ItemIdCount it) => it.protoId == ic.protoId);
							if (itemIdCount2.count > ic.count)
							{
								itemIdCount2.count -= ic.count;
							}
							else
							{
								itemsNeedToGet.Remove(itemIdCount2);
							}
						}
					}
					GetItemMaterialFromFactory(colonyFactory, itemsNeedToGet, ref transferedFactoryItem);
					if (transferedFactoryItem)
					{
						ShowTips(ETipType.factory_to_storage);
					}
					colonyFactory.CreateNewTaskWithItems(itemsNeedToGet);
				}
				List<ItemIdCount> list2 = ResolveItemsToProcess(itemsNeedToGet, base.TeamId, colonyFactory);
				list2.RemoveAll((ItemIdCount it) => it.count <= 0);
				if (list2.Count > 0)
				{
					List<ColonyBase> colonyItemsByItemId5 = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1356);
					ColonyProcessing colonyProcessing = ((colonyItemsByItemId5 == null || colonyItemsByItemId5.Count <= 0) ? null : (colonyItemsByItemId5[0] as ColonyProcessing));
					if (colonyProcessing != null)
					{
						List<ItemIdCount> itemsInProcessing = colonyProcessing.GetItemsInProcessing();
						foreach (ItemIdCount item6 in itemsInProcessing)
						{
							CSUtils.RemoveItemIdCount(list2, item6.protoId, item6.count);
						}
					}
					if (list2.Count > 0)
					{
						colonyProcessing?.CreateNewTaskWithItems(list2);
					}
				}
			}
		}
		ClearDesires();
		if (colonyItemsByItemId != null && colonyItemsByItemId.Count > 0)
		{
			List<ItemIdCount> desires = colonyItemsByItemId[0].GetDesires();
			if (desires != null && desires.Count > 0)
			{
				storageDesires.Add(colonyItemsByItemId[0], desires);
				List<ItemIdCount> itemsNeedToGet2 = new List<ItemIdCount>();
				GetItemFromStorageAndFactory(storageDesires, colonyItemsByItemId, colonyFactory, ref itemsNeedToGet2);
				if (itemsNeedToGet2.Count > 0 && colonyFactory != null)
				{
					if (colonyFactory != null)
					{
						List<ItemIdCount> list3 = new List<ItemIdCount>();
						List<ItemIdCount> compoudingItem2 = colonyFactory.GetCompoudingItem();
						ItemIdCount needItem2;
						foreach (ItemIdCount item7 in itemsNeedToGet2)
						{
							needItem2 = item7;
							ItemIdCount itemIdCount3 = compoudingItem2.Find((ItemIdCount it) => it.protoId == needItem2.protoId);
							if (itemIdCount3 != null)
							{
								if (itemIdCount3.count >= needItem2.count)
								{
									list3.Add(new ItemIdCount(itemIdCount3.protoId, needItem2.count));
								}
								else
								{
									list3.Add(new ItemIdCount(itemIdCount3.protoId, itemIdCount3.count));
								}
							}
						}
						if (list3.Count > 0)
						{
							ItemIdCount ic2;
							foreach (ItemIdCount item8 in list3)
							{
								ic2 = item8;
								ItemIdCount itemIdCount4 = itemsNeedToGet2.Find((ItemIdCount it) => it.protoId == ic2.protoId);
								if (itemIdCount4.count > ic2.count)
								{
									itemIdCount4.count -= ic2.count;
								}
								else
								{
									itemsNeedToGet2.Remove(itemIdCount4);
								}
							}
						}
					}
					colonyFactory.CreateNewTaskWithItems(itemsNeedToGet2);
				}
			}
		}
		if (MedicalCheck != null && MedicalTreat != null && MedicalTent != null && MedicalCheck.IsWorking() && MedicalTreat.IsWorking() && MedicalTent.IsWorking() && MedicalCheck.GetWorkerAmount() + MedicalTreat.GetWorkerAmount() + MedicalTent.GetWorkerAmount() > 0)
		{
			if (lastTime < 0.0)
			{
				lastTime = GameTime.PlayTime.Second;
				return;
			}
			MedicineResearchState += GameTime.PlayTime.Second - lastTime;
			lastTime = GameTime.PlayTime.Second;
			if (colonyItemsByItemId != null && MedicineResearchState >= medicineSupplyTime)
			{
				if (LogFilter.logDebug)
				{
					Debug.Log("supplyMedicineTime");
				}
				MedicineResearchTimes++;
				List<ItemIdCount> list4 = new List<ItemIdCount>();
				foreach (MedicineSupply item9 in CSMedicineSupport.AllMedicine)
				{
					if ((float)MedicineResearchTimes - item9.rounds * (float)Mathf.FloorToInt((float)MedicineResearchTimes / item9.rounds) < 1f && CSUtils.GetItemCountFromAllStorage(item9.protoId, base.TeamId) < ItemProto.GetItemData(item9.protoId).maxStackNum)
					{
						list4.Add(new ItemIdCount(item9.protoId, item9.count));
					}
				}
				if (list4.Count > 0)
				{
					if (CSUtils.AddItemListToStorage(list4, base.TeamId))
					{
						ShowTips(ETipType.medicine_supply);
					}
					else
					{
						ShowTips(ETipType.storage_full);
					}
				}
				MedicineResearchState = 0.0;
				if (MedicineResearchTimes == int.MaxValue)
				{
					MedicineResearchTimes = 0;
				}
			}
			SyncSave();
		}
		else
		{
			lastTime = GameTime.PlayTime.Second;
		}
	}

	public void ClearRequirements()
	{
		ppcoalRequirements.Clear();
		farmRequirements.Clear();
		storageRequirements.Clear();
	}

	public void ClearDesires()
	{
		storageDesires.Clear();
	}

	public void GetItemMaterialFromFactory(ColonyFactory factory, List<ItemIdCount> itemsNeedToGet, ref bool transferedFactoryItem)
	{
		foreach (ItemIdCount item in itemsNeedToGet)
		{
			Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(item.protoId);
			if (formula == null)
			{
				continue;
			}
			foreach (Replicator.Formula.Material material in formula.materials)
			{
				int compoundEndItemCount = factory.GetCompoundEndItemCount(material.itemId);
				if (compoundEndItemCount > 0 && CSUtils.AddToStorage(material.itemId, compoundEndItemCount, base.TeamId))
				{
					factory.CountDownItem(material.itemId, compoundEndItemCount);
					transferedFactoryItem = false;
				}
			}
		}
	}

	public void GetItemFromStorageAndFactory(Dictionary<ColonyBase, List<ItemIdCount>> requiementsMachine, List<ColonyBase> storages, ColonyFactory factory, ref List<ItemIdCount> itemsNeedToGet)
	{
		foreach (KeyValuePair<ColonyBase, List<ItemIdCount>> item in requiementsMachine)
		{
			ItemIdCount iic;
			foreach (ItemIdCount item2 in item.Value)
			{
				iic = item2;
				int num = 0;
				int num2 = 0;
				if (!(item.Key is ColonyStorage))
				{
					foreach (ColonyBase storage in storages)
					{
						ColonyStorage colonyStorage = storage as ColonyStorage;
						int itemCount = colonyStorage.GetItemCount(iic.protoId);
						if (itemCount > 0)
						{
							if (itemCount >= iic.count - num)
							{
								num = iic.count;
								num2 = iic.count;
								break;
							}
							num += itemCount;
							num2 = iic.count;
						}
					}
					if (num == iic.count)
					{
						if (item.Key.MeetDemand(iic.protoId, num))
						{
							CSUtils.CountDownItemFromAllStorage(iic.protoId, num, base.TeamId);
						}
						continue;
					}
				}
				int num3 = iic.count - num;
				int num4 = 0;
				if (factory != null)
				{
					List<ItemIdCount> compoudingEndItem = factory.GetCompoudingEndItem();
					ItemIdCount itemIdCount = compoudingEndItem.Find((ItemIdCount it) => it.protoId == iic.protoId);
					if (itemIdCount != null)
					{
						int num5 = 0;
						num5 = ((itemIdCount.count < num3) ? itemIdCount.count : num3);
						num += num5;
						num4 += num5;
					}
				}
				if (num == iic.count)
				{
					if (item.Key.MeetDemand(iic.protoId, num))
					{
						if (num2 > 0)
						{
							CSUtils.CountDownItemFromAllStorage(iic.protoId, num2, base.TeamId);
						}
						if (num4 > 0)
						{
							factory.CountDownItem(iic.protoId, num4);
						}
					}
					continue;
				}
				if (num > 0 && item.Key.MeetDemand(iic.protoId, num))
				{
					if (num2 > 0)
					{
						CSUtils.CountDownItemFromAllStorage(iic.protoId, num2, base.TeamId);
					}
					if (num4 > 0)
					{
						factory.CountDownItem(iic.protoId, num4);
					}
				}
				num3 = iic.count - num;
				ItemIdCount itemIdCount2 = itemsNeedToGet.Find((ItemIdCount it) => it.protoId == iic.protoId);
				if (itemIdCount2 != null)
				{
					itemIdCount2.count += num3;
				}
				else
				{
					itemsNeedToGet.Add(new ItemIdCount(iic.protoId, num3));
				}
			}
		}
	}

	public void ShowReplicatorFor(List<int> productIdList)
	{
		ShowTips(ETipType.replicate_for, 82210002);
	}

	public void ShowProcessFor(List<ItemIdCount> processItems)
	{
		ShowTips(ETipType.process_for_storage);
	}

	public void ShowTips(ETipType type, int replaceStrId = -1)
	{
		uLink.NetworkPlayer playerPeer = Player.GetPlayerPeer(_Network.ownerId);
		if (!playerPeer.isUnassigned && playerPeer.isConnected)
		{
			_Network.RPCPeer(playerPeer, EPacketType.PT_CL_ASB_ShowTip, (int)type, replaceStrId);
		}
	}

	public static List<ItemIdCount> ResolveItemsToProcess(List<ItemIdCount> itemsNeedToGet, int teamId = -1, ColonyFactory cf = null)
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		List<ItemIdCount> list2 = new List<ItemIdCount>();
		foreach (ItemIdCount item in itemsNeedToGet)
		{
			if (ColonyProcessing.CanProcessItem(item.protoId))
			{
				list.Add(item);
				continue;
			}
			List<ItemIdCount> list3 = new List<ItemIdCount>();
			list3.Add(item);
			do
			{
				List<ItemIdCount> list4 = new List<ItemIdCount>();
				list4.AddRange(list3);
				foreach (ItemIdCount item2 in list4)
				{
					Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(item2.protoId);
					if (formula == null)
					{
						list3.Remove(item2);
						if (LogFilter.logDebug)
						{
							Debug.LogError("can't get " + item2.protoId + "for colony");
						}
						continue;
					}
					Replicator.Formula.Material mt;
					foreach (Replicator.Formula.Material material in formula.materials)
					{
						mt = material;
						int num = mt.itemCount * Mathf.CeilToInt((float)item2.count * 1f / (float)formula.m_productItemCount);
						if (teamId != -1)
						{
							int num2 = CSUtils.GetItemCountFromAllStorage(mt.itemId, teamId);
							if (cf != null)
							{
								num2 += cf.GetAllCompoundItemCount(mt.itemId);
							}
							ItemIdCount itemIdCount = list2.Find((ItemIdCount it) => it.protoId == mt.itemId);
							if (num2 > 0 && (itemIdCount == null || itemIdCount.count < num2))
							{
								int num3 = 0;
								num3 = ((itemIdCount != null) ? (num2 - itemIdCount.count) : num2);
								int num4 = 0;
								if (num > num3)
								{
									num -= num3;
									num4 = num3;
								}
								else
								{
									num = 0;
									num4 = num;
								}
								if (itemIdCount == null)
								{
									list2.Add(new ItemIdCount(mt.itemId, num4));
								}
								else
								{
									itemIdCount.count += num4;
								}
								if (num == 0)
								{
									continue;
								}
							}
						}
						if (ColonyProcessing.CanProcessItem(mt.itemId))
						{
							CSUtils.AddItemIdCount(list, mt.itemId, num);
						}
						else
						{
							CSUtils.AddItemIdCount(list3, mt.itemId, num);
						}
					}
					list3.Remove(item2);
				}
			}
			while (list3.Count > 0);
		}
		return list;
	}

	public static List<int> resolveProtoIdToProcess(List<int> protoIds)
	{
		List<int> list = new List<int>();
		foreach (int protoId in protoIds)
		{
			if (ColonyProcessing.CanProcessItem(protoId))
			{
				list.Add(protoId);
				continue;
			}
			List<int> list2 = new List<int>();
			list2.Add(protoId);
			do
			{
				List<int> list3 = new List<int>();
				list3.AddRange(list2);
				foreach (int item in list3)
				{
					Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(item);
					if (formula == null)
					{
						list2.Remove(item);
						if (LogFilter.logDebug)
						{
							Debug.LogError("can't get " + item + "for colony");
						}
						continue;
					}
					foreach (Replicator.Formula.Material material in formula.materials)
					{
						if (ColonyProcessing.CanProcessItem(material.itemId))
						{
							if (!list.Contains(material.itemId))
							{
								list.Add(material.itemId);
							}
						}
						else if (!list2.Contains(material.itemId))
						{
							list2.Add(material.itemId);
						}
					}
					list2.Remove(item);
				}
			}
			while (list2.Count > 0);
		}
		return list;
	}
}
