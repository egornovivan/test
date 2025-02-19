using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTSneakAttack), "SneakAttack")]
public class BTSneakAttack : BTNormal
{
	private class Data
	{
		[Behave]
		public float enterRadius;

		[Behave]
		public float exitMinRadius;

		[Behave]
		public float exitMaxRadius;

		[Behave]
		public float moveSpeed;
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
		if (base.attackEnemy.ThreatDamage > 0f)
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.DistanceXZ < m_Data.enterRadius)
		{
			return BehaveResult.Failure;
		}
		SetBool("Snake", value: true);
		SetSpeed(m_Data.moveSpeed);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.ThreatDamage > 0f)
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.DistanceXZ < m_Data.exitMinRadius)
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.DistanceXZ > m_Data.exitMaxRadius)
		{
			return BehaveResult.Failure;
		}
		MoveToPosition(base.attackEnemy.position);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && GetBool("Snake"))
		{
			SetBool("Snake", value: false);
			SetSpeed(0f);
		}
	}
}
