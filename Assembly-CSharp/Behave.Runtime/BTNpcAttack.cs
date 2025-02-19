using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcAttack), "NpcAttack")]
public class BTNpcAttack : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy) || base.selectattackEnemy.GroupAttack == EAttackGroup.Threat)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
