using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;

public class CSPPFusion : CSPPCoal
{
	private int fuelID = CSInfoMgr.m_ppFusion.m_WorkedTimeItemID[0];

	private int fuelMaxCount = CSInfoMgr.m_ppFusion.m_WorkedTimeItemCnt[0];

	private float autoPercent = 0.2f;

	private int autoCount = 15;

	private CSPPFusionData m_PPData;

	public override int FuelID => fuelID;

	public override int FuelMaxCount => fuelMaxCount;

	public override float AutoPercent => autoPercent;

	public override int AutoCount => autoCount;

	public new CSPPFusionData Data
	{
		get
		{
			if (m_PPData == null)
			{
				m_PPData = m_Data as CSPPFusionData;
			}
			return m_PPData;
		}
	}

	public CSPPFusion()
	{
		m_Type = 35;
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 35, ref refData) : MultiColonyManager.Instance.AssignData(ID, 35, ref refData, _ColonyObj));
		m_Data = refData as CSPPFusionData;
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
}
