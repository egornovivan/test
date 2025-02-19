using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveRange), "MoveRange")]
public class BTMoveRange : BTNormal
{
	private class Data
	{
		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public float angle;

		public float m_StartTime;

		public bool m_IsReady;
	}

	private Data m_Data;

	private bool IsReady(Enemy e)
	{
		return PEUtil.Magnitude(base.position, e.position, is3D: false) - base.radius - e.radius > m_Data.maxRange * 1.5f;
	}

	private Vector3 GetReadyPosition(Enemy e)
	{
		Vector3 zero = Vector3.zero;
		float num = m_Data.maxRange * 2f + base.entity.maxRadius + e.radius;
		float num2 = e.height + (m_Data.minHeight + m_Data.maxHeight) * 0.5f;
		if (base.entity.Field == MovementField.water)
		{
			num2 = e.height * 0.5f - base.entity.maxHeight * 0.5f;
		}
		Vector3 vector = base.transform.forward - base.attackEnemy.DirectionXZ;
		vector.y = 0f;
		zero = base.attackEnemy.position + vector.normalized * num + Vector3.up * num2;
		Vector3 direction = zero - base.position;
		float maxDistance = Vector3.Distance(zero, base.position);
		if (!Physics.Raycast(zero, direction, maxDistance, 2189312))
		{
			return zero;
		}
		vector = Quaternion.AngleAxis(90f, Vector3.up) * vector;
		return base.attackEnemy.position + vector.normalized * num + Vector3.up * num2;
	}

	private Vector3 GetAttackPosition(Enemy e)
	{
		Vector3 normalized = Vector3.ProjectOnPlane(base.position - base.attackEnemy.position, Vector3.up).normalized;
		Vector3 vector = normalized * ((m_Data.minRange + m_Data.maxRange) * 0.5f + base.entity.maxRadius + e.radius);
		float num = e.height + (m_Data.minHeight + m_Data.maxHeight) * 0.5f;
		if (base.entity.Field == MovementField.water)
		{
			num = e.height * 0.5f - base.entity.maxHeight * 0.5f;
		}
		return base.attackEnemy.position + vector + Vector3.up * num;
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
		m_Data.m_IsReady = false;
		m_Data.m_StartTime = Time.time;
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
		if (!m_Data.m_IsReady)
		{
			m_Data.m_IsReady = IsReady(base.attackEnemy);
		}
		if (!m_Data.m_IsReady)
		{
			MoveToPosition(GetReadyPosition(base.attackEnemy), SpeedState.Run);
		}
		else
		{
			Vector3 attackPosition = GetAttackPosition(base.attackEnemy);
			if (!(PEUtil.SqrMagnitude(base.position, attackPosition) > 1f))
			{
				MoveToPosition(Vector3.zero);
				FaceDirection(base.attackEnemy.Direction);
				Vector3 from = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up);
				Vector3 to = Vector3.ProjectOnPlane(base.attackEnemy.position - base.position, Vector3.up);
				float num = Vector3.Angle(from, to);
				if (num < m_Data.angle)
				{
					return BehaveResult.Success;
				}
				if (num < 45f)
				{
					return BehaveResult.Running;
				}
				return BehaveResult.Failure;
			}
			MoveToPosition(attackPosition, SpeedState.Run);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			m_Data.m_StartTime = 0f;
			FaceDirection(Vector3.zero);
		}
	}
}
