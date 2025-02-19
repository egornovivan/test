using ItemAsset;
using Pathea;
using UnityEngine;

public class PEEnergySheildLogic : PEEquipmentLogic, IRechargeableEquipment
{
	public float m_RechargeEnergySpeed;

	public float m_RechargeDelay;

	public float m_MaxEnergy;

	public bool m_Active = true;

	private Energy m_EnergyAttr;

	private float m_LastNetValue;

	private UTimer m_Time;

	public float enMax => m_MaxEnergy;

	public float enCurrent
	{
		get
		{
			return (!(null != m_Entity)) ? 0f : m_Entity.GetAttribute(AttribType.Shield);
		}
		set
		{
			if (m_EnergyAttr != null)
			{
				m_EnergyAttr.energy.current = value;
			}
			if (null != m_Entity)
			{
				m_Entity.SetAttribute(AttribType.Shield, value);
			}
		}
	}

	public float rechargeSpeed => m_RechargeEnergySpeed;

	public float lastUsedTime { get; set; }

	public float rechargeDelay => m_RechargeDelay;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Equip.SetEnergySheild(this);
		m_EnergyAttr = m_ItemObj.GetCmpt<Energy>();
		if (m_EnergyAttr != null)
		{
			m_Entity.SetAttribute(AttribType.Shield, m_EnergyAttr.energy.current);
			m_EnergyAttr.SetMax(m_MaxEnergy);
		}
		m_Entity.SetAttribute(AttribType.ShieldMax, m_MaxEnergy);
		lastUsedTime = Time.time;
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = 3.0;
		m_LastNetValue = enCurrent;
		if (!m_Active)
		{
			DeactiveSheild();
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		m_Entity.SetAttribute(AttribType.Shield, 0f);
		m_Entity.SetAttribute(AttribType.ShieldMax, 0f);
	}

	protected virtual void Update()
	{
		if (GameConfig.IsMultiMode && m_Time != null)
		{
			m_Time.Update(Time.deltaTime);
			if (m_Time.Second < 0.0 && Mathf.Abs(m_LastNetValue - enCurrent) > 3f)
			{
				m_Time.Second = 3.0;
				m_LastNetValue = enCurrent;
			}
		}
	}

	public virtual void ActiveSheild(bool fullCharge = false)
	{
		m_Active = true;
		if (fullCharge)
		{
			enCurrent = enMax;
		}
	}

	public virtual void DeactiveSheild()
	{
		m_Active = false;
		enCurrent = 0f;
	}
}
