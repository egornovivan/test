namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsThreat), "IsThreat")]
public class BTIsThreat : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy != null || base.escapeEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (base.threatEnemy != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
