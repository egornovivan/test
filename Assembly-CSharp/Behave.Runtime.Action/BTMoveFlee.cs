using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveFlee), "MoveFlee")]
public class BTMoveFlee : BTNormal
{
	private class Data
	{
		[Behave]
		public float time;

		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float forwardSpeed;

		[Behave]
		public float backSpeed;

		public float m_StartTime;

		public float m_LastCooldownTime;
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
		if (Time.time - m_Data.m_LastCooldownTime <= m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartTime = Time.time;
		Vector3 vector = base.attackEnemy.position - base.position;
		vector = Vector3.ProjectOnPlane(vector, base.transform.up);
		if (Vector3.Dot(base.transform.forward, vector.normalized) > 0f)
		{
			SetSpeed(m_Data.backSpeed);
			MoveDirection(base.position - base.attackEnemy.position);
		}
		else
		{
			SetSpeed(m_Data.forwardSpeed);
			MoveDirection(base.attackEnemy.position - base.position);
		}
		FaceDirection(base.attackEnemy.position - base.position);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime < m_Data.time)
		{
			return BehaveResult.Running;
		}
		m_Data.m_LastCooldownTime = Time.time;
		MoveDirection(Vector3.zero);
		FaceDirection(Vector3.zero);
		return BehaveResult.Success;
	}

	private void Reset(Tree sender)
	{
		MoveDirection(Vector3.zero);
		FaceDirection(Vector3.zero);
	}
}
