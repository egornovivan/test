namespace Behave.Runtime;

[BehaveAction(typeof(BTSucceed), "Succeed")]
public class BTSucceed : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Success;
	}
}
