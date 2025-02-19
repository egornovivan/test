using System;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSRecycle : CSWorkerMachine
{
	public delegate void NoParmDel();

	private CSRecycleData m_RData;

	public CSRecycleInfo m_RInfo;

	public Recycle m_Item;

	public NoParmDel onRecylced;

	private CounterScript m_Counter;

	public float m_CostsTime;

	public override bool IsDoingJobOn => base.IsRunning && IsRecycling && m_Counter.enabled;

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

	public CSBuildingLogic BuildingLogic => base.gameLogic.GetComponent<CSBuildingLogic>();

	public CSRecycleData Data
	{
		get
		{
			if (m_RData == null)
			{
				m_RData = m_Data as CSRecycleData;
			}
			return m_RData;
		}
	}

	public CSRecycleInfo Info
	{
		get
		{
			if (m_RInfo == null)
			{
				m_RInfo = m_Info as CSRecycleInfo;
			}
			return m_RInfo;
		}
	}

	public bool IsRecycling => m_Counter != null;

	public float CostsTime => m_CostsTime;

	public CSRecycle()
	{
		m_Type = 6;
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
		return base.IsRunning && IsRecycling;
	}

	public override float GetWorkerParam()
	{
		float num = 1f;
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null)
			{
				num *= 1f - cSPersonnel.GetRecycleSkill;
			}
		}
		return num;
	}

	public float CountFinalTime()
	{
		int workingCount = GetWorkingCount();
		float num = Info.m_BaseTime * Mathf.Pow(0.82f, workingCount);
		float workerParam = GetWorkerParam();
		return num * workerParam;
	}

	public override void RecountCounter()
	{
		if (m_Counter != null)
		{
			float num = m_Counter.CurCounter / m_Counter.FinalCounter;
			float num2 = CountFinalTime();
			float curTime = num2 * num;
			StartCounter(curTime, num2);
		}
	}

	public void StartCounter()
	{
		float finalTime = CountFinalTime();
		StartCounter(0f, finalTime);
	}

	public void StartCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (m_Counter == null)
			{
				m_Counter = CSMain.Instance.CreateCounter("Recycle", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			if (!PeGameMgr.IsMulti)
			{
				m_Counter.OnTimeUp = OnRecycled;
			}
		}
	}

	public void StopCounter()
	{
		CSMain.Instance.DestoryCounter(m_Counter);
		m_Counter = null;
	}

	private void OnRecycled()
	{
		Dictionary<int, int> recycleItems = GetRecycleItems();
		if (recycleItems == null)
		{
			return;
		}
		List<ItemIdCount> list = new List<ItemIdCount>();
		foreach (KeyValuePair<int, int> item in recycleItems)
		{
			list.Add(new ItemIdCount(item.Key, item.Value));
		}
		if (list.Count <= 0)
		{
			return;
		}
		List<MaterialItem> list2 = CSUtils.ItemIdCountToMaterialItem(list);
		ItemPackage playerPak = PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package._playerPak;
		if (playerPak == null)
		{
			return;
		}
		bool flag = false;
		if (playerPak.CanAdd(list2))
		{
			playerPak.Add(list2);
			GameUI.Instance.mItemPackageCtrl.ResetItem();
			flag = true;
			CSUtils.ShowTips(82201094);
		}
		if (!flag && base.Assembly != null && base.Assembly.Storages != null)
		{
			foreach (CSCommon storage in base.Assembly.Storages)
			{
				CSStorage cSStorage = storage as CSStorage;
				if (cSStorage.m_Package.CanAdd(list2))
				{
					cSStorage.m_Package.Add(list2);
					flag = true;
					CSUtils.ShowTips(82201095);
					break;
				}
			}
		}
		if (!flag)
		{
			System.Random random = new System.Random();
			List<ItemIdCount> list3 = list.FindAll((ItemIdCount it) => it.count > 0);
			if (list3.Count <= 0)
			{
				return;
			}
			int[] items = CSUtils.ItemIdCountListToIntArray(list3);
			Vector3 vector = base.Position + new Vector3(0f, 0.72f, 0f);
			if (BuildingLogic != null && BuildingLogic.m_ResultTrans.Length > 0)
			{
				Transform transform = BuildingLogic.m_ResultTrans[random.Next(BuildingLogic.m_ResultTrans.Length)];
				if (transform != null)
				{
					vector = transform.position;
				}
			}
			for (; RandomItemMgr.Instance.ContainsPos(vector); vector += new Vector3(0f, 0.01f, 0f))
			{
			}
			RandomItemMgr.Instance.GenProcessingItem(vector + new Vector3((float)random.NextDouble() * 0.15f, 0f, (float)random.NextDouble() * 0.15f), items);
			flag = true;
			CSUtils.ShowTips(82201096);
		}
		if (flag)
		{
			PeSingleton<ItemMgr>.Instance.DestroyItem(m_Item.itemObj.instanceId);
			m_Item = null;
			if (onRecylced != null)
			{
				onRecylced();
			}
		}
	}

	public Dictionary<int, int> GetRecycleItems()
	{
		if (m_Item == null)
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		MaterialItem[] recycleItems = m_Item.GetRecycleItems();
		if (recycleItems != null && recycleItems.Length > 0)
		{
			for (int i = 0; i < recycleItems.Length; i++)
			{
				dictionary[recycleItems[i].protoId] = recycleItems[i].count * m_Item.itemObj.stackCount;
			}
		}
		return dictionary;
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		if (m_Counter != null)
		{
			UnityEngine.Object.Destroy(m_Counter);
		}
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 6, ref refData) : MultiColonyManager.Instance.AssignData(ID, 6, ref refData, _ColonyObj));
		m_Data = refData as CSRecycleData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = Info.m_Durability;
			return;
		}
		StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
		StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		if (PeSingleton<ItemMgr>.Instance.Get(Data.m_ObjID) == null)
		{
			m_Item = null;
		}
		else
		{
			m_Item = PeSingleton<ItemMgr>.Instance.Get(Data.m_ObjID).GetCmpt<Recycle>();
		}
		StartCounter(Data.m_CurTime, Data.m_Time);
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
		if (m_Item != null)
		{
			if (IsRecycling)
			{
				m_CostsTime = m_Counter.FinalCounter - m_Counter.CurCounter;
			}
			else
			{
				m_CostsTime = CountFinalTime();
			}
		}
		else
		{
			m_CostsTime = 0f;
		}
		if (m_Counter != null)
		{
			Data.m_CurTime = m_Counter.CurCounter;
			Data.m_Time = m_Counter.FinalCounter;
		}
		else
		{
			Data.m_CurTime = 0f;
			Data.m_Time = -1f;
		}
		if (m_Item != null)
		{
			Data.m_ObjID = m_Item.itemObj.instanceId;
		}
		else
		{
			Data.m_ObjID = -1;
		}
	}

	public override bool NeedWorkers()
	{
		return m_Item != null && m_IsRunning;
	}
}
