namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTHasEscapeGroup), "HasEscapeGroup")]
public class BTHasEscapeGroup : BTNormalGroup
{
	private BehaveResult Tick(Tree sender)
	{
		BehaveGroup behaveGroup = sender.ActiveAgent as BehaveGroup;
		if (behaveGroup == null || behaveGroup.Leader == null)
		{
			return BehaveResult.Failure;
		}
		if (behaveGroup.HasEscapeEnemy())
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
