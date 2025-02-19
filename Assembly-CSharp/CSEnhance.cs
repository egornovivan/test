using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public class CSEnhance : CSWorkerMachine
{
	public delegate void EnhancedDel(Strengthen item);

	private CSEnhanceData m_EData;

	public CSEnhanceInfo m_EInfo;

	public Strengthen m_Item;

	public EnhancedDel onEnhancedTimeUp;

	private CounterScript m_Counter;

	public float m_CostsTime;

	public override bool IsDoingJobOn => base.IsRunning && IsEnhancing && m_Counter.enabled;

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

	public CSEnhanceData Data
	{
		get
		{
			if (m_EData == null)
			{
				m_EData = m_Data as CSEnhanceData;
			}
			return m_EData;
		}
	}

	public CSEnhanceInfo Info
	{
		get
		{
			if (m_EInfo == null)
			{
				m_EInfo = m_Info as CSEnhanceInfo;
			}
			return m_EInfo;
		}
	}

	public bool IsEnhancing => m_Counter != null;

	public float CostsTime => m_CostsTime;

	public CSEnhance()
	{
		m_Type = 4;
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
		return base.IsRunning && IsEnhancing;
	}

	public override float GetWorkerParam()
	{
		float num = 1f;
		CSPersonnel[] workers = m_Workers;
		foreach (CSPersonnel cSPersonnel in workers)
		{
			if (cSPersonnel != null)
			{
				num *= 1f - cSPersonnel.GetEnhanceSkill;
			}
		}
		return num;
	}

	public float CountFinalTime()
	{
		int workingCount = GetWorkingCount();
		float num = Info.m_BaseTime * Mathf.Pow(0.82f, workingCount);
		float workerParam = GetWorkerParam();
		return num * GetWorkerParam();
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
				m_Counter = CSMain.Instance.CreateCounter("Enhance", curTime, finalTime);
			}
			else
			{
				m_Counter.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				m_Counter.OnTimeUp = OnEnhanced;
			}
		}
	}

	public void StopCounter()
	{
		CSMain.Instance.DestoryCounter(m_Counter);
		m_Counter = null;
	}

	public void OnEnhanced()
	{
		if (m_Item == null)
		{
			Debug.LogWarning("The Enhance item is null, so cant be enhanced!");
			return;
		}
		if (onEnhancedTimeUp != null)
		{
			onEnhancedTimeUp(m_Item);
		}
		if (!GameConfig.IsMultiMode && m_Item != null)
		{
			m_Item.LevelUp();
		}
	}

	public Dictionary<int, int> GetCostsItems()
	{
		if (m_Item == null)
		{
			return null;
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		MaterialItem[] materialItems = m_Item.GetMaterialItems();
		if (materialItems == null)
		{
			return null;
		}
		MaterialItem[] array = materialItems;
		foreach (MaterialItem materialItem in array)
		{
			dictionary[materialItem.protoId] = Mathf.CeilToInt(materialItem.count);
		}
		return dictionary;
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
		bool flag = ((!GameConfig.IsMultiMode) ? m_Creator.m_DataInst.AssignData(ID, 4, ref refData) : MultiColonyManager.Instance.AssignData(ID, 4, ref refData, _ColonyObj));
		m_Data = refData as CSEnhanceData;
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
			m_Item = PeSingleton<ItemMgr>.Instance.Get(Data.m_ObjID).GetCmpt<Strengthen>();
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
			if (IsEnhancing)
			{
				m_Counter.SetFinalCounter(CountFinalTime());
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
