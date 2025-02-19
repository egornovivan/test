namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsAttack), "IsAttack")]
public class BTIsAttack : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
