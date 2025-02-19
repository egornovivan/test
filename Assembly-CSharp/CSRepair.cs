using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSRepair : CSWorkerMachine
{
	public delegate void RepairedDel(Repair item);

	private CSRepairData m_RData;

	public CSRepairInfo m_RInfo;

	public Repair m_Item;

	public RepairedDel onRepairedTimeUp;

	private CounterScript m_Counter;

	public float m_CostsTime;

	public override bool IsDoingJobOn => base.IsRunning && m_Counter != null && m_Counter.enabled;

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

	public CSRepairData Data
	{
		get
		{
			if (m_RData == null)
			{
				m_RData = m_Data as CSRepairData;
			}
			return m_RData;
		}
	}

	public CSRepairInfo Info
	{
		get
		{
			if (m_RInfo == null)
			{
				m_RInfo = m_Info as CSRepairInfo;
			}
			return m_RInfo;
		}
	}

	public bool IsRepairingM => m_Counter != null;

	public float CostsTime => m_CostsTime;

	public CSRepair()
	{
		m_Type = 5;
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
		return base.IsRunning && m_Counter != null;
	}

	public override float GetWorkerParam()
	{
		float num = 1f;
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null)
			{
				num *= 1f - cSPersonnel.GetRepairSkill;
			}
		}
		return num;
	}

	public float CountFinalTime()
	{
		int workingCount = GetWorkingCount();
		return Info.m_BaseTime * Mathf.Pow(0.82f, workingCount) * GetWorkerParam();
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

	public void CounterToRunning()
	{
		m_IsRunning = true;
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
				m_Counter = CSMain.Instance.CreateCounter("RepairItem", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				m_Counter.OnTimeUp = OnRepairItemEnd;
			}
			CounterToRunning();
		}
	}

	public void StopCounter()
	{
		Data.m_CurTime = 0f;
		Data.m_Time = -1f;
		CSMain.Instance.DestoryCounter(m_Counter);
		m_Counter = null;
	}

	public void OnRepairItemEnd()
	{
		if (m_Item == null)
		{
			Debug.LogWarning("The Repair item is null, so cant be repaired!");
			return;
		}
		if (onRepairedTimeUp != null)
		{
			onRepairedTimeUp(m_Item);
		}
		if (!GameConfig.IsMultiMode)
		{
			Repairing();
		}
	}

	public void Repairing()
	{
		if (m_Item != null)
		{
			m_Item.Do();
		}
	}

	public Dictionary<int, int> GetCostsItems()
	{
		if (m_Item == null)
		{
			return null;
		}
		if (m_Item.GetValue().IsCurrentMax())
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		List<MaterialItem> requirements = m_Item.GetRequirements();
		foreach (MaterialItem item in requirements)
		{
			dictionary[item.protoId] = item.count;
		}
		return dictionary;
	}

	public float GetIncreasingDura()
	{
		if (m_Item == null)
		{
			return 0f;
		}
		return m_Item.GetValue().ExpendValue;
	}

	public bool IsFull()
	{
		if (m_Item == null)
		{
			return false;
		}
		return m_Item.GetValue().IsCurrentMax();
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		if (m_Counter != null)
		{
			Object.Destroy(m_Counter);
		}
	}

	public override void CreateData()
	{
		CSDefaultData refData = null;
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 5, ref refData) : MultiColonyManager.Instance.AssignData(ID, 5, ref refData, _ColonyObj));
		m_Data = refData as CSRepairData;
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
			m_Item = PeSingleton<ItemMgr>.Instance.Get(Data.m_ObjID).GetCmpt<Repair>();
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
		if (m_Item != null)
		{
			Data.m_ObjID = m_Item.itemObj.instanceId;
		}
		else
		{
			Data.m_ObjID = -1;
		}
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
			if (IsRepairingM)
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
	}

	public override bool NeedWorkers()
	{
		return m_Item != null && m_IsRunning;
	}
}
