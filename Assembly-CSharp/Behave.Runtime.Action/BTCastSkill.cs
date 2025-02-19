using Pathea;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTCastSkill), "CastSkill")]
public class BTCastSkill : BTNormal
{
	private class Data
	{
		[Behave]
		public int skillID;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!IsSkillRunnable(m_Data.skillID))
		{
			return BehaveResult.Failure;
		}
		StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (IsSkillRunning(m_Data.skillID))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
