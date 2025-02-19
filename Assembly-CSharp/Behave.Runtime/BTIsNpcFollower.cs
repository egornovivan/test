namespace Behave.Runtime;

[BehaveAction(typeof(BTIsNpcFollower), "IsNpcFollower")]
public class BTIsNpcFollower : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.IsNpcFollower)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
