namespace Behave.Runtime;

[BehaveAction(typeof(BTFailure), "Failure")]
public class BTFailure : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Failure;
	}
}
