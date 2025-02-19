using System;
using System.Collections.Generic;
using CSRecord;
using Pathea;
using Pathea.Operate;
using UnityEngine;

public abstract class CSEntity
{
	public delegate void OnDurabilityChange(float dura);

	public delegate void EventListnerDel(int event_id, CSEntity enti, object arg);

	private const float cHSRetainTime = 10f;

	public int tipsCounter0;

	public int tipsIntervalFrameDurabilityLow = 1800;

	public ColonyBase _ColonyObj;

	public int ID = -1;

	public int m_Type;

	protected int m_Grade;

	public CSCreator m_Creator;

	protected GameObject m_LogicObj;

	protected GameObject m_Object;

	public Transform[] workTrans;

	public Transform[] resultTrans;

	public PEPatients pePatient;

	public CSInfo m_Info;

	protected CSObjectData m_Data;

	protected CounterScript m_CSRepair;

	protected CounterScript m_CSDelete;

	protected bool m_IsRunning;

	public OnDurabilityChange onDuraChange;

	private int m_HealthState;

	private float m_HurtTime;

	private float m_RestoreTime;

	protected float m_RepairValue;

	public ColonyNetwork _Net
	{
		get
		{
			if (_ColonyObj == null)
			{
				return null;
			}
			return _ColonyObj._Network;
		}
	}

	public int Grade => m_Grade;

	public CSMgCreator m_MgCreator => m_Creator as CSMgCreator;

	public bool IsMine => m_Creator == CSMain.s_MgCreator;

	public virtual GameObject gameLogic
	{
		get
		{
			return m_LogicObj;
		}
		set
		{
			m_LogicObj = value;
		}
	}

	public virtual GameObject ModelObj
	{
		get
		{
			if (m_LogicObj != null && m_LogicObj.GetComponent<CSBuildingLogic>() != null)
			{
				return m_LogicObj.GetComponent<CSBuildingLogic>().ModelObj;
			}
			return null;
		}
	}

	public int logicId
	{
		get
		{
			if (gameLogic == null)
			{
				return -1;
			}
			if (gameLogic.GetComponent<CSBuildingLogic>() == null)
			{
				return -1;
			}
			return gameLogic.GetComponent<CSBuildingLogic>().id;
		}
	}

	public int PeEntityId
	{
		get
		{
			if (gameLogic == null)
			{
				return -1;
			}
			if (gameLogic.GetComponent<CSBuildingLogic>() == null)
			{
				return -1;
			}
			if (gameLogic.GetComponent<CSBuildingLogic>()._peEntity == null)
			{
				return -1;
			}
			return gameLogic.GetComponent<CSBuildingLogic>()._peEntity.Id;
		}
	}

	public bool InTest
	{
		get
		{
			if (gameLogic == null)
			{
				return false;
			}
			if (gameLogic.GetComponent<CSBuildingLogic>() == null)
			{
				return false;
			}
			return gameLogic.GetComponent<CSBuildingLogic>().InTest;
		}
	}

	public virtual GameObject gameObject
	{
		get
		{
			return m_Object;
		}
		set
		{
			m_Object = value;
		}
	}

	public string Name => CSUtils.GetEntityName(m_Type);

	public int NameId => CSUtils.GetEntityNameID(m_Type);

	public Vector3 Position
	{
		get
		{
			return m_Data.m_Position;
		}
		set
		{
			m_Data.m_Position = value;
		}
	}

	public int ItemID
	{
		get
		{
			return m_Data.ItemID;
		}
		set
		{
			m_Data.ItemID = value;
		}
	}

	public Bounds Bound
	{
		get
		{
			return m_Data.m_Bounds;
		}
		set
		{
			m_Data.m_Bounds = value;
		}
	}

	public float MaxDurability
	{
		get
		{
			return m_Info.m_Durability;
		}
		set
		{
			m_Info.m_Durability = value;
		}
	}

