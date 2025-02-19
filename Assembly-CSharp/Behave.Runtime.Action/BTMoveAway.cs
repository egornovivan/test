using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveAway), "MoveAway")]
public class BTMoveAway : BTNormal
{
	private class Data
	{
		[Behave]
		public float cdTime = 5f;

		[Behave]
		public float prob;

		[Behave]
		public float angle;

		[Behave]
		public float distance;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		public float m_Time;
	}

	private float m_StartTime;

	private float m_LastTime;

	private Vector3 m_Direction;

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
		if (Time.time - m_LastTime < m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (Vector3.Angle(base.attackEnemy.Direction, base.transform.forward) < m_Data.angle || base.attackEnemy.SqrDistanceXZ > m_Data.distance * m_Data.distance)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_StartTime = Time.time;
		m_LastTime = Time.time;
		m_Direction = base.transform.forward;
		m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
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
		if (Time.time - m_StartTime < m_Data.m_Time)
		{
			MoveDirection(m_Direction, SpeedState.Run);
			FaceDirection(m_Direction);
			return BehaveResult.Running;
		}
		MoveDirection(Vector3.zero);
		FaceDirection(Vector3.zero);
		return BehaveResult.Success;
	}
}
