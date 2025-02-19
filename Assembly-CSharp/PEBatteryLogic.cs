using ItemAsset;
using Pathea;
using UnityEngine;

public class PEBatteryLogic : PEEquipmentLogic
{
	private Energy m_Energy;

	private float m_LastNetValue;

	private UTimer m_Time;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Energy = m_ItemObj.GetCmpt<Energy>();
		if (m_Energy != null)
		{
			m_Entity.SetAttribute(AttribType.EnergyMax, m_Energy.valueMax);
			m_Entity.SetAttribute(AttribType.Energy, m_Energy.floatValue.current);
		}
		m_LastNetValue = m_Entity.GetAttribute(AttribType.Energy);
		m_Time = new UTimer();
		m_Time.ElapseSpeed = -1f;
		m_Time.Second = 3.0;
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (m_Energy != null)
		{
			m_Energy.SetMax(m_Entity.GetAttribute(AttribType.EnergyMax));
			m_Energy.floatValue.current = m_Entity.GetAttribute(AttribType.Energy);
			m_Entity.SetAttribute(AttribType.EnergyMax, 0f);
			m_Entity.SetAttribute(AttribType.Energy, 0f);
		}
	}

	private void Update()
	{
		if (m_Energy != null)
		{
			m_Energy.floatValue.current = m_Entity.GetAttribute(AttribType.Energy);
		}
		if (GameConfig.IsMultiMode && m_Time != null)
		{
			m_Time.Update(Time.deltaTime);
			if (m_Time.Second < 0.0 && Mathf.Abs(m_LastNetValue - m_Entity.GetAttribute(AttribType.Energy)) > 3f)
			{
				m_Time.Second = 3.0;
				m_LastNetValue = m_Entity.GetAttribute(AttribType.Energy);
				PlayerNetwork.mainPlayer.RequestBatteryEnergyReload(m_Entity.Id, m_ItemObj.instanceId, m_LastNetValue);
			}
		}
	}
}
