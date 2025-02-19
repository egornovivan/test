namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTHasEnemyGroup), "HasEnemyGroup")]
public class BTHasEnemyGroup : BTNormalGroup
{
	private BehaveResult Tick(Tree sender)
	{
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (behaveGroup.HasAttackEnemy())
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
