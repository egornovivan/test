using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTAttackEnemyIsAttacking), "AttackEnemyIsAttacking")]
public class BTAttackEnemyIsAttacking : BTNormal
{
	private bool IsInEnemyFoward(Enemy enemy, PeEntity self)
	{
		Vector3 vector = self.position;
		Vector3 vector2 = enemy.position;
		Vector3 forward = enemy.entityTarget.peTrans.forward;
		Vector3 normalized = (vector - vector2).normalized;
		float num = Mathf.Abs(PEUtil.Angle(forward, normalized));
		return num <= 90f;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.selectattackEnemy.entityTarget == null || base.selectattackEnemy.entityTarget.target == null)
		{
			return BehaveResult.Failure;
		}
		if (base.selectattackEnemy.entityTarget.target != null)
		{
			Enemy enemy = base.selectattackEnemy.entityTarget.target.GetAttackEnemy();
			if (enemy == null)
			{
				return BehaveResult.Failure;
			}
			if (base.selectattackEnemy.entityTarget.IsAttacking && enemy.entityTarget == base.entity && IsInEnemyFoward(base.selectattackEnemy, base.entity))
			{
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Failure;
	}
}
