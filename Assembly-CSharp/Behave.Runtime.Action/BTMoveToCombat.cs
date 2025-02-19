using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveToCombat), "MoveToCombat")]
public class BTMoveToCombat : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;
	}

	private Data m_Data;

	private Vector3 m_Local;

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
		float num = PEUtil.MagnitudeH(base.position, base.attackEnemy.position) - base.radius - base.attackEnemy.radius;
		if (num <= m_Data.maxRange)
		{
			return BehaveResult.Success;
		}
		m_Local = PEUtil.GetRandomPosition(Vector3.zero, base.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, -75f, 75f);
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
		if (Stucking(5f))
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = base.attackEnemy.position + m_Local;
		if (PEUtil.MagnitudeH(base.position, vector) < 1f)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Success;
		}
		MoveToPosition(vector, SpeedState.Run);
		return BehaveResult.Success;
	}
}
