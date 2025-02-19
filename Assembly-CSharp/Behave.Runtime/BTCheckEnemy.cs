using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCheckEnemy), "CheckEnemy")]
public class BTCheckEnemy : BTNormal
{
	private class Data
	{
		[Behave]
		public int Type;
	}

	private Data m_Data;

	private bool CheckField(Enemy enemy)
	{
		int type = m_Data.Type;
		if (type == 1)
		{
			return enemy.entityTarget == null || enemy.entityTarget.Field == MovementField.Sky || enemy.entityTarget.IsBoss;
		}
		return false;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsMotorNpc)
		{
			return BehaveResult.Failure;
		}
		if (CheckField(base.selectattackEnemy))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
