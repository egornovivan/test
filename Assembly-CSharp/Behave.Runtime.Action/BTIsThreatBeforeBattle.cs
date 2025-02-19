using Pathea;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsThreatBeforeBattle), "BTIsThreatBeforeBattle")]
public class BTIsThreatBeforeBattle : BTNormal
{
	private Enemy m_Enemy;

	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null)
		{
			m_Enemy = null;
		}
		if (m_Enemy != null || base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		m_Enemy = base.attackEnemy;
		return BehaveResult.Success;
	}
}
