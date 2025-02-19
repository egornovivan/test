using System;
using System.Collections.Generic;
using CSRecord;
using CustomData;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSFactory : CSWorkerMachine
{
	public class CCompoudItem
	{
		public ItemObject item;

		public float curTime;

		public float Time;

		public int itemID;

		public int itemCnt;
	}

	public const int c_CompoudItemCount = 8;

	private CSFactoryData m_FData;

	public CSFactoryInfo m_FInfo;

	protected CounterScript m_Counter;

	public int m_CurCompoundIndex;

	public override bool IsDoingJobOn => m_Counter != null && base.IsRunning && m_Counter.enabled;

	public override GameObject gameLogic
	{
		get
		{
			return base.gameLogic;
		}
		set
		{
			base.gameLogic = value;
			if (!(gameLogic != null))
			{
				return;
			}
			PEMachine component = gameLogic.GetComponent<PEMachine>();
			if (component != null)
			{
				for (int i = 0; i < m_WorkSpaces.Length; i++)
				{
					m_WorkSpaces[i].WorkMachine = component;
				}
			}
		}
	}

	public CSFactoryData Data
	{
		get
		{
			if (m_FData == null)
			{
				m_FData = m_Data as CSFactoryData;
			}
			return m_FData;
		}
	}

	public CSFactoryInfo Info
	{
		get
		{
			if (m_FInfo == null)
			{
				m_FInfo = m_Info as CSFactoryInfo;
			}
			return m_FInfo;
		}
	}

	public int CompoudItemsCount => Data.m_CompoudItems.Count;

	public CSFactory()
	{
		m_Type = 8;
		m_Workers = new CSPersonnel[4];
		m_WorkSpaces = new PersonnelSpace[4];
		for (int i = 0; i < m_WorkSpaces.Length; i++)
		{
			m_WorkSpaces[i] = new PersonnelSpace(this);
		}
		m_Grade = 3;
	}

	public override bool IsDoingJob()
	{
		return m_Counter != null && base.IsRunning;
	}

	public bool SetCompoudItem(int itemID, int count, float time)
	{
		if (Data.m_CompoudItems.Count >= 8)
		{
			return false;
		}
		CompoudItem compoudItem = new CompoudItem();
		compoudItem.curTime = 0f;
		compoudItem.time = time;
		if (Application.isEditor && base.InTest)
		{
			compoudItem.time = 5f;
		}
		compoudItem.itemID = itemID;
		compoudItem.itemCnt = count;
		Data.m_CompoudItems.Add(compoudItem);
		return true;
	}

	public bool SetCompoudItemAuto(int itemID, int count, float time)
	{
		if (Data.m_CompoudItems.Count >= 6)
		{
			return false;
		}
		CompoudItem compoudItem = new CompoudItem();
		compoudItem.curTime = 0f;
		compoudItem.time = time;
		if (Application.isEditor && base.InTest)
		{
			compoudItem.time = 5f;
		}
		compoudItem.itemID = itemID;
		compoudItem.itemCnt = count;
		Data.m_CompoudItems.Add(compoudItem);
		return true;
	}

	public bool GetTakeAwayCompoundItem(int index, out CompoudItem outCompoudItem)
	{
		outCompoudItem = null;
		List<CompoudItem> compoudItems = Data.m_CompoudItems;
		if (index >= compoudItems.Count || index < 0)
		{
			return false;
		}
		if (compoudItems[index].curTime >= compoudItems[index].time)
		{
			outCompoudItem = compoudItems[index];
			return true;
		}
		outCompoudItem = compoudItems[index];
		return false;
	}

	public bool TakeAwayCompoudItem(int index)
	{
		List<CompoudItem> compoudItems = Data.m_CompoudItems;
		if (index >= compoudItems.Count || index < 0)
		{
			return false;
		}
		if (compoudItems[index].curTime >= compoudItems[index].time)
		{
			compoudItems.RemoveAt(index);
			m_CurCompoundIndex--;
			return true;
		}
		return false;
	}

	public bool TakeAwayCompoudItem(int index, out CompoudItem outCompoudItem)
	{
		outCompoudItem = null;
		List<CompoudItem> compoudItems = Data.m_CompoudItems;
		if (index >= compoudItems.Count || index < 0)
		{
			return false;
		}
		if (compoudItems[index].curTime >= compoudItems[index].time)
		{
			outCompoudItem = compoudItems[index];
			compoudItems.RemoveAt(index);
			m_CurCompoundIndex--;
			return true;
		}
		return false;
	}

	private void StartCompoud()
	{
		int count = Data.m_CompoudItems.Count;
		if (count != 0 && m_CurCompoundIndex < count && !(m_Counter != null))
		{
			float time = Data.m_CompoudItems[m_CurCompoundIndex].time;
			float curTime = Data.m_CompoudItems[m_CurCompoundIndex].curTime;
			time = FixFinalTime(time);
			_startCounter(curTime, time);
		}
	}

	public override float GetWorkerParam()
	{
		float num = 1f;
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null)
			{
				num *= 1f - cSPersonnel.GetCompoundSkill;
			}
		}
		return num;
	}

	private float FixFinalTime(float origin)
	{
		int workingCount = GetWorkingCount();
		return origin * Mathf.Pow(0.82f, workingCount) * GetWorkerParam() * 0.65f;
	}

	public override void RecountCounter()
	{
		if (m_Counter != null)
		{
			float num = m_Counter.CurCounter / m_Counter.FinalCounter;
			float num2 = FixFinalTime(GetCurOriginTime());
			float curTime = num2 * num;
			_startCounter(curTime, num2);
		}
	}

	public float GetCurOriginTime()
	{
		Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.FindByProductId(Data.m_CompoudItems[m_CurCompoundIndex].itemID);
		return (float)Data.m_CompoudItems[m_CurCompoundIndex].itemCnt * formula.timeNeed / (float)formula.m_productItemCount;
	}

	private void _startCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (m_Counter == null)
			{
				m_Counter = CSMain.Instance.CreateCounter("Compoud", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				m_Counter.OnTimeUp = _onCompoudingEnd;
			}
		}
	}

	private void _onCompoudingEnd()
	{
		int count = Data.m_CompoudItems.Count;
		if (m_CurCompoundIndex < count)
		{
			Data.m_CompoudItems[m_CurCompoundIndex].curTime = Data.m_CompoudItems[m_CurCompoundIndex].time;
			m_CurCompoundIndex++;
		}
	}

	private void MultiMode_onCompoudingEnd(int index)
	{
		m_CurCompoundIndex = index;
		Data.m_CompoudItems[m_CurCompoundIndex].curTime = Data.m_CompoudItems[m_CurCompoundIndex].time;
		if (m_CurCompoundIndex < Data.m_CompoudItems.Count)
		{
			m_CurCompoundIndex++;
		}
	}

	public void MultiModeIsReady(int index)
	{
		if (m_Counter != null)
		{
			CSMain.Instance.DestoryCounter(m_Counter);
			m_Counter = null;
		}
		MultiMode_onCompoudingEnd(index);
	}

	public List<ItemIdCount> GetCompoudingEndItem()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		CompoudItem ci;
		foreach (CompoudItem compoudItem in Data.m_CompoudItems)
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

	public List<ItemIdCount> GetCompoudingItem()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		CompoudItem ci;
		foreach (CompoudItem compoudItem in Data.m_CompoudItems)
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

	public int GetAllCompoundItemCount(int protoId)
	{
		int num = 0;
		foreach (CompoudItem compoudItem in Data.m_CompoudItems)
		{
			if (compoudItem.itemID == protoId)
			{
				num += compoudItem.itemCnt;
			}
		}
		return num;
	}

	public int GetCompoundEndItemCount(int protoId)
	{
		int num = 0;
		foreach (CompoudItem compoudItem in Data.m_CompoudItems)
		{
			if (compoudItem.curTime >= compoudItem.time && compoudItem.itemID == protoId)
			{
				num += compoudItem.itemCnt;
			}
		}
		return num;
	}

	public int GetCompoundingItemCount(int protoId)
	{
		int num = 0;
		foreach (CompoudItem compoudItem in Data.m_CompoudItems)
		{
			if (compoudItem.curTime < compoudItem.time && compoudItem.itemID == protoId)
			{
				num += compoudItem.itemCnt;
			}
		}
		return num;
	}

	public bool CountDownItem(int protoId, int count)
	{
		foreach (CompoudItem compoudItem in Data.m_CompoudItems)
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
				if (count == 0)
				{
					break;
				}
			}
		}
		int count2 = Data.m_CompoudItems.FindAll((CompoudItem it) => it.itemCnt == 0).Count;
		Data.m_CompoudItems.RemoveAll((CompoudItem it) => it.itemCnt == 0);
		m_CurCompoundIndex -= count2;
		if (count == 0)
		{
			return true;
		}
		return false;
	}

	public void OnCancelCompound(int index)
	{
		if (index >= Data.m_CompoudItems.Count)
		{
			return;
		}
		CompoudItem compoudItem = Data.m_CompoudItems[index];
		if (PeGameMgr.IsSingle)
		{
			List<ItemIdCount> list = new List<ItemIdCount>();
			Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.FindByProductId(compoudItem.itemID);
			foreach (Replicator.Formula.Material material in formula.materials)
			{
				list.Add(new ItemIdCount(material.itemId, material.itemCount * compoudItem.itemCnt / formula.m_productItemCount));
			}
			if (!CSUtils.AddItemListToStorage(list, base.Assembly))
			{
				System.Random random = new System.Random();
				Vector3 pos = ((!(gameLogic != null)) ? (base.Position + new Vector3(0f, 0f, 6f)) : (base.Position + gameLogic.transform.rotation * new Vector3(0f, 0f, 4f)));
				pos += new Vector3((float)random.NextDouble() * 0.1f, 0f, (float)random.NextDouble() * 0.1f);
				for (; RandomItemMgr.Instance.ContainsPos(pos); pos += new Vector3(0f, 0.01f, 0f))
				{
				}
				RandomItemMgr.Instance.GenFactoryCancel(pos, CSUtils.ItemIdCountListToIntArray(list));
			}
			if (m_CurCompoundIndex == index && m_Counter != null)
			{
				CSMain.Instance.DestoryCounter(m_Counter);
				m_Counter = null;
			}
			if (m_CurCompoundIndex > index)
			{
				m_CurCompoundIndex--;
			}
			Data.m_CompoudItems.Remove(compoudItem);
		}
		else
		{
			base._Net.RPCServer(EPacketType.PT_CL_FCT_GenFactoryCancel, index, compoudItem);
			if (m_CurCompoundIndex == index && m_Counter != null)
			{
				CSMain.Instance.DestoryCounter(m_Counter);
				m_Counter = null;
			}
			if (m_CurCompoundIndex > index)
			{
				m_CurCompoundIndex--;
			}
			Data.m_CompoudItems.Remove(compoudItem);
		}
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 8, ref refData) : MultiColonyManager.Instance.AssignData(ID, 8, ref refData, _ColonyObj));
		m_Data = refData as CSFactoryData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			return;
		}
		StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
		StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		int num = 0;
		while (num < Data.m_CompoudItems.Count)
		{
			ItemProto itemData = ItemProto.GetItemData(Data.m_CompoudItems[num].itemID);
			if (itemData == null)
			{
				Data.m_CompoudItems.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	public override void Update()
	{
		base.Update();
		if (!base.IsRunning)
		{
			if (m_Counter != null)
			{
				m_Counter.enabled = false;
			}
			return;
		}
		if (m_Counter != null)
		{
			m_Counter.enabled = true;
		}
		m_CurCompoundIndex = 0;
		if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
		{
			return;
		}
		while (Data.m_CompoudItems[m_CurCompoundIndex].curTime >= Data.m_CompoudItems[m_CurCompoundIndex].time)
		{
			m_CurCompoundIndex++;
			if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
			{
				m_CurCompoundIndex = Data.m_CompoudItems.Count;
				break;
			}
		}
		StartCompoud();
		if (m_Counter != null && Data.m_CompoudItems.Count > m_CurCompoundIndex && m_CurCompoundIndex >= 0)
		{
			Data.m_CompoudItems[m_CurCompoundIndex].curTime = m_Counter.CurCounter;
			Data.m_CompoudItems[m_CurCompoundIndex].time = m_Counter.FinalCounter;
		}
	}

	public void MultiModeTakeAwayCompoudItem(int index)
	{
		List<CompoudItem> compoudItems = Data.m_CompoudItems;
		if (index < compoudItems.Count && index >= 0)
		{
			compoudItems.RemoveAt(index);
			if (m_CurCompoundIndex > 0)
			{
				m_CurCompoundIndex--;
			}
		}
	}

	public void SetAllItems(CompoudItem[] itemList)
	{
		Data.m_CompoudItems.Clear();
		for (int i = 0; i < itemList.Length; i++)
		{
			Data.m_CompoudItems.Add(itemList[i]);
		}
		m_CurCompoundIndex = 0;
		if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
		{
			return;
		}
		while (Data.m_CompoudItems[m_CurCompoundIndex].curTime >= Data.m_CompoudItems[m_CurCompoundIndex].time)
		{
			m_CurCompoundIndex++;
			if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
			{
				m_CurCompoundIndex = Data.m_CompoudItems.Count;
				break;
			}
		}
		if (m_CurCompoundIndex >= Data.m_CompoudItems.Count)
		{
			if (m_Counter != null)
			{
				CSMain.Instance.DestoryCounter(m_Counter);
			}
		}
		else
		{
			float time = Data.m_CompoudItems[m_CurCompoundIndex].time;
			float curTime = Data.m_CompoudItems[m_CurCompoundIndex].curTime;
			_startCounter(curTime, time);
		}
	}

	public void CreateNewTaskWithItems(List<ItemIdCount> allItemsList)
	{
		if (base.Assembly == null || base.Assembly.Storages == null)
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
			CSAutocycleMgr.Instance.ShowReplicatorFor(list);
		}
		allItemsList.RemoveAll((ItemIdCount it) => it.count == 0);
	}

	public void ReplicateItem(ItemIdCount iic, List<int> replicatingItems, out List<ItemIdCount> materialList, out int productItemCount)
	{
		Replicator.KnownFormula[] knowFormulasByProductItemId = UIGraphControl.GetReplicator().GetKnowFormulasByProductItemId(iic.protoId);
		materialList = new List<ItemIdCount>();
		productItemCount = 0;
		if (knowFormulasByProductItemId == null || knowFormulasByProductItemId.Length == 0)
		{
			return;
		}
		Replicator.Formula formula = PeSingleton<Replicator.Formula.Mgr>.Instance.Find(knowFormulasByProductItemId[0].id);
		foreach (Replicator.Formula.Material material in formula.materials)
		{
			materialList.Add(new ItemIdCount(material.itemId, material.itemCount));
		}
		productItemCount = formula.m_productItemCount;
		int num = Mathf.CeilToInt((float)iic.count * 1f / (float)formula.m_productItemCount);
		int materialListCount = CSUtils.GetMaterialListCount(materialList, base.Assembly);
		if (materialListCount == 0)
		{
			return;
		}
		if (materialListCount >= num)
		{
			if (!SetCompoudItemAuto(iic.protoId, num * formula.m_productItemCount, formula.timeNeed * (float)num))
			{
				return;
			}
			iic.count = 0;
			foreach (ItemIdCount material2 in materialList)
			{
				CSUtils.CountDownItemFromFactoryAndAllStorage(material2.protoId, material2.count * num, base.Assembly);
			}
			replicatingItems.Add(iic.protoId);
		}
		else
		{
			if (!SetCompoudItemAuto(iic.protoId, materialListCount * formula.m_productItemCount, formula.timeNeed * (float)materialListCount))
			{
				return;
			}
			iic.count -= materialListCount * formula.m_productItemCount;
			foreach (ItemIdCount material3 in materialList)
			{
				CSUtils.CountDownItemFromFactoryAndAllStorage(material3.protoId, material3.count * materialListCount, base.Assembly);
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
			int num = CSUtils.GetItemCountFromAllStorage(material.protoId, base.Assembly) + GetAllCompoundItemCount(material.protoId);
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
}
