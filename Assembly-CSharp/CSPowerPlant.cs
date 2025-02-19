using System.Collections.Generic;
using CSRecord;
using ItemAsset;
using Pathea;
using UnityEngine;

public abstract class CSPowerPlant : CSCommon
{
	public int m_PPType;

	public float m_RestPower;

	protected CSPowerPlanetData m_PPBaseData;

	public CSPowerPlantInfo m_PPInfo;

	public List<CSElectric> m_Electrics;

	public Energy[] m_ChargingItems;

	public CSPowerPlanetData PPBaseData
	{
		get
		{
			if (m_PPBaseData == null)
			{
				m_PPBaseData = m_Data as CSPowerPlanetData;
			}
			return m_PPBaseData;
		}
	}

	public CSPowerPlantInfo Info
	{
		get
		{
			if (m_PPInfo == null)
			{
				m_PPInfo = m_Info as CSPowerPlantInfo;
			}
			return m_PPInfo;
		}
	}

	public bool bShowElectric
	{
		get
		{
			return PPBaseData.bShowElectric;
		}
		set
		{
			PPBaseData.bShowElectric = value;
		}
	}

	public float Radius => Info.m_Radius;

	public CSPowerPlant()
	{
		m_PPType = 32;
		m_Electrics = new List<CSElectric>();
		m_ChargingItems = new Energy[12];
		m_Grade = 2;
	}

	public bool InRange(Vector3 pos)
	{
		Vector2 a = new Vector2(pos.x, pos.z);
		Vector2 b = new Vector2(base.Position.x, base.Position.z);
		if (Vector2.Distance(a, b) <= Radius)
		{
			return true;
		}
		return false;
	}

	public void RemoveElectric(CSElectric csel)
	{
		m_RestPower += csel.m_Power;
		m_Electrics.Remove(csel);
	}

	public void AddElectric(CSElectric csel)
	{
		m_RestPower -= csel.m_Power;
		m_Electrics.Add(csel);
	}

	public virtual bool isWorking()
	{
		return true;
	}

	public void SetChargingItem(int index, ItemObject item)
	{
		if (PPBaseData == null)
		{
			return;
		}
		if (item == null)
		{
			m_ChargingItems[index] = null;
			PPBaseData.m_ChargingItems.Remove(index);
			return;
		}
		Energy cmpt = item.GetCmpt<Energy>();
		if (cmpt == null)
		{
			Debug.Log("Item cannot be charged!");
			return;
		}
		if (m_ChargingItems.Length <= index || index < 0)
		{
			Debug.Log("The giving index is out of arange!");
			return;
		}
		m_ChargingItems[index] = cmpt;
		if (PPBaseData.m_ChargingItems.ContainsKey(index))
		{
			if (item != null)
			{
				PPBaseData.m_ChargingItems[index] = item.instanceId;
			}
			else
			{
				PPBaseData.m_ChargingItems.Remove(index);
			}
		}
		else if (item != null)
		{
			PPBaseData.m_ChargingItems.Add(index, item.instanceId);
		}
	}

	public ItemObject GetChargingItem(int index)
	{
		if (PPBaseData == null)
		{
			return null;
		}
		if (m_ChargingItems.Length <= index || index < 0)
		{
			Debug.Log("The giving index is out of arange!");
			return null;
		}
		if (m_ChargingItems[index] == null)
		{
			return null;
		}
		return m_ChargingItems[index].itemObj;
	}

	public int GetChargingItemsCnt()
	{
		return m_ChargingItems.Length;
	}

	public override void ChangeState()
	{
		if (base.Assembly != null && base.Assembly.IsRunning && isWorking())
		{
			m_IsRunning = true;
			FindElectrics();
		}
		else
		{
			DetachAllElectrics();
			m_IsRunning = false;
		}
		GlobalEvent.NoticePowerPlantStateChanged();
	}

	public override void DestroySelf()
	{
		base.DestroySelf();
		GlobalEvent.NoticePowerPlantStateChanged();
	}

	public void AttachElectric(CSElectric cs)
	{
		if (!m_Electrics.Exists((CSElectric item0) => item0 == cs) && m_IsRunning && InRange(cs.Position) && m_RestPower >= cs.m_Power)
		{
			m_RestPower -= cs.m_Power;
			m_Electrics.Add(cs);
			cs.m_PowerPlant = this;
			cs.ChangeState();
		}
	}

	public void DetachElectric(CSElectric cs)
	{
		m_Electrics.Remove(cs);
		cs.m_PowerPlant = null;
		cs.ChangeState();
	}

	protected void FindElectrics()
	{
		foreach (KeyValuePair<CSConst.ObjectType, List<CSCommon>> item in base.Assembly.m_BelongObjectsMap)
		{
			if (CSAssembly.IsPowerPlant(item.Key))
			{
				continue;
			}
			foreach (CSCommon item2 in item.Value)
			{
				if (!(item2 is CSElectric cSElectric))
				{
					break;
				}
				if (!cSElectric.IsRunning)
				{
					AttachElectric(cSElectric);
				}
			}
		}
	}

	protected void DetachAllElectrics()
	{
		CSElectric[] array = m_Electrics.ToArray();
		CSElectric[] array2 = array;
		foreach (CSElectric cSElectric in array2)
		{
			DetachElectric(cSElectric);
			foreach (CSCommon allPowerPlant in cSElectric.Assembly.AllPowerPlants)
			{
				if (allPowerPlant != this)
				{
					CSPowerPlant cSPowerPlant = allPowerPlant as CSPowerPlant;
					cSPowerPlant.AttachElectric(cSElectric);
					if (cSElectric.IsRunning)
					{
						break;
					}
				}
			}
			cSElectric.ChangeState();
		}
	}

	protected virtual void ChargingItem(float deltaTime)
	{
		if (GameConfig.IsMultiMode)
		{
			return;
		}
		Energy[] chargingItems = m_ChargingItems;
		foreach (Energy energy in chargingItems)
		{
			if (energy != null)
			{
				energy.energy.Change(deltaTime * Info.m_ChargingRate * 10000f / Time.deltaTime);
				energy.energy.ChangePercent(deltaTime * Info.m_ChargingRate);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (m_IsRunning && PeGameMgr.IsSingle)
		{
			ChargingItem(Mathf.Clamp(Time.deltaTime * GameTime.Timer.ElapseSpeed, 0f, 50f));
		}
	}
}
