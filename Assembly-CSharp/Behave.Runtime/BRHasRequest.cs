namespace Behave.Runtime;

[BehaveAction(typeof(BRHasRequest), "RHasRequest")]
public class BRHasRequest : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.hasAnyRequest)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
