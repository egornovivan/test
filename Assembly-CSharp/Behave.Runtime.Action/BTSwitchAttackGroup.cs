namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSwitchAttackGroup), "SwitchAttackGroup")]
public class BTSwitchAttackGroup : BTNormalGroup
{
	private float m_LastCheckTime;

	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Failure;
	}
}
