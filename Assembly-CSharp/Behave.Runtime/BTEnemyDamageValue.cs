using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTEnemyDamageValue), "EnemyDamageValue")]
public class BTEnemyDamageValue : BTNormal
{
	private class Data
	{
		[Behave]
		public float DamageValuePer;
	}

	private Data m_Data;

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
		return BehaveResult.Failure;
	}
}
