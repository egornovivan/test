namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTHasTarget), "HasTarget")]
public class BTHasTarget : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (!base.hasAttackEnemy)
		{
			SetBool("Combat", value: false);
			return BehaveResult.Failure;
		}
		SetBool("Combat", value: true);
		return BehaveResult.Success;
	}
}