	public float CurrentDurability
	{
		get
		{
			return BaseData.m_Durability;
		}
		set
		{
			BaseData.m_Durability = value;
		}
	}

	public float DurabilityPercent => CurrentDurability / MaxDurability;

	public CSObjectData BaseData => m_Data;

	public CounterScript CSRepair => m_CSRepair;

	public CounterScript CSDelete => m_CSDelete;

	public bool isRepairing => m_CSRepair != null;

	public bool isDeleting => m_CSDelete != null;

	public bool IsRunning => m_IsRunning;

	public virtual bool IsDoingJobOn => IsRunning;

	public float DuraPercent
	{
		get
		{
			return BaseData.m_Durability / m_Info.m_Durability;
		}
		set
		{
			BaseData.m_Durability = Mathf.Max(m_Info.m_Durability * value, 0f);
			if (onDuraChange != null)
			{
				onDuraChange(BaseData.m_Durability);
			}
		}
	}

	public int HealthState => m_HealthState;

	private event EventListnerDel m_EventLisetner;

	public abstract bool IsDoingJob();

	public void AddEventListener(EventListnerDel listener)
	{
		this.m_EventLisetner = (EventListnerDel)Delegate.Combine(this.m_EventLisetner, listener);
	}

	public void RemoveEventListener(EventListnerDel listener)
	{
		this.m_EventLisetner = (EventListnerDel)Delegate.Remove(this.m_EventLisetner, listener);
	}

	public void ExcuteEvent(int event_type, object arg = null)
	{
		if (this.m_EventLisetner != null)
		{
			this.m_EventLisetner(event_type, this, arg);
		}
	}

	public void SetHealthState(int health_state)
	{
		switch (health_state)
		{
		case 2:
			m_HealthState |= health_state;
			m_HurtTime = 0f;
			break;
		case 8:
			m_HealthState |= health_state;
			m_RestoreTime = 0f;
			break;
		}
	}

	public void AddDeleteGetsItem(int itemId, int count)
	{
		m_Data.m_DeleteGetsItem.Add(itemId, count);
	}

	public void ClearDeleteGetsItem()
	{
		m_Data.m_DeleteGetsItem.Clear();
	}

	public void StartRepairCounter()
	{
		float num = (m_Info.m_Durability - m_Data.m_Durability) / m_Info.m_Durability;
		float num2 = m_Info.m_RepairTime * num;
		float repairVal = (m_Info.m_Durability - m_Data.m_Durability) / num2;
		StartRepairCounter(0f, num2, repairVal);
	}

	public void StartRepairCounter(float curTime, float finalTime, float repairVal)
	{
		if (!(finalTime < 0f))
		{
			if (m_CSRepair == null)
			{
				m_CSRepair = CSMain.Instance.CreateCounter(Name + " Repair", curTime, finalTime);
			}
			else
			{
				m_CSRepair.Init(curTime, finalTime);
			}
			m_CSRepair.OnTimeTick = OnRepairTick;
			m_RepairValue = repairVal;
		}
	}

	private void OnRepairTick(float deltaTime)
	{
		m_Data.m_Durability += m_RepairValue * deltaTime;
		m_Data.m_Durability = Mathf.Min(m_Data.m_Durability, m_Info.m_Durability);
		if (onDuraChange != null)
		{
			onDuraChange(BaseData.m_Durability);
		}
	}

	public void StartDeleteCounter()
	{
		float num = m_Data.m_Durability / m_Info.m_Durability;
		float finalTime = m_Info.m_DeleteTime * num;
		StartDeleteCounter(0f, finalTime);
	}

	public void StartDeleteCounter(float curTime, float finalTime)
	{
		if (!(finalTime < 0f))
		{
			if (m_CSDelete == null)
			{
				m_CSDelete = CSMain.Instance.CreateCounter(Name + " Delete", curTime, finalTime);
			}
			else
			{
				m_CSDelete.Init(curTime, finalTime);
			}
			if (!GameConfig.IsMultiMode)
			{
				m_CSDelete.OnTimeUp = OnDeleteTimesUp;
			}
		}
	}

