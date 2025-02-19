namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcIsBase), "NpcIsBase")]
public class BTNpcIsBase : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.IsNpcBase)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
