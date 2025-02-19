using SkillSystem;
using UnityEngine;

namespace Pathea.Projectile;

public class SkProjectileEX : SkProjectile
{
	[SerializeField]
	private int m_EmitProjectileID;

	[SerializeField]
	private bool m_EmitOnEnd;

	protected override void Hit(Vector3 pos, Vector3 normal, Transform hitTrans)
	{
		EmitProjectile(hitTrans);
		base.Hit(pos, normal, hitTrans);
	}

	protected override void Hit(PECapsuleHitResult hitResult, SkEntity skEntity = null)
	{
		EmitProjectile(hitResult.hitTrans);
		base.Hit(hitResult, skEntity);
	}

	protected override void OnLifeTimeEnd()
	{
		if (m_EmitOnEnd)
		{
			EmitProjectile();
		}
		base.OnLifeTimeEnd();
	}

	private void EmitProjectile(Transform hitTrans = null)
	{
		Transform casterTrans = GetCasterTrans();
		if (null == casterTrans)
		{
			Debug.LogWarning("Can't find caster");
		}
		else if (null != hitTrans)
		{
			Singleton<ProjectileBuilder>.Instance.Register(m_EmitProjectileID, casterTrans, base.transform, hitTrans);
		}
		else if (m_TargetPosition != Vector3.zero)
		{
			Singleton<ProjectileBuilder>.Instance.Register(m_EmitProjectileID, casterTrans, base.transform, m_TargetPosition);
		}
		else if (null != m_Target)
		{
			Singleton<ProjectileBuilder>.Instance.Register(m_EmitProjectileID, casterTrans, base.transform, m_Target);
		}
	}
}
