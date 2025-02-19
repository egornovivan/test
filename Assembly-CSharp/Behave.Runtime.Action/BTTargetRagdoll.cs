namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTargetRagdoll), "TargetRagdoll")]
public class BTTargetRagdoll : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy != null && base.attackEnemy.isRagdoll)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
