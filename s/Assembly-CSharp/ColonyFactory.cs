using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ColonyFactory : ColonyBase
{
	public const int MAX_WORKER_COUNT = 4;

	public const int c_CompoudItemCount = 8;

	private CSFactoryData _MyData;

	public override int MaxWorkerCount => 4;

	public ColonyFactory(ColonyNetwork network)
	{
		SetNetwork(network);
		_RecordData = new CSFactoryData();
		_MyData = (CSFactoryData)_RecordData;
		LoadData();
		SetWorkerCountChangeEventHandler(OnWorkerCountChange);
	}

	public override void MyUpdate()
	{
		if (!IsWorking() || _MyData.m_CompoudItems.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _MyData.m_CompoudItems.Count; i++)
		{
			CompoudItem compoudItem = _MyData.m_CompoudItems[i];
			if (compoudItem != null && compoudItem.curTime < compoudItem.time)
			{
				compoudItem.curTime += 1f;
				if (compoudItem.curTime >= compoudItem.time)
				{
					_Network.RPCOthers(EPacketType.PT_CL_FCT_IsReady, compoudItem.itemID, i);
				}
				break;
			}
		}
		UpdateTimeTick(Time.time);
	}

	protected override void UpdateTimeTick(float curTime)
	{
		if ((int)curTime % 5 == 0)
		{
			SyncCompoundItem();
		}
	}

	public override void CombomData(BinaryWriter writer)
	{
		BufferHelper.Serialize(writer, _MyData.m_CompoudItems.Count);
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			BufferHelper.Serialize(writer, compoudItem.curTime);
			BufferHelper.Serialize(writer, compoudItem.time);
			BufferHelper.Serialize(writer, compoudItem.itemID);
			BufferHelper.Serialize(writer, compoudItem.itemCnt);
		}
	}

	public override void ParseData(byte[] data, int ver)
	{
		using MemoryStream input = new MemoryStream(data);
		using BinaryReader reader = new BinaryReader(input);
		int num = BufferHelper.ReadInt32(reader);
		for (int i = 0; i < num; i++)
		{
			CompoudItem compoudItem = new CompoudItem();
			compoudItem.curTime = BufferHelper.ReadSingle(reader);
			compoudItem.time = BufferHelper.ReadSingle(reader);
			compoudItem.itemID = BufferHelper.ReadInt32(reader);
			compoudItem.itemCnt = BufferHelper.ReadInt32(reader);
			_MyData.m_CompoudItems.Add(compoudItem);
		}
	}

	public override void InitMyData()
	{
	}

	public override bool IsWorking()
	{
		if (ColonyMgr._Instance.HasCoreAndPower(_Network.TeamId, _Network.transform.position))
		{
			return true;
		}
		return false;
	}

	public Replicator.Formula MultiMergeSkill(Player player, int skillId, int productCount)
	{
		if (_MyData.m_CompoudItems.Count >= 8)
		{
			return null;
		}
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.Find(skillId);
		foreach (Replicator.Formula.Material material in formula.materials)
		{
			if (player.GetItemNum(material.itemId) < material.itemCount * productCount)
			{
				return null;
			}
		}
		if (!ServerConfig.UnlimitedRes)
		{
			IEnumerable<ItemSample> items = formula.materials.Select((Replicator.Formula.Material iter) => new ItemSample(iter.itemId, iter.itemCount * productCount));
			ItemObject[] items2 = player.Package.RemoveItem(items);
			player.SyncItemList(items2);
			player.SyncPackageIndex();
		}
		float time = formula.timeNeed * (float)productCount;
		CreateCompoudItem(formula.productItemId, formula.m_productItemCount * productCount, time);
		return formula;
	}

	public void CreateCompoudItem(int itemID, int count, float time)
	{
		CompoudItem compoudItem = new CompoudItem();
		compoudItem.curTime = 0f;
		compoudItem.time = FixFinalTime(time);
		compoudItem.itemID = itemID;
		compoudItem.itemCnt = count;
		_MyData.m_CompoudItems.Add(compoudItem);
		_Network.RPCOthers(EPacketType.PT_CL_FCT_AddCompoudList, compoudItem);
		SyncSave();
	}

	public void OnWorkerCountChange()
	{
		for (int i = 0; i < _MyData.m_CompoudItems.Count; i++)
		{
			CompoudItem compoudItem = _MyData.m_CompoudItems[i];
			if (compoudItem != null && compoudItem.curTime < compoudItem.time)
			{
				float num = _MyData.m_CompoudItems[i].curTime / _MyData.m_CompoudItems[i].time;
				_MyData.m_CompoudItems[i].time = FixFinalTime(GetOriginalTime(_MyData.m_CompoudItems[i]));
				_MyData.m_CompoudItems[i].curTime = _MyData.m_CompoudItems[i].time * num;
			}
		}
	}

	public float GetOriginalTime(CompoudItem ci)
	{
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(ci.itemID);
		return (float)ci.itemCnt * formula.timeNeed / (float)formula.m_productItemCount;
	}

	public float GetWorkerParam()
	{
		float num = 1f;
		foreach (ColonyNpc value in _worker.Values)
		{
			if (value != null)
			{
				num *= 1f - value.GetCompoundSkill;
			}
		}
		return num;
	}

	private float FixFinalTime(float origin)
	{
		int getWorkingCount = base.GetWorkingCount;
		return origin * Mathf.Pow(0.82f, getWorkingCount) * GetWorkerParam() * 0.65f;
	}

	public int Fetch(Player player, int index)
	{
		if (!IsWorking())
		{
			return 0;
		}
		if (_MyData.m_CompoudItems.Count <= index)
		{
			return 0;
		}
		CompoudItem compoudItem = _MyData.m_CompoudItems[index];
		if (player == null || compoudItem == null)
		{
			return 0;
		}
		if (compoudItem.curTime < compoudItem.time)
		{
			return 0;
		}
		ItemProto itemData = ItemProto.GetItemData(compoudItem.itemID);
		if (itemData == null)
		{
			return 0;
		}
		int itemCnt = compoudItem.itemCnt;
		int num = compoudItem.itemCnt;
		List<ItemObject> effItems = new List<ItemObject>();
		if (itemData.maxStackNum == 1)
		{
			while (num > 0 && player.Package.GetEmptyIndex(itemData) >= 0)
			{
				ItemObject item = player.CreateItem(compoudItem.itemID, 1, syn: false);
				num--;
				effItems.Add(item);
				player.Package.AddItem(item);
			}
		}
		else if (player.Package.CanAdd(itemData.id, num))
		{
			player.Package.AddSameItems(itemData.id, num, ref effItems);
			num = 0;
		}
		if (num == itemCnt)
		{
			return 0;
		}
		player.SyncItemList(effItems);
		ItemSample item2 = new ItemSample(compoudItem.itemID, itemCnt - num);
		player.SyncNewItem(item2);
		player.SyncPackageIndex();
		if (num == 0)
		{
			_MyData.m_CompoudItems.Remove(compoudItem);
		}
		else
		{
			compoudItem.itemCnt = num;
		}
		SyncCompoundItem();
		SyncSave();
		return compoudItem.itemID;
	}

	public void OnCancelCompound(int index, CompoudItem ci)
	{
		if (_Network == null || _Network.transform == null || index >= _MyData.m_CompoudItems.Count || index < 0)
		{
			return;
		}
		CompoudItem compoudItem = _MyData.m_CompoudItems[index];
		if (compoudItem.itemID != ci.itemID || compoudItem.itemCnt != ci.itemCnt)
		{
			return;
		}
		List<ItemIdCount> list = new List<ItemIdCount>();
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(ci.itemID);
		foreach (Replicator.Formula.Material material in formula.materials)
		{
			list.Add(new ItemIdCount(material.itemId, material.itemCount * ci.itemCnt / formula.m_productItemCount));
		}
		if (!CSUtils.AddItemListToStorage(list, base.TeamId))
		{
			System.Random random = new System.Random();
			Vector3 vector = _Network.transform.position + _Network.transform.rotation * new Vector3(0f, 0f, 4f);
			Quaternion quaternion = Quaternion.Euler(0f, random.Next(360), 0f);
			RandomItemObj randomItemObj = new RandomItemObj(vector + new Vector3((float)random.NextDouble() * 0.1f, 0f, (float)random.NextDouble() * 0.1f), quaternion, CSUtils.ItemIdCountListToIntArray(list));
			RandomItemMgr.Instance.AddItemForFactoryCancel(randomItemObj);
			_Network.RPCOthers(EPacketType.PT_CL_FCT_GenFactoryCancel, randomItemObj.position, quaternion, randomItemObj.items);
		}
		_MyData.m_CompoudItems.Remove(compoudItem);
		SyncCompoundItem();
		SyncSave();
	}

	public List<ItemIdCount> GetCompoudingItem()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		CompoudItem ci;
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			ci = compoudItem;
			if (ci.curTime < ci.time)
			{
				ItemIdCount itemIdCount = list.Find((ItemIdCount it) => it.protoId == ci.itemID);
				if (itemIdCount != null)
				{
					itemIdCount.count += ci.itemCnt;
				}
				else
				{
					list.Add(new ItemIdCount(ci.itemID, ci.itemCnt));
				}
			}
		}
		return list;
	}

	public List<ItemIdCount> GetCompoudingEndItem()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		CompoudItem ci;
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			ci = compoudItem;
			if (ci.curTime >= ci.time)
			{
				ItemIdCount itemIdCount = list.Find((ItemIdCount it) => it.protoId == ci.itemID);
				if (itemIdCount != null)
				{
					itemIdCount.count += ci.itemCnt;
				}
				else
				{
					list.Add(new ItemIdCount(ci.itemID, ci.itemCnt));
				}
			}
		}
		return list;
	}

	public int GetAllCompoundItemCount(int protoId)
	{
		int num = 0;
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			if (compoudItem.itemID == protoId)
			{
				num += compoudItem.itemCnt;
			}
		}
		return num;
	}

	public int GetCompoundingItemCount(int protoId)
	{
		int num = 0;
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			if (compoudItem.curTime < compoudItem.time && compoudItem.itemID == protoId)
			{
				num += compoudItem.itemCnt;
			}
		}
		return num;
	}

	public int GetCompoundEndItemCount(int protoId)
	{
		int num = 0;
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			if (compoudItem.curTime >= compoudItem.time && compoudItem.itemID == protoId)
			{
				num += compoudItem.itemCnt;
			}
		}
		return num;
	}

	public bool CountDownItem(int protoId, int count)
	{
		bool flag = false;
		foreach (CompoudItem compoudItem in _MyData.m_CompoudItems)
		{
			if (compoudItem.curTime >= compoudItem.time && compoudItem.itemID == protoId)
			{
				if (compoudItem.itemCnt > count)
				{
					compoudItem.itemCnt -= count;
					count = 0;
				}
				else
				{
					count -= compoudItem.itemCnt;
					compoudItem.itemCnt = 0;
				}
				flag = true;
				if (count == 0)
				{
					break;
				}
			}
		}
		_MyData.m_CompoudItems.RemoveAll((CompoudItem it) => it.itemCnt == 0);
		if (flag)
		{
			SyncCompoundItem();
			SyncSave();
		}
		if (count == 0)
		{
			return true;
		}
		return false;
	}

	public void CreateNewTaskWithItems(List<ItemIdCount> allItemsList)
	{
		List<ColonyBase> colonyItemsByItemId = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1127);
		if (colonyItemsByItemId == null || colonyItemsByItemId.Count == 0)
		{
			return;
		}
		List<ColonyBase> colonyItemsByItemId2 = ColonyMgr._Instance.GetColonyItemsByItemId(base.TeamId, 1129);
		if (colonyItemsByItemId2 == null || colonyItemsByItemId2.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		List<ItemIdCount> list2 = new List<ItemIdCount>();
		foreach (ItemIdCount allItems in allItemsList)
		{
			ReplicateItem(allItems, list, out var materialList, out var productItemCount);
			if (allItems.count <= 0)
			{
				continue;
			}
			foreach (ItemIdCount item in materialList)
			{
				item.count *= Mathf.CeilToInt((float)allItems.count * 1f / (float)productItemCount);
				CSUtils.AddItemIdCount(list2, item.protoId, item.count);
			}
		}
		ReplicateMaterialRecursion(0, 2, list, list2);
		if (list.Count > 0)
		{
			ColonyMgr._Instance.GetColonyAssembly(base.TeamId)?.ShowReplicatorFor(list);
		}
		allItemsList.RemoveAll((ItemIdCount it) => it.count == 0);
	}

	public void ReplicateItem(ItemIdCount iic, List<int> replicatingItems, out List<ItemIdCount> materialList, out int productItemCount)
	{
		Replicator.Formula formula = Replicator.Formula.Mgr.Instance.FindByProductId(iic.protoId);
		materialList = new List<ItemIdCount>();
		productItemCount = 0;
		if (formula == null)
		{
			return;
		}
		foreach (Replicator.Formula.Material material in formula.materials)
		{
			materialList.Add(new ItemIdCount(material.itemId, material.itemCount));
		}
		productItemCount = formula.m_productItemCount;
		int num = Mathf.CeilToInt((float)iic.count * 1f / (float)formula.m_productItemCount);
		int materialListCount = CSUtils.GetMaterialListCount(materialList, base.TeamId);
		if (materialListCount == 0)
		{
			return;
		}
		if (materialListCount >= num)
		{
			if (_MyData.m_CompoudItems.Count >= 6)
			{
				return;
			}
			CreateCompoudItem(iic.protoId, num * formula.m_productItemCount, formula.timeNeed * (float)num);
			iic.count = 0;
			foreach (ItemIdCount material2 in materialList)
			{
				CSUtils.CountDownItemFromFacAndAllStorage(material2.protoId, material2.count * num, base.TeamId);
			}
			replicatingItems.Add(iic.protoId);
		}
		else
		{
			if (_MyData.m_CompoudItems.Count >= 6)
			{
				return;
			}
			CreateCompoudItem(iic.protoId, materialListCount * formula.m_productItemCount, formula.timeNeed * (float)materialListCount);
			iic.count -= materialListCount * formula.m_productItemCount;
			foreach (ItemIdCount material3 in materialList)
			{
				CSUtils.CountDownItemFromFacAndAllStorage(material3.protoId, material3.count * materialListCount, base.TeamId);
			}
			replicatingItems.Add(iic.protoId);
		}
	}

	public void ReplicateMaterialRecursion(int curDepth, int maxDepth, List<int> replicatingItems, List<ItemIdCount> materialList)
	{
		if (curDepth >= maxDepth)
		{
			return;
		}
		List<ItemIdCount> list = new List<ItemIdCount>();
		foreach (ItemIdCount material in materialList)
		{
			int num = CSUtils.GetItemCountFromAllStorage(material.protoId, base.TeamId) + GetAllCompoundItemCount(material.protoId);
			if (num > 0)
			{
				if (material.count <= num)
				{
					continue;
				}
				material.count -= num;
			}
			ReplicateItem(material, replicatingItems, out var materialList2, out var productItemCount);
			if (material.count <= 0)
			{
				continue;
			}
			foreach (ItemIdCount item in materialList2)
			{
				item.count *= Mathf.CeilToInt((float)material.count * 1f / (float)productItemCount);
				CSUtils.AddItemIdCount(list, item.protoId, item.count);
			}
		}
		if (list.Count != 0)
		{
			curDepth++;
			ReplicateMaterialRecursion(curDepth, maxDepth, replicatingItems, list);
		}
	}

	public void SyncCompoundItem()
	{
		_Network.RPCOthers(EPacketType.PT_CL_FCT_SyncAllItems, _MyData.m_CompoudItems.ToArray());
	}
}
