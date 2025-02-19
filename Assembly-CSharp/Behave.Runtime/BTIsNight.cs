namespace Behave.Runtime;

[BehaveAction(typeof(BTIsNight), "IsNight")]
public class BTIsNight : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (GameConfig.IsNight)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
