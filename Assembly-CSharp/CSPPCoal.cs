using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSPPCoal : CSPowerPlant
{
	private const int tipsIntervalFramePPcoal = 2400;

	private int fuelID = CSInfoMgr.m_ppCoal.m_WorkedTimeItemID[0];

	private int fuelMaxCount = CSInfoMgr.m_ppCoal.m_WorkedTimeItemCnt[0];

	private float autoPercent = 0.2f;

	private int autoCount = 15;

	private CSPPCoalData m_PPData;

	private CounterScript m_CSWorking;

	private int tipsCounterPPcoal;

	public virtual int FuelID => fuelID;

	public virtual int FuelMaxCount => fuelMaxCount;

	public virtual float AutoPercent => autoPercent;

	public virtual int AutoCount => autoCount;

	public CSPPCoalData Data
	{
		get
		{
			if (m_PPData == null)
			{
				m_PPData = m_Data as CSPPCoalData;
			}
			return m_PPData;
		}
	}

	public float RestTime => Mathf.Max(Data.m_WorkedTime - Data.m_CurWorkedTime, 0f);

	public float RestPercent => RestTime / Data.m_WorkedTime;

	public CSPPCoal()
	{
		m_Type = 33;
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	public void StartWorkingCounter()
	{
		StartWorkingCounter(0f, base.Info.m_WorkedTime);
	}

	public void StartWorkingCounter(float curTime)
	{
		StartWorkingCounter(curTime, base.Info.m_WorkedTime);
	}

	public void StartWorkingCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (m_CSWorking == null)
			{
				m_CSWorking = CSMain.Instance.CreateCounter(base.Name + " Working", curTime, finalTime);
			}
			else
			{
				m_CSWorking.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				m_CSWorking.OnTimeUp = OnWorked;
			}
			ChangeState();
		}
	}

	public void StopCounter()
	{
		CSMain.Instance.DestoryCounter(m_CSWorking);
		m_CSWorking = null;
	}

	public void OnWorked()
	{
		m_IsRunning = false;
		DetachAllElectrics();
	}

	public override bool isWorking()
	{
		return m_CSWorking != null;
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		if (m_CSWorking != null)
		{
			Object.DestroyImmediate(m_CSWorking);
		}
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 33, ref refData) : MultiColonyManager.Instance.AssignData(ID, 33, ref refData, _ColonyObj));
		m_Data = refData as CSPPCoalData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = base.Info.m_Durability;
			StartWorkingCounter();
			return;
		}
		StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
		StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
		StartWorkingCounter(Data.m_CurWorkedTime, Data.m_WorkedTime);
		foreach (KeyValuePair<int, int> chargingItem in Data.m_ChargingItems)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.Get(chargingItem.Value);
			if (itemObject != null)
			{
				m_ChargingItems[chargingItem.Key] = PeSingleton<ItemMgr>.Instance.Get(chargingItem.Value).GetCmpt<Energy>();
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
		if (m_CSWorking != null)
		{
			Data.m_CurWorkedTime = m_CSWorking.CurCounter;
			Data.m_WorkedTime = m_CSWorking.FinalCounter;
		}
		else
		{
			Data.m_CurWorkedTime = 0f;
			Data.m_WorkedTime = -1f;
		}
		if (m_CSWorking != null)
		{
			m_CSWorking.enabled = base.Assembly != null;
		}
		if (!PeGameMgr.IsSingle && (!PeGameMgr.IsMulti || !(base._Net != null) || base._Net.TeamId != BaseNetwork.MainPlayer.TeamId))
		{
			return;
		}
		if (RestPercent <= 0.05f)
		{
			if (tipsCounterPPcoal % 2400 == 0)
			{
				switch (m_Type)
				{
				case 35:
					CSUtils.ShowTips(9500408, base.NameId);
					break;
				case 33:
					CSUtils.ShowTips(8000530, base.NameId);
					break;
				}
				tipsCounterPPcoal = 0;
			}
			tipsCounterPPcoal++;
		}
		else
		{
			tipsCounterPPcoal = 0;
		}
	}

	public override List<ItemIdCount> GetRequirements()
	{
		List<ItemIdCount> list = new List<ItemIdCount>();
		float num = Mathf.Max(Data.m_WorkedTime - Data.m_CurWorkedTime, 0f);
		float num2 = num / base.Info.m_WorkedTime;
		if (num2 < AutoPercent)
		{
			list.Add(new ItemIdCount(FuelID, AutoCount));
		}
		return list;
	}

	public override bool MeetDemand(int protoId, int count)
	{
		if (count <= 0)
		{
			return true;
		}
		float num = 1f / (float)FuelMaxCount;
		float num2 = (float)count * num;
		float num3 = num2 * base.Info.m_WorkedTime;
		float curTime = Mathf.Max(0f, Data.m_CurWorkedTime - num3);
		StartWorkingCounter(curTime);
		return true;
	}

	public override bool MeetDemand(ItemIdCount supplyItem)
	{
		return MeetDemand(supplyItem.protoId, supplyItem.count);
	}
}
