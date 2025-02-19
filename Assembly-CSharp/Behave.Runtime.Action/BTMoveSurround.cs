using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveSurround), "MoveSurround")]
public class BTMoveSurround : BTNormal
{
	private class Data
	{
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
		public float maxTime = 15f;
	}

	private Data m_Data;

	private float m_Time;

	private float m_StartTime;

	private Vector3 m_HoverPosition;

	private Vector3 GetAroundPos()
	{
		if (base.entity.Group == null)
		{
			if (base.field == MovementField.Sky)
			{
				return PEUtil.GetRandomPositionInSky(base.attackEnemy.position, base.transform.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -75f, 75f);
			}
			if (base.field == MovementField.water)
			{
				return PEUtil.GetRandomPositionInWater(base.attackEnemy.position, base.transform.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, m_Data.minHeight, m_Data.maxHeight, -75f, 75f);
			}
			return PEUtil.GetRandomPositionOnGround(base.attackEnemy.position, base.transform.position - base.attackEnemy.position, m_Data.minRange, m_Data.maxRange, -75f, 75f);
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
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		m_HoverPosition = GetAroundPos();
		m_StartTime = Time.time;
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
		if (base.attackEnemy.GroupAttack != 0)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_StartTime > m_Time)
		{
			return BehaveResult.Success;
		}
		if (m_HoverPosition == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = m_HoverPosition - base.position;
		float num = PEUtil.SqrMagnitude(base.position, m_HoverPosition, is3D: false);
		if (num < 4f || Stucking())
		{
			MoveToPosition(Vector3.zero);
			FaceDirection(base.attackEnemy.DirectionXZ);
		}
		else if (Vector3.Dot(base.transform.forward, vector.normalized) > 0f)
		{
			MoveToPosition(m_HoverPosition, SpeedState.Run);
		}
		else
		{
			MoveToPosition(m_HoverPosition, SpeedState.Retreat);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_StartTime > float.Epsilon)
		{
			m_StartTime = 0f;
			MoveDirection(Vector3.zero);
			FaceDirection(Vector3.zero);
			m_HoverPosition = Vector3.zero;
		}
	}
}
