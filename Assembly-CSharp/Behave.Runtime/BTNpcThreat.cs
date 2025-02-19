using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcThreat), "NpcThreat")]
public class BTNpcThreat : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy) || base.selectattackEnemy.GroupAttack == EAttackGroup.Attack)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
