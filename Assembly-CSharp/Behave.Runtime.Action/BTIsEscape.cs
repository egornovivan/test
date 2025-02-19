namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsEscape), "IsEscape")]
public class BTIsEscape : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.escapeEnemy != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
