using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveAround), "MoveAround")]
public class BTMoveAround : BTNormal
{
	private class Data
	{
		[Behave]
		public float cdTime = 10f;

		[Behave]
		public float prob = 1f;

		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public float minTime = 10f;

		[Behave]
		public float maxTime = 10f;
	}

	private Data m_Data;

	private float m_Time;

	private float m_LastTime;

	private float m_StartTime;

	private Vector3 m_HoverPosition;

	private Vector3 GetAroundPos()
	{
		if (base.entity.Group == null)
		{
			if (base.field == MovementField.Sky)
			{
				return PEUtil.GetRandomPositionInSky(base.attackEnemy.position, base.transform.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90f, 90f);
			}
			if (base.field == MovementField.water)
			{
				return PEUtil.GetRandomPositionInWater(base.attackEnemy.position, base.transform.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -90f, 90f);
			}
			return PEUtil.GetRandomPositionOnGround(base.attackEnemy.position, base.transform.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, -90f, 90f);
		}
		return base.entity.Group.FollowEnemy(base.entity, base.entity.maxRadius + base.attackEnemy.radius + Random.Range(m_Data.minRange, m_Data.maxRange));
	}

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
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_HoverPosition = GetAroundPos();
		m_StartTime = Time.time;
		m_LastTime = Time.time;
		m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
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
		if (m_HoverPosition == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		float num = PEUtil.SqrMagnitude(base.position, m_HoverPosition);
		if (num < 1f || Stucking() || Time.time - m_StartTime > m_Time)
		{
			MoveToPosition(Vector3.zero);
			return BehaveResult.Success;
		}
		MoveToPosition(m_HoverPosition, SpeedState.Run);
		return BehaveResult.Running;
	}
}
