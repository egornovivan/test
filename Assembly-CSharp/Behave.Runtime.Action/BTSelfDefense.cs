namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSelfDefense), "SelfDefense")]
public class BTSelfDefense : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Failure;
	}
}
