using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMoveAnimator), "MoveAnimator")]
public class BTMoveAnimator : BTNormal
{
	private class Data
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float startTime;

		[Behave]
		public float endTime;

		[Behave]
		public float time;

		[Behave]
		public float speed;

		[Behave]
		public int startSkill;

		[Behave]
		public int endSkill;

		[Behave]
		public Vector3 anchor = Vector3.zero;

		public float m_StartTime;

		public float m_LastCooldownTime;

		public bool m_startSkill;

		public bool m_endSkill;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_LastCooldownTime <= m_Data.cdTime)
		{
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.prob)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.motionMove != null && base.entity.motionMove is Motion_Move_Motor)
		{
			Motion_Move_Motor motion_Move_Motor = base.entity.motionMove as Motion_Move_Motor;
			if (motion_Move_Motor != null && motion_Move_Motor.motor != null)
			{
				motion_Move_Motor.motor.desiredMovementEffect = Vector3.zero;
			}
		}
		m_Data.m_StartTime = Time.time;
		m_Data.m_startSkill = false;
		m_Data.m_endSkill = false;
		SetBool(m_Data.anim, value: true);
		StopMove();
		SetSpeed(m_Data.speed);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		float num = Time.time - m_Data.m_StartTime;
		if (num > m_Data.time)
		{
			m_Data.m_LastCooldownTime = Time.time;
			return BehaveResult.Success;
		}
		if (num >= m_Data.startTime && num <= m_Data.endTime)
		{
			MoveDirection(base.transform.TransformDirection(m_Data.anchor));
			FaceDirection(base.transform.forward);
			SetSpeed(m_Data.speed);
		}
		else
		{
			MoveDirection(Vector3.zero);
			FaceDirection(Vector3.zero);
		}
		if (m_Data.startSkill > 0 && !m_Data.m_startSkill && num > m_Data.startTime)
		{
			m_Data.m_startSkill = true;
			StartSkill(null, m_Data.startSkill);
		}
		if (m_Data.m_startSkill && num > m_Data.endTime && IsSkillRunning(m_Data.startSkill))
		{
			StopSkill(m_Data.startSkill);
		}
		if (m_Data.endSkill > 0 && !m_Data.m_endSkill && num > m_Data.endTime)
		{
			m_Data.m_endSkill = true;
			StartSkill(null, m_Data.endSkill);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			SetSpeed(0f);
			SetBool(m_Data.anim, value: false);
			MoveDirection(Vector3.zero);
			FaceDirection(Vector3.zero);
			m_Data.m_StartTime = 0f;
		}
	}
}
