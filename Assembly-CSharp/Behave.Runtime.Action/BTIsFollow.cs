namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsFollow), "IsFollow")]
public class BTIsFollow : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy != null || base.escapeEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (base.followEnemy != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
