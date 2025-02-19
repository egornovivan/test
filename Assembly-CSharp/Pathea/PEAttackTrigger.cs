using System;
using System.Collections.Generic;
using PETools;
using SkillSystem;
using UnityEngine;

namespace Pathea;

public class PEAttackTrigger : MonoBehaviour
{
	[Serializable]
	public class PEAttackPart
	{
		public AttackForm attackForm;

		public PECapsuleTrigger capsule;

		public void Init()
		{
			capsule.ResetInfo();
		}

		public void Update(Vector3 centerPos)
		{
			capsule.Update(centerPos);
		}
	}

	public static Action<SkEntity> onHitSkEntity;

	public LayerMask attackLayer;

	public PEAttackPart[] attackParts;

	private List<PEDefenceTrigger> m_HitTriggers = new List<PEDefenceTrigger>();

	private List<PEDefenceTrigger> m_EffectedTriggers = new List<PEDefenceTrigger>();

	private List<SkEntity> m_HitEntitys = new List<SkEntity>();

	private bool m_Active = true;

	private SkEntity m_SkEntity;

	private PeEntity m_PEEntity;

	public bool active
	{
		get
		{
			return m_Active;
		}
		set
		{
			if (m_Active != value)
			{
				m_Active = value;
				Collider component = GetComponent<Collider>();
				if (null != component)
				{
					component.enabled = m_Active;
				}
				UpdatePartsInfo();
			}
		}
	}

	public event Action<PEDefenceTrigger, PECapsuleHitResult> onHitTrigger;

	public void ClearHitInfo()
	{
		m_HitTriggers.Clear();
		m_EffectedTriggers.Clear();
		m_HitEntitys.Clear();
	}

	public void ResetHitInfo()
	{
		for (int i = 0; i < m_EffectedTriggers.Count; i++)
		{
			m_HitTriggers.Add(m_EffectedTriggers[i]);
		}
		m_EffectedTriggers.Clear();
		m_HitEntitys.Clear();
	}

	private void Reset()
	{
		attackLayer = LayerMask.GetMask("Damage", "GIEProductLayer");
	}

	private void Start()
	{
		for (int i = 0; i < attackParts.Length; i++)
		{
			attackParts[i].Init();
		}
		m_SkEntity = GetComponentInParent<SkEntity>();
		if (null != m_SkEntity)
		{
			m_PEEntity = m_SkEntity.GetComponent<PeEntity>();
		}
		active = false;
	}

