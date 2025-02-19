namespace Behave.Runtime;

[BehaveAction(typeof(BTStopMove), "StopMove")]
public class BTStopMove : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		StopMove();
		return BehaveResult.Success;
	}
}
