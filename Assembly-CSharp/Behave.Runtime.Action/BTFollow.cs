using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTFollow), "Follow")]
public class BTFollow : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		public float m_Time;

		public float m_StartTime;

		public Vector3 m_FollowPosition;
	}

	private Data m_Data;

	private Vector3 GetFollowPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius, float minAngle, float maxAngle)
	{
		if (base.field == MovementField.Sky)
		{
			if (IsFly())
			{
				return PEUtil.GetRandomPositionInSky(center, direction, minRadius, maxRadius, 10f, 25f, minAngle, maxAngle);
			}
			return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, isResult: false);
		}
		if (base.field == MovementField.water)
		{
			return PEUtil.GetRandomPositionInWater(center, direction, minRadius, maxRadius, 5f, 25f, minAngle, maxAngle, isResult: false);
		}
		return PEUtil.GetRandomPositionOnGround(center, direction, minRadius, maxRadius, minAngle, maxAngle, isResult: false);
	}

	private Vector3 GetFollowPosition(Vector3 center, Vector3 direction, float minRadius, float maxRadius)
	{
		for (int i = 0; i < 5; i++)
		{
			Vector3 followPosition = GetFollowPosition(center, direction, minRadius, maxRadius, -75f, 75f);
			if (followPosition != Vector3.zero && EvadePolarShield(followPosition))
			{
				return followPosition;
			}
		}
		for (int j = 0; j < 5; j++)
		{
			Vector3 followPosition2 = GetFollowPosition(center, direction, minRadius, maxRadius, -135f, 135f);
			if (followPosition2 != Vector3.zero && EvadePolarShield(followPosition2))
			{
				return followPosition2;
			}
		}
		for (int k = 0; k < 5; k++)
		{
			Vector3 zero = Vector3.zero;
			zero = ((base.field != MovementField.Land && (base.field != MovementField.Sky || IsFly())) ? PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75f, 75f, -5f, 5f) : PEUtil.GetRandomPosition(center, direction, minRadius, maxRadius, -75f, 75f));
			if (zero != Vector3.zero && EvadePolarShield(zero))
			{
				return zero;
			}
		}
		return GetEvadePolarShieldPosition(base.position);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || !Enemy.IsNullOrInvalid(base.escapeEnemy) || Enemy.IsNullOrInvalid(base.followEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_FollowPosition = Vector3.zero;
		m_Data.m_StartTime = Time.time;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || !Enemy.IsNullOrInvalid(base.escapeEnemy) || Enemy.IsNullOrInvalid(base.followEnemy))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
		{
			return BehaveResult.Success;
		}
		float num = base.entity.maxRadius + base.followEnemy.entityTarget.maxRadius;
		if (PEUtil.SqrMagnitudeH(base.position, m_Data.m_FollowPosition) < 1f)
		{
			m_Data.m_FollowPosition = Vector3.zero;
		}
		if (m_Data.m_FollowPosition == Vector3.zero)
		{
			m_Data.m_FollowPosition = GetFollowPosition(base.followEnemy.position, base.position - base.followEnemy.position, 5f + num, 10f + num);
		}
		if (m_Data.m_FollowPosition == Vector3.zero)
		{
			m_Data.m_FollowPosition = GetFollowPosition(base.position, base.followEnemy.position - base.position, 5f + num, 10f + num);
		}
		if (m_Data.m_FollowPosition == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		MoveToPosition(m_Data.m_FollowPosition, SpeedState.Run);
		return BehaveResult.Running;
	}
}
