namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsField), "IsField")]
public class BTIsField : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcFollower && !base.IsNpcBase && !base.IsNpcCampsite && !base.hasAnyRequest)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
