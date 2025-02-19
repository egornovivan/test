using Pathea;
using Pathea.Projectile;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsTargetMelee), "IsTargetMelee")]
public class BTIsTargetMelee : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		for (int i = 0; i < SkProjectile.s_Projectiles.Count; i++)
		{
			SkProjectile skProjectile = SkProjectile.s_Projectiles[i];
			if (skProjectile == null)
			{
				continue;
			}
			if (skProjectile.m_Target != null && skProjectile.m_Target.IsChildOf(base.entity.transform))
			{
				return BehaveResult.Success;
			}
			if (skProjectile.m_TargetPosition != Vector3.zero)
			{
				Vector3 direction = skProjectile.m_TargetPosition - skProjectile.transform.position;
				Vector3 origin = base.entity.tr.InverseTransformPoint(skProjectile.transform.position);
				Vector3 direction2 = base.entity.tr.InverseTransformDirection(direction);
				Ray ray = new Ray(origin, direction2);
				if (base.entity.bounds.IntersectRay(ray))
				{
					return BehaveResult.Success;
				}
			}
			if (!(base.entity.biologyViewCmpt != null) || !(base.entity.biologyViewCmpt.monoModelCtrlr != null))
			{
				continue;
			}
			Bounds colliderBounds = base.entity.biologyViewCmpt.monoModelCtrlr.ColliderBounds;
			Bounds triggerBounds = skProjectile.TriggerBounds;
			if (colliderBounds.size != Vector3.zero && triggerBounds.size != Vector3.zero)
			{
				triggerBounds.Encapsulate(Vector3.one * 2f);
				if (colliderBounds.Intersects(triggerBounds))
				{
					return BehaveResult.Success;
				}
			}
		}
		if (base.attackEnemy.entityTarget.motionMgr != null && base.attackEnemy.entityTarget.motionMgr.IsActionRunning(PEActionType.SwordAttack) && base.attackEnemy.SqrDistanceXZ < 9f)
		{
			return BehaveResult.Success;
		}
		if (base.attackEnemy.entityTarget.IsAttacking && base.attackEnemy.SqrDistanceXZ < 9f)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
