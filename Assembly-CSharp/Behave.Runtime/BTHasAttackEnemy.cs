using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTHasAttackEnemy), "hasAttackEnemy")]
public class BTHasAttackEnemy : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
