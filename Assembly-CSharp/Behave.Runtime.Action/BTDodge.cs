using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTDodge), "Dodge")]
public class BTDodge : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null || base.attackEnemy.entityTarget == null || base.attackEnemy.entityTarget.target == null)
		{
			return BehaveResult.Failure;
		}
		Enemy enemy = base.attackEnemy.entityTarget.target.GetAttackEnemy();
		if (enemy == null)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Dodge))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.SqrDistanceXZ < 1f && base.attackEnemy.entityTarget.IsAttacking && enemy.entityTarget == base.entity)
		{
			float num = ((!(Random.value > 0.5f)) ? 1f : (-1f));
			Vector3 vec = ((!(Random.value > 0.5f)) ? (-base.transform.forward) : (num * base.transform.right));
			if (Random.value >= 0.3f && base.attackEnemy.entityTarget.IsAttacking)
			{
				PEActionParamV param = PEActionParamV.param;
				param.vec = vec;
				DoAction(PEActionType.Step, param);
			}
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