	private void LateUpdate()
	{
		try
		{
			UpdatePartsInfo();
			UpdateHitState();
		}
		catch (Exception ex)
		{
			if (null != m_PEEntity.animCmpt && null != m_PEEntity.animCmpt.animator)
			{
				Animator animator = m_PEEntity.animCmpt.animator;
				string text = "PlayingAnim:";
				List<AnimatorClipInfo> list = new List<AnimatorClipInfo>();
				for (int i = 0; i < animator.layerCount; i++)
				{
					list.AddRange(animator.GetCurrentAnimatorClipInfo(i));
				}
				for (int j = 0; j < list.Count; j++)
				{
					text = text + list[j].clip.name + "\n";
				}
				Debug.LogError(text + ex.ToString());
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((attackLayer.value & (1 << other.gameObject.layer)) == 0)
		{
			return;
		}
		SkEntity componentInParent = other.gameObject.GetComponentInParent<SkEntity>();
		if (null == componentInParent || componentInParent == m_SkEntity)
		{
			return;
		}
		PEDefenceTrigger component = other.GetComponent<PEDefenceTrigger>();
		if (null != component)
		{
			if (!m_HitTriggers.Contains(component) && !m_EffectedTriggers.Contains(component))
			{
				if (CheckHit(component))
				{
					m_EffectedTriggers.Add(component);
				}
				else
				{
					m_HitTriggers.Add(component);
				}
			}
		}
		else
		{
			OnHitCollider(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if ((attackLayer.value & (1 << other.gameObject.layer)) != 0)
		{
			PEDefenceTrigger component = other.GetComponent<PEDefenceTrigger>();
			if (null != component)
			{
				m_HitTriggers.Remove(component);
			}
		}
	}

	private void UpdatePartsInfo()
	{
		if (m_Active)
		{
			Vector3 centerPos = ((!(null != m_PEEntity)) ? Vector3.zero : m_PEEntity.centerPos);
			for (int i = 0; i < attackParts.Length; i++)
			{
				attackParts[i].Update(centerPos);
			}
		}
	}

	private void UpdateHitState()
	{
		if (!active)
		{
			return;
		}
		for (int num = m_HitTriggers.Count - 1; num >= 0; num--)
		{
			if (null == m_HitTriggers[num])
			{
				m_HitTriggers.RemoveAt(num);
			}
			else if (CheckHit(m_HitTriggers[num]))
			{
				m_EffectedTriggers.Add(m_HitTriggers[num]);
				m_HitTriggers.RemoveAt(num);
			}
		}
	}

	private bool CheckHit(PEDefenceTrigger defenceTrigger)
	{
		defenceTrigger.UpdateInfo();
		PECapsuleHitResult pECapsuleHitResult = null;
		for (int i = 0; i < attackParts.Length; i++)
		{
			if (!attackParts[i].capsule.enable)
			{
				continue;
			}
			for (int j = 0; j < defenceTrigger.defenceParts.Length; j++)
			{
				if (defenceTrigger.defenceParts[j].capsule.enable && attackParts[i].capsule.CheckCollision(defenceTrigger.defenceParts[j].capsule, out var result))
				{
					result.selfAttackForm = attackParts[i].attackForm;
					result.hitDefenceType = defenceTrigger.defenceParts[j].defenceType;
					result.damageScale = defenceTrigger.defenceParts[j].damageScale;
					if (pECapsuleHitResult == null || pECapsuleHitResult.distance < result.distance)
					{
						pECapsuleHitResult = result;
					}
				}
			}
		}
		if (pECapsuleHitResult != null)
		{
			OnHit(defenceTrigger, pECapsuleHitResult);
			return true;
		}
		return false;
	}

	private void OnHitCollider(Collider other)
	{
		if (null == m_SkEntity || (null != m_PEEntity && m_PEEntity.IsDeath()))
		{
			return;
		}
		SkEntity componentInParent = other.gameObject.GetComponentInParent<SkEntity>();
		bool flag = PEUtil.IsVoxelOrBlock45(componentInParent);
		if (null == componentInParent || m_HitEntitys.Contains(componentInParent) || (!flag && !PEUtil.CanDamage(m_SkEntity, componentInParent)))
		{
			return;
		}
		m_HitEntitys.Add(componentInParent);
		PECapsuleHitResult pECapsuleHitResult = null;
		bool flag2 = false;
		float num = 100f;
		for (int i = 0; i < attackParts.Length; i++)
		{
			if (attackParts[i].capsule.enable)
			{
				if (attackParts[i].capsule.GetClosestPos(other.transform.position, out var result))
				{
					pECapsuleHitResult = result;
					pECapsuleHitResult.selfAttackForm = attackParts[i].attackForm;
					pECapsuleHitResult.hitTrans = other.transform;
					pECapsuleHitResult.damageScale = 1f;
					return;
				}
				if (result.distance < num)
				{
					num = result.distance;
					pECapsuleHitResult = result;
					pECapsuleHitResult.hitTrans = other.transform;
					pECapsuleHitResult.selfAttackForm = attackParts[i].attackForm;
					pECapsuleHitResult.damageScale = 1f;
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			m_SkEntity.CollisionCheck(pECapsuleHitResult);
			if (onHitSkEntity != null)
			{
				onHitSkEntity(componentInParent);
			}
		}
	}

	private void OnHit(PEDefenceTrigger defenceTrigger, PECapsuleHitResult result)
	{
		if (null != m_PEEntity && m_PEEntity.IsDeath())
		{
			return;
		}
		if (null != defenceTrigger)
		{
			SkEntity componentInParent = defenceTrigger.GetComponentInParent<SkEntity>();
			if (!PEUtil.IsVoxelOrBlock45(componentInParent) && !PEUtil.CanDamage(m_SkEntity, componentInParent))
			{
				return;
			}
			if (onHitSkEntity != null)
			{
				onHitSkEntity(componentInParent);
			}
		}
		if (this.onHitTrigger != null)
		{
			this.onHitTrigger(defenceTrigger, result);
		}
		if (null != m_SkEntity)
		{
			m_SkEntity.CollisionCheck(result);
		}
		if (null != m_PEEntity)
		{
			m_PEEntity.SendMsg(EMsg.Battle_AttackHit, result);
		}
	}
}
