using ItemAsset;
using Pathea;
using Pathea.Effect;
using UnityEngine;

public class PEBodyFitEnergySheild : PEEnergySheildLogic
{
	[SerializeField]
	private PEDefenceTrigger m_SubDefenceTrigger;

	[SerializeField]
	private int m_StartEffectID;

	[SerializeField]
	private float m_StartEffectDelayTime;

	[SerializeField]
	private int m_EndEffectID;

	[SerializeField]
	private float m_EndEffectDelayTime;

	private ControllableEffect m_StartEffect;

	private BiologyViewCmpt m_View;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_View = m_Entity.biologyViewCmpt;
		SyncDefenceTrigger();
	}

	protected override void Update()
	{
		base.Update();
		UpdateEffectState();
		if (m_Active && enCurrent < float.Epsilon)
		{
			DeactiveSheild();
		}
		if (m_Entity.IsDeath())
		{
			DeactiveSheild();
		}
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		if (null != m_View && null != m_View.defenceTrigger)
		{
			m_View.defenceTrigger.active = true;
		}
	}

	public override void OnModelRebuild()
	{
		SyncDefenceTrigger();
		SetDefenceState(m_Active);
	}

	public override void ActiveSheild(bool fullCharge = false)
	{
		base.ActiveSheild(fullCharge);
		SetDefenceState(active: true);
		Invoke("PlayStartEffect", m_StartEffectDelayTime);
	}

	public override void DeactiveSheild()
	{
		base.DeactiveSheild();
		SetDefenceState(active: false);
		if (IsInvoking("PlayStartEffect"))
		{
			CancelInvoke("PlayStartEffect");
		}
		if (m_StartEffect != null)
		{
			m_StartEffect.Destory();
			m_StartEffect = null;
		}
		Invoke("PlayEndEffect", m_EndEffectDelayTime);
	}

	private void SyncDefenceTrigger()
	{
		if (null != m_SubDefenceTrigger && null != m_View && null != m_View.defenceTrigger)
		{
			m_SubDefenceTrigger.SyncBone(m_View.defenceTrigger);
		}
	}

	private void SetDefenceState(bool active)
	{
		if (null != m_SubDefenceTrigger)
		{
			m_SubDefenceTrigger.active = active;
			if (null != m_View && null != m_View.defenceTrigger)
			{
				m_View.defenceTrigger.active = !active;
			}
		}
	}

	private void UpdateEffectState()
	{
		if (m_StartEffect != null && null != m_View && null != m_View.modelTrans)
		{
			m_StartEffect.active = m_View.modelTrans.gameObject.activeSelf;
		}
	}

	private void OnDestroy()
	{
		if (m_StartEffect != null)
		{
			m_StartEffect.Destory();
			m_StartEffect = null;
		}
	}

	private void PlayStartEffect()
	{
		if (null != m_View && null != m_View.modelTrans && m_StartEffectID != 0)
		{
			m_StartEffect = new ControllableEffect(m_StartEffectID, m_View.modelTrans);
		}
	}

	private void PlayEndEffect()
	{
		if (null != m_View && null != m_View.modelTrans && m_EndEffectID != 0)
		{
			Singleton<EffectBuilder>.Instance.Register(m_EndEffectID, null, m_View.modelTrans);
		}
	}
}
