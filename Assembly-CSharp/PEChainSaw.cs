using ItemAsset;
using Pathea;
using UnityEngine;

public class PEChainSaw : PEAxe, IHeavyEquipment
{
	[SerializeField]
	private Renderer m_Renderer;

	[SerializeField]
	private float m_TexSpeed = 3f;

	[SerializeField]
	private float m_EnergyCostSpeed = 5f;

	private bool m_Active;

	private Vector2 m_TexOffset = Vector2.zero;

	private EquipmentActiveEffect m_Effect;

	public MoveStyle baseMoveStyle => m_HandChangeAttr.m_BaseMoveStyle;

	public string activeAnim => m_HandChangeAttr.m_PutOnAnim;

	public string deactiveAnim => m_HandChangeAttr.m_PutOffAnim;

	public PEActionMask mask => m_HandChangeAttr.m_HoldActionMask;

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
	}

	public override void SetActiveState(bool active)
	{
		m_Active = active;
		if (null != m_Effect)
		{
			m_Effect.SetActiveState(active);
		}
	}

	private void Update()
	{
		if (m_Active)
		{
			UpdateChainUV();
			UpdateEnCost();
		}
	}

	private void UpdateChainUV()
	{
		m_TexOffset += m_TexSpeed * Time.deltaTime * Vector2.right;
		m_TexOffset.x %= 1f;
		m_Renderer.materials[1].SetTextureOffset("_MainTex", m_TexOffset);
	}

	private void UpdateEnCost()
	{
		if (null != m_Entity)
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
		if (!(null == m_Entity.motionMgr))
		{
			m_Entity.motionMgr.EndImmediately(PEActionType.Fell);
			m_Entity.motionMgr.EndAction(m_HandChangeAttr.m_ActiveActionType);
		}
	}

	virtual void IHeavyEquipment.HidEquipmentByUnderWater(bool hide)
	{
		HidEquipmentByUnderWater(hide);
	}
}
