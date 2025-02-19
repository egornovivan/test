namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveWeapon), "MoveWeapon")]
public class BTMoveWeapon : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Success;
	}
}
