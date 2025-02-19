namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTHasEnemy), "HasEnemy")]
public class BTHasEnemy : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null && !base.entity.IsAttacking)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
