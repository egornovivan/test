using ItemAsset;
using Pathea;
using UnityEngine;

public class PECrusher : PEDigTool, IHeavyEquipment
{
	private const string AnimName = "Running";

	[SerializeField]
	private Animation m_Anim;

	[SerializeField]
	private float m_EnergyCostSpeed = 5f;

	private EquipmentActiveEffect m_Effect;

	private bool m_Active;

	public MoveStyle baseMoveStyle => m_HandChangeAttr.m_BaseMoveStyle;

	public override bool canHoldEquipment
	{
		get
		{
			if (null == m_Entity || m_Entity.GetAttribute(AttribType.Energy) < m_EnergyCostSpeed)
			{
				return false;
			}
			return base.canHoldEquipment;
		}
	}

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_Effect = GetComponent<EquipmentActiveEffect>();
		if (null != m_Anim)
		{
			m_Anim.Stop();
		}
		if (null != m_Indicator)
		{
			m_Indicator.m_AddHeight = 1;
		}
	}

	public override void SetActiveState(bool active)
	{
		base.SetActiveState(active);
		m_Active = active;
		if (null != m_Effect)
		{
			m_Effect.SetActiveState(active);
		}
		if (active)
		{
			if (null != m_Anim && !m_Anim.IsPlaying("Running"))
			{
				m_Anim.CrossFade("Running");
			}
		}
		else if (null != m_Anim)
		{
			m_Anim.Stop("Running");
		}
	}

	public void UpdateEnCost()
	{
		if (!(null == m_Entity) && m_Active)
		{
			float attribute = m_Entity.GetAttribute(AttribType.Energy);
			attribute -= Time.deltaTime * m_EnergyCostSpeed;
			if (attribute <= 0f)
			{
				attribute = 0f;
				EndAction();
			}
			m_Entity.SetAttribute(AttribType.Energy, attribute);
		}
	}

	private void EndAction()
	{
		if (!(null == m_Entity) && !(null == m_Entity.motionMgr))
		{
			m_Entity.motionMgr.EndImmediately(PEActionType.Dig);
			m_Entity.motionMgr.EndAction(m_HandChangeAttr.m_ActiveActionType);
		}
	}

	virtual void IHeavyEquipment.HidEquipmentByUnderWater(bool hide)
	{
		HidEquipmentByUnderWater(hide);
	}
}
