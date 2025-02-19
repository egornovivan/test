namespace Behave.Runtime;

[BehaveAction(typeof(BTHasTreat), "HasTreat")]
public class BTHasTreat : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.Treat != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
