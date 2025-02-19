using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackCheck), "AttackCheck")]
public class BTAttackCheck : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.entityTarget == null || base.attackEnemy.entityTarget.animCmpt == null || base.attackEnemy.entityTarget.target == null)
		{
			return BehaveResult.Success;
		}
		Vector3 vec = base.position - base.attackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		if (!CanDoAction(PEActionType.Step, param))
		{
			return BehaveResult.Success;
		}
		if (base.Weapon != null && base.Weapon.GetAttackMode() != null)
		{
			AttackMode[] attackMode = base.Weapon.GetAttackMode();
			if (attackMode.Length > 0 && attackMode[0].type == AttackType.Ranged)
			{
				return BehaveResult.Success;
			}
		}
		Enemy enemy = base.attackEnemy.entityTarget.target.GetAttackEnemy();
		if (enemy == null)
		{
			return BehaveResult.Success;
		}
		EAttackCheck integer = (EAttackCheck)base.attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
		if (enemy.entityTarget == base.entity && integer != 0)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
