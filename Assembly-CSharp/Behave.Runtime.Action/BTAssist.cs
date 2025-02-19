namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAssist), "Assist")]
public class BTAssist : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Failure;
	}
}
