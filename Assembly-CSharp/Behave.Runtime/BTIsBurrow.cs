namespace Behave.Runtime;

[BehaveAction(typeof(BTIsBurrow), "IsBurrow")]
public class BTIsBurrow : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (GetBool("Burrow"))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
