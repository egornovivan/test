using System.Collections.Generic;
using System.Text.RegularExpressions;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSAutocycleMgr : MonoBehaviour
{
	private static CSAutocycleMgr mInstance;

	private double lastTime = -1.0;

	private int counter = 1;

	private double medicineSupplyTime = 7200.0;

	public CSAssembly core;

	public Dictionary<CSCommon, List<ItemIdCount>> ppcoalRequirements = new Dictionary<CSCommon, List<ItemIdCount>>();

	public Dictionary<CSCommon, List<ItemIdCount>> farmRequirements = new Dictionary<CSCommon, List<ItemIdCount>>();

	public Dictionary<CSCommon, List<ItemIdCount>> storageRequirements = new Dictionary<CSCommon, List<ItemIdCount>>();

	public Dictionary<CSCommon, List<ItemIdCount>> storageDesires = new Dictionary<CSCommon, List<ItemIdCount>>();

	public static CSAutocycleMgr Instance => mInstance;

	public bool HasPPCoalRequirements => ppcoalRequirements.Keys.Count > 0;

	public bool HasFarmRequirements => farmRequirements.Keys.Count > 0;

	public bool HasStorageRequirements => storageRequirements.Keys.Count > 0;

	private void Init()
	{
		core = CSMain.GetAssemblyEntity();
	}

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!PeGameMgr.IsSingle)
		{
			return;
		}
		if (core == null)
		{
			Init();
		}
		if (core == null)
		{
			return;
		}
		counter++;
		if (counter % 450 != 0)
		{
			return;
		}
		counter = 0;
		if (core.Storages == null)
		{
			return;
		}
		CSFactory factory = core.Factory;
		List<CSCommon> storages = core.Storages;
		ClearRequirements();
		List<CSCommon> belongCommons = core.GetBelongCommons();
		foreach (CSCommon item in belongCommons)
		{
			if (!(item is CSPPCoal) && !(item is CSFarm))
			{
				continue;
			}
			List<ItemIdCount> requirements = item.GetRequirements();
			if (requirements != null && requirements.Count > 0)
			{
				if (item is CSPPCoal)
				{
					ppcoalRequirements.Add(item, requirements);
				}
				if (item is CSFarm)
				{
					farmRequirements.Add(item, requirements);
				}
			}
		}
		bool transferedItem = false;
		if (storages != null)
		{
			if (factory != null)
			{
				List<ItemIdCount> compoudingEndItem = factory.GetCompoudingEndItem();
				if (compoudingEndItem.Count > 0)
				{
					List<int> autoProtoIdList = CSStorage.GetAutoProtoIdList();
					foreach (ItemIdCount item2 in compoudingEndItem)
					{
						if (autoProtoIdList.Contains(item2.protoId) && CSUtils.AddToStorage(item2.protoId, item2.count, core))
						{
							factory.CountDownItem(item2.protoId, item2.count);
							transferedItem = true;
						}
					}
				}
			}
			List<ItemIdCount> requirements2 = storages[0].GetRequirements();
			if (requirements2 != null && requirements2.Count > 0)
			{
				storageRequirements.Add(storages[0], requirements2);
			}
		}
		List<ItemIdCount> itemsNeedToGet = new List<ItemIdCount>();
		GetItemFromStorageAndFactory(ppcoalRequirements, storages, factory, ref itemsNeedToGet);
		GetItemFromStorageAndFactory(farmRequirements, storages, factory, ref itemsNeedToGet);
		GetItemFromStorageAndFactory(storageRequirements, storages, factory, ref itemsNeedToGet);
		if (itemsNeedToGet.Count > 0)
		{
			if (factory != null)
			{
				List<ItemIdCount> list = new List<ItemIdCount>();
				List<ItemIdCount> compoudingItem = factory.GetCompoudingItem();
				ItemIdCount needItem;
				foreach (ItemIdCount item3 in itemsNeedToGet)
				{
					needItem = item3;
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
					foreach (ItemIdCount item4 in list)
					{
						ic = item4;
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
				GetItemMaterialFromFactory(factory, itemsNeedToGet, ref transferedItem);
				if (transferedItem)
				{
					ShowTips(ETipType.factory_to_storage);
				}
				factory.CreateNewTaskWithItems(itemsNeedToGet);
			}
			List<ItemIdCount> list2 = ResolveItemsToProcess(itemsNeedToGet, core, factory);
			list2.RemoveAll((ItemIdCount it) => it.count <= 0);
			if (list2.Count > 0)
			{
				CSProcessing processingFacility = core.ProcessingFacility;
				if (processingFacility != null)
				{
					List<ItemIdCount> itemsInProcessing = processingFacility.GetItemsInProcessing();
					foreach (ItemIdCount item5 in itemsInProcessing)
					{
						CSUtils.RemoveItemIdCount(list2, item5.protoId, item5.count);
					}
				}
				if (list2.Count > 0)
				{
					processingFacility?.CreateNewTaskWithItems(list2);
				}
			}
		}
		ClearDesires();
		if (storages != null)
		{
			List<ItemIdCount> desires = storages[0].GetDesires();
			if (desires != null && desires.Count > 0)
			{
				storageDesires.Add(storages[0], desires);
				List<ItemIdCount> itemsNeedToGet2 = new List<ItemIdCount>();
				GetItemFromStorageAndFactory(storageDesires, storages, factory, ref itemsNeedToGet2);
				if (itemsNeedToGet2.Count > 0 && factory != null)
				{
					if (factory != null)
					{
						List<ItemIdCount> list3 = new List<ItemIdCount>();
						List<ItemIdCount> compoudingItem2 = factory.GetCompoudingItem();
						ItemIdCount needItem2;
						foreach (ItemIdCount item6 in itemsNeedToGet2)
						{
							needItem2 = item6;
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
							foreach (ItemIdCount item7 in list3)
							{
								ic2 = item7;
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
					factory.CreateNewTaskWithItems(itemsNeedToGet2);
				}
			}
		}
		if (core.MedicalCheck != null && core.MedicalTreat != null && core.MedicalTent != null && core.MedicalCheck.IsRunning && core.MedicalTreat.IsRunning && core.MedicalTent.IsRunning && core.MedicalCheck.WorkerCount + core.MedicalTreat.WorkerCount + core.MedicalTent.WorkerCount > 0)
		{
			if (lastTime < 0.0)
			{
				lastTime = GameTime.PlayTime.Second;
				return;
			}
			core.MedicineResearchState += GameTime.PlayTime.Second - lastTime;
			lastTime = GameTime.PlayTime.Second;
			if (storages == null || !(core.MedicineResearchState >= medicineSupplyTime))
			{
				return;
			}
			Debug.Log("supplyMedicineTime");
			core.MedicineResearchTimes++;
			List<ItemIdCount> list4 = new List<ItemIdCount>();
			foreach (MedicineSupply item8 in CSMedicineSupport.AllMedicine)
			{
				if ((float)core.MedicineResearchTimes - item8.rounds * (float)Mathf.FloorToInt((float)core.MedicineResearchTimes / item8.rounds) < 1f && CSUtils.GetItemCountFromAllStorage(item8.protoId, core) < ItemProto.GetItemData(item8.protoId).maxStackNum)
				{
					list4.Add(new ItemIdCount(item8.protoId, item8.count));
				}
			}
			if (list4.Count > 0)
			{
				if (CSUtils.AddItemListToStorage(list4, core))
				{
					ShowTips(ETipType.medicine_supply);
				}
				else
				{
					ShowTips(ETipType.storage_full);
				}
			}
			core.MedicineResearchState = 0.0;
			if (core.MedicineResearchTimes == int.MaxValue)
			{
				core.MedicineResearchTimes = 0;
			}
		}
		else
		{
			lastTime = GameTime.PlayTime.Second;
		}
	}

	public void GetItemFromStorageAndFactory(Dictionary<CSCommon, List<ItemIdCount>> requirementsMachine, List<CSCommon> storages, CSFactory factory, ref List<ItemIdCount> itemsNeedToGet)
	{
		foreach (KeyValuePair<CSCommon, List<ItemIdCount>> item in requirementsMachine)
		{
			ItemIdCount iic;
			foreach (ItemIdCount item2 in item.Value)
			{
				iic = item2;
				int num = 0;
				int num2 = 0;
				if (!(item.Key is CSStorage))
				{
					foreach (CSCommon storage in storages)
					{
						CSStorage cSStorage = storage as CSStorage;
						int itemCount = cSStorage.GetItemCount(iic.protoId);
						if (itemCount > 0)
						{
							if (itemCount >= iic.count - num)
							{
								num = iic.count;
								num2 = iic.count;
								break;
							}
							num += itemCount;
							num2 += itemCount;
						}
					}
					if (num == iic.count)
					{
						if (item.Key.MeetDemand(iic.protoId, num))
						{
							CSUtils.CountDownItemFromAllStorage(iic.protoId, num, core);
						}
						continue;
					}
				}
				int num3 = iic.count - num;
				int num4 = 0;
				if (factory != null)
				{
					int compoundEndItemCount = factory.GetCompoundEndItemCount(iic.protoId);
					if (compoundEndItemCount > 0)
					{
						int num5 = 0;
						num5 = ((compoundEndItemCount < num3) ? compoundEndItemCount : num3);
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
							CSUtils.CountDownItemFromAllStorage(iic.protoId, num2, core);
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
						CSUtils.CountDownItemFromAllStorage(iic.protoId, num2, core);
					}
					if (num4 > 0)
					{
						factory.CountDownItem(iic.protoId, num4);
					}
				}
				num3 = iic.count - num;
				ItemIdCount itemIdCount = itemsNeedToGet.Find((ItemIdCount it) => it.protoId == iic.protoId);
				if (itemIdCount != null)
				{
					itemIdCount.count += num3;
				}
				else
				{
					itemsNeedToGet.Add(new ItemIdCount(iic.protoId, num3));
				}
			}
		}
	}

	public void GetItemMaterialFromFactory(CSFactory factory, List<ItemIdCount> itemsNeedToGet, ref bool transferedItem)
	{
		foreach (ItemIdCount item in itemsNeedToGet)
		{
			Replicator.KnownFormula[] knowFormulasByProductItemId = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(item.protoId);
			if (knowFormulasByProductItemId == null || knowFormulasByProductItemId.Length == 0)
			{
				continue;
			}
			Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.Find(knowFormulasByProductItemId[0].id);
			foreach (Replicator.Formula.Material material in formula.materials)
			{
				int compoundEndItemCount = factory.GetCompoundEndItemCount(material.itemId);
				if (compoundEndItemCount > 0 && CSUtils.AddToStorage(material.itemId, compoundEndItemCount, core))
				{
					factory.CountDownItem(material.itemId, compoundEndItemCount);
					transferedItem = true;
				}
			}
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

	public void Check()
	{
	}

	public void SearchRequirement()
	{
	}

	public void AddRequirement(CSEntity entity)
	{
	}

	public void FinishRequirement()
	{
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
		string text = "?";
		string @string = PELocalization.GetString(replaceStrId);
		switch (type)
		{
		case ETipType.process_for_resource:
			text = CSUtils.GetNoFormatString(PELocalization.GetString(82201060), @string);
			break;
		case ETipType.process_for_storage:
			text = CSUtils.GetNoFormatString(PELocalization.GetString(82201062), @string);
			break;
		case ETipType.replicate_for:
			text = CSUtils.GetNoFormatString(PELocalization.GetString(82201064), @string);
			break;
		case ETipType.storage_full:
			text = CSUtils.GetNoFormatString(PELocalization.GetString(82201066), @string);
			break;
		case ETipType.factory_to_storage:
			text = CSUtils.GetNoFormatString(PELocalization.GetString(82201083), @string);
			break;
		case ETipType.medicine_supply:
			text = CSUtils.GetNoFormatString(PELocalization.GetString(82201087), @string);
			break;
		}
		new PeTipMsg(text, PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Colony);
		string[] array = Regex.Split(text, "\\[-\\] ", RegexOptions.IgnoreCase);
		if (array.Length >= 2)
		{
			CSUI_MainWndCtrl.ShowStatusBar(array[1], 10f);
		}
	}

	public static List<ItemIdCount> ResolveItemsToProcess(List<ItemIdCount> itemsNeedToGet, CSAssembly core = null, CSFactory factory = null)
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		List<ItemIdCount> list2 = new List<ItemIdCount>();
		foreach (ItemIdCount item in itemsNeedToGet)
		{
			if (CSProcessing.CanProcessItem(item.protoId))
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
					Replicator.KnownFormula[] knowFormulasByProductItemId = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(item2.protoId);
					if (knowFormulasByProductItemId == null || knowFormulasByProductItemId.Length == 0)
					{
						list3.Remove(item2);
						continue;
					}
					Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.Find(knowFormulasByProductItemId[0].id);
					Replicator.Formula.Material mt;
					foreach (Replicator.Formula.Material material in formula.materials)
					{
						mt = material;
						int num = mt.itemCount * Mathf.CeilToInt((float)item2.count * 1f / (float)formula.m_productItemCount);
						if (core != null)
						{
							int num2 = CSUtils.GetItemCountFromAllStorage(mt.itemId, core);
							if (factory != null)
							{
								num2 += factory.GetAllCompoundItemCount(mt.itemId);
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
						if (CSProcessing.CanProcessItem(mt.itemId))
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
			if (CSProcessing.CanProcessItem(protoId))
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
					Replicator.KnownFormula[] knowFormulasByProductItemId = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(item);
					if (knowFormulasByProductItemId == null || knowFormulasByProductItemId.Length == 0)
					{
						list2.Remove(item);
						continue;
					}
					Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.Find(knowFormulasByProductItemId[0].id);
					foreach (Replicator.Formula.Material material in formula.materials)
					{
						if (CSProcessing.CanProcessItem(material.itemId))
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