	private void OnDeleteTimesUp()
	{
		m_Creator.RemoveEntity(ID);
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		foreach (KeyValuePair<int, int> item in m_Data.m_DeleteGetsItem)
		{
			PeSingleton<PeCreature>.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>().package.Add(item.Key, item.Value, newFlag: true);
		}
	}

	public virtual void DestroySelf()
	{
		Debug.Log("Delete " + Name + "successfully");
		m_MgCreator.RemoveLogic(ID);
		DragArticleAgent.Destory(logicId);
		PeSingleton<EntityMgr>.Instance.Remove(PeEntityId);
		if (m_CSRepair != null)
		{
			UnityEngine.Object.Destroy(m_CSRepair);
		}
		if (m_CSDelete != null)
		{
			UnityEngine.Object.Destroy(m_CSDelete);
		}
		if (m_Object != null)
		{
			UnityEngine.Object.Destroy(m_Object);
		}
		ExcuteEvent(1);
	}

	public virtual void ChangeState()
	{
		m_IsRunning = true;
	}

	public abstract void CreateData();

	public abstract void RemoveData();

	public virtual void Update()
	{
		if (m_CSRepair != null)
		{
			m_Data.m_CurRepairTime = m_CSRepair.CurCounter;
			m_Data.m_RepairTime = m_CSRepair.FinalCounter;
			m_Data.m_RepairValue = m_RepairValue;
		}
		else
		{
			m_Data.m_CurRepairTime = 0f;
			m_Data.m_RepairTime = -1f;
		}
		if (m_CSDelete != null)
		{
			m_Data.m_CurDeleteTime = m_CSDelete.CurCounter;
			m_Data.m_DeleteTime = m_CSDelete.FinalCounter;
		}
		else
		{
			m_Data.m_CurDeleteTime = 0f;
			m_Data.m_DeleteTime = -1f;
		}
		if (m_HurtTime >= 10f)
		{
			m_HealthState &= -3;
		}
		else
		{
			m_HurtTime += Time.deltaTime;
		}
		if (m_RestoreTime >= 10f)
		{
			m_HealthState &= -9;
		}
		else
		{
			m_HurtTime += Time.deltaTime;
		}
		if (!PeGameMgr.IsSingle && (!PeGameMgr.IsMulti || !(_Net != null) || _Net.TeamId != BaseNetwork.MainPlayer.TeamId))
		{
			return;
		}
		if (DurabilityPercent <= 0.1f)
		{
			if (tipsCounter0 % tipsIntervalFrameDurabilityLow == 0)
			{
				CSUtils.ShowTips(8000531, Name);
				tipsCounter0 = 0;
			}
			tipsCounter0++;
		}
		else
		{
			tipsCounter0 = 0;
		}
	}

	public virtual void AfterTurn90Degree()
	{
	}

	public bool ContainPoint(Vector3 pos)
	{
		return Bound.Contains(pos);
	}

	public void OnLifeChanged(float life_percent)
	{
		if (BaseData == null)
		{
			Debug.LogError("The data is not exsit!");
		}
		else
		{
			DuraPercent = life_percent;
		}
	}

	public virtual void DestroySomeData()
	{
	}

	public virtual void UpdateDataToUI()
	{
	}

	public virtual void StopWorking(int npcid)
	{
	}

	public virtual List<ItemIdCount> GetRequirements()
	{
		return null;
	}

	public virtual List<ItemIdCount> GetDesires()
	{
		return null;
	}

	public virtual bool MeetDemand(ItemIdCount supplyItem)
	{
		return true;
	}

	public virtual bool MeetDemand(int protoId, int count)
	{
		return true;
	}

	public virtual bool MeetDemands(List<ItemIdCount> supplyItems)
	{
		return true;
	}

	public virtual void InitAfterAllDataReady()
	{
	}
}
