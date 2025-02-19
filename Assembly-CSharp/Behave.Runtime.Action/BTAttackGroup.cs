namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackGroup), "AttackGroup")]
public class BTAttackGroup : BTNormalGroup
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
			behaveGroup.PauseMemberBehave(value: false);
			return BehaveResult.Running;
		}
		return BehaveResult.Failure;
	}
}
