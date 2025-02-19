using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSPPSolar : CSPowerPlant
{
	public CSPPSolarData m_PPData;

	public CSPPSolarData Data
	{
		get
		{
			if (m_PPData == null)
			{
				m_PPData = m_Data as CSPPSolarData;
			}
			return m_PPData;
		}
	}

	public CSPPSolar()
	{
		m_Type = 34;
	}

	public override bool IsDoingJob()
	{
		return base.IsRunning;
	}

	protected override void ChargingItem(float deltaTime)
	{
		Energy[] chargingItems = m_ChargingItems;
		for (int i = 0; i < chargingItems.Length; i++)
		{
			chargingItems[i]?.energy.Change(deltaTime * base.Info.m_ChargingRate * 10000f / Time.deltaTime);
		}
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = m_Creator.m_DataInst.AssignData(ID, 34, ref refData);
		m_Data = refData as CSPowerPlanetData;
		if (flag)
		{
			Data.m_Name = CSUtils.GetEntityName(m_Type);
			Data.m_Durability = base.Info.m_Durability;
		}
		else
		{
			StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
			StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			foreach (KeyValuePair<int, int> chargingItem in Data.m_ChargingItems)
			{
				m_ChargingItems[chargingItem.Key] = PeSingleton<ItemMgr>.Instance.Get(chargingItem.Value).GetCmpt<Energy>();
			}
		}
		m_IsRunning = true;
	}

	public override void RemoveData()
	{
		m_Creator.m_DataInst.RemoveObjectData(ID);
	}

	public override void Update()
	{
		base.Update();
	}
}
