namespace Behave.Runtime;

[BehaveAction(typeof(BTIsFly), "IsFly")]
public class BTIsFly : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (IsFly())
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
