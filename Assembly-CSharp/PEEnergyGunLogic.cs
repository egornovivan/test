using ItemAsset;
using Pathea;
using UnityEngine;

public class PEEnergyGunLogic : PEEquipmentLogic, IRechargeableEquipment
{
	public float m_RechargeEnergySpeed = 3f;

	public float m_RechargeDelay = 1.5f;

	public Magazine m_Magazine;

	private GunAmmo m_ItemAmmoAttr;

	private float m_LastNetValue;

	private UTimer m_Time;

	public virtual float magazineSize => m_Magazine.m_Size;

	public virtual float magazineValue
	{
		get
		{
			if (m_ItemAmmoAttr != null && Mathf.Abs((float)m_ItemAmmoAttr.count - m_Magazine.m_Value) > 0.8f)
			{
				m_Magazine.m_Value = m_ItemAmmoAttr.count;
			}
			return m_Magazine.m_Value;
		}
		set
		{
			m_Magazine.m_Value = value;
			if (m_ItemAmmoAttr != null)
			{
				m_ItemAmmoAttr.count = Mathf.RoundToInt(value);
			}
		}
	}

	public float enMax => magazineSize;

	public float enCurrent
	{
		get
		{
			return magazineValue;
		}
		set
		{
			magazineValue = value;
		}
	}

	public float rechargeSpeed => m_RechargeEnergySpeed;

	public float lastUsedTime { get; set; }

	public float rechargeDelay => m_RechargeDelay;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Equip.SetEnergyGunLogic(this);
		m_ItemAmmoAttr = itemObj.GetCmpt<GunAmmo>();
		if (m_ItemAmmoAttr != null && m_Magazine != null)
		{
			if (m_ItemAmmoAttr.count < 0)
			{
				m_ItemAmmoAttr.count = (int)magazineSize;
			}
			m_Magazine.m_Value = m_ItemAmmoAttr.count;
		}
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = 3.0;
		m_LastNetValue = enCurrent;
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
				PlayerNetwork.mainPlayer.RequestGunEnergyReload(m_Entity.Id, m_ItemObj.instanceId, enCurrent);
			}
		}
	}
}
