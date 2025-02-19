using Pathea;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsMemberSurround), "IsMemberSurround")]
public class BTIsMemberSurround : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null || base.attackEnemy.GroupAttack == EAttackGroup.Attack)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
