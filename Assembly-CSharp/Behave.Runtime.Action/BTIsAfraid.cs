namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsAfraid), "IsAfraid")]
public class BTIsAfraid : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy != null || base.escapeEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (base.afraidEnemy != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
