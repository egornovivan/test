using ItemAsset;
using Pathea;
using UnityEngine;

public class PEJetPackLogic : PEEquipmentLogic, IRechargeableEquipment
{
	public float m_BoostPowerUp = 12f;

	public float m_MaxUpSpeed = 7f;

	public float m_BoostHorizonalSpeed = 2f;

	public float m_EnergyMax = 50f;

	public float m_EnergyThreshold = 20f;

	public float m_CostSpeed = 5f;

	public float m_RechargeSpeed = 2f;

	public float m_RechargeDelay = 2f;

	private float m_LastNetValue;

	private UTimer m_Time;

	private JetPkg m_ItemAttr;

	public float enMax => m_EnergyMax;

	public float enCurrent
	{
		get
		{
			return (m_ItemAttr == null) ? 0f : m_ItemAttr.energy;
		}
		set
		{
			if (m_ItemAttr != null)
			{
				m_ItemAttr.energy = value;
			}
		}
	}

	public float rechargeSpeed => m_RechargeSpeed;

	public float lastUsedTime { get; set; }

	public float rechargeDelay => m_RechargeDelay;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Equip.SetJetPackLogic(this);
		m_ItemAttr = itemObj.GetCmpt<JetPkg>();
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = 3.0;
		m_LastNetValue = enCurrent;
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		m_Equip.SetJetPackLogic(null);
	}

	private void Update()
	{
		if (GameConfig.IsMultiMode && m_Time != null)
		{
			m_Time.Update(Time.deltaTime);
			if (m_Time.Second < 0.0 && Mathf.Abs(m_LastNetValue - enCurrent) > 3f)
			{
				m_Time.Second = 3.0;
				m_LastNetValue = enCurrent;
				PlayerNetwork.mainPlayer.RequestJetPackEnergyReload(m_Entity.Id, m_ItemObj.instanceId, m_LastNetValue);
			}
		}
	}
}
