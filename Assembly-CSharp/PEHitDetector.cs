using System;
using Pathea;
using Pathea.Projectile;
using SkillSystem;
using UnityEngine;

public class PEHitDetector : MonoBehaviour
{
	protected SkEntity m_SkEntity;

	private void Start()
	{
		m_SkEntity = GetComponentInParent<SkEntity>();
		SkProjectile.onHitSkEntity = (Action<SkEntity>)Delegate.Combine(SkProjectile.onHitSkEntity, new Action<SkEntity>(OnHitEntity));
		PEAttackTrigger.onHitSkEntity = (Action<SkEntity>)Delegate.Combine(PEAttackTrigger.onHitSkEntity, new Action<SkEntity>(OnHitEntity));
	}

	private void OnDestroy()
	{
		SkProjectile.onHitSkEntity = (Action<SkEntity>)Delegate.Remove(SkProjectile.onHitSkEntity, new Action<SkEntity>(OnHitEntity));
		PEAttackTrigger.onHitSkEntity = (Action<SkEntity>)Delegate.Remove(PEAttackTrigger.onHitSkEntity, new Action<SkEntity>(OnHitEntity));
	}

	private void OnHitEntity(SkEntity entity)
	{
		if (null != entity && entity == m_SkEntity)
		{
			OnHit();
		}
	}

	protected virtual void OnHit()
	{
	}
}
