using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTElude), "Elude")]
public class BTElude : BTNormal
{
	private EludePoint m_Point;

	private BehaveResult Init(Tree sender)
	{
		m_Point = PEEludePoint.GetEludePoint(base.transform.position, base.attackEnemy.position);
		if (m_Point == null)
		{
			if (!Enemy.IsNullOrInvalid(base.attackEnemy))
			{
				SetEscapeEntity(base.attackEnemy.entityTarget);
			}
			return BehaveResult.Failure;
		}
		m_Point.Dirty = true;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!m_Point.CanElude(base.attackEnemy.position))
		{
			return BehaveResult.Failure;
		}
		if (!m_Point.Elude(base.position))
		{
			MoveToPosition(m_Point.Position, SpeedState.Run);
		}
		else
		{
			MoveToPosition(Vector3.zero);
			FaceDirection(m_Point.FaceDirection);
			Vector3 from = Vector3.ProjectOnPlane(base.transform.forward, Vector3.up);
			Vector3 to = Vector3.ProjectOnPlane(m_Point.FaceDirection, Vector3.up);
			if (!GetBool("Elude") && Vector3.Angle(from, to) < 5f)
			{
				SetBool("Elude", value: true);
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Point != null)
		{
			SetBool("Elude", value: false);
			FaceDirection(Vector3.zero);
			m_Point.Dirty = false;
			m_Point = null;
		}
	}
}
