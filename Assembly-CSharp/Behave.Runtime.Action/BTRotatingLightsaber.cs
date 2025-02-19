using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTRotatingLightsaber), "RotatingLightsaber")]
public class BTRotatingLightsaber : BTAttackBase
{
	private class Data : IAttack
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minAngle;

		[Behave]
		public float maxAngle;

		[Behave]
		public int skillID;

		private float m_LastCDTime;

		public float m_StartTime;

		public float Weight => prob;

		public void SetCDTime(float time)
		{
			m_LastCDTime = time;
		}

		public bool Ready()
		{
			if (Cooldown())
			{
				return Random.value <= prob;
			}
			return false;
		}

		private bool Cooldown()
		{
			return Time.time - m_LastCDTime > cdTime;
		}

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID);
		}

		public bool IsReadyCD(Enemy enemy)
		{
			return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
		}

		public bool ReadyAttack(Enemy enemy)
		{
			if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
			{
				return false;
			}
			if (!PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
			{
				return false;
			}
			return true;
		}

		public bool CanAttack(Enemy enemy)
		{
			return true;
		}

		public bool IsBlocked(Enemy enemy)
		{
			return false;
		}

		public bool CanInterrupt()
		{
			return false;
		}
	}

	private Data m_Data;

	private float m_StartTime;

	private float m_Angle;

	private bool m_Arrived;

	private Vector3 m_TargetPosition;

	private Vector3 m_EndPosition;

	private Vector3 m_FaceDirection;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.Attack == null || !base.attackEnemy.Attack.Equals(m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.attackEnemy.Attack.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		StopMove();
		m_Angle = 0f;
		m_Arrived = false;
		m_StartTime = Time.time;
		Vector3 vector = base.attackEnemy.position - base.position;
		Vector3 normalized = Vector3.ProjectOnPlane(vector, Vector3.up).normalized;
		Vector3 vector2 = Vector3.ProjectOnPlane(normalized * 5f + Vector3.up * 2f, base.transform.right);
		m_TargetPosition = base.attackEnemy.centerPos + vector2;
		m_Data.SetCDTime(Time.time);
		SetBool(m_Data.anim, value: true);
		StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Stucking(3f))
		{
			return BehaveResult.Failure;
		}
		if (!m_Arrived)
		{
			m_Arrived = PEUtil.SqrMagnitudeH(base.position, m_TargetPosition) < 1f;
		}
		if (!m_Arrived)
		{
			FaceDirection(base.transform.forward);
			if (GetBool("LaserRunning"))
			{
				MoveDirection(m_TargetPosition - base.position, SpeedState.Run);
			}
		}
		else if (GetBool(m_Data.anim))
		{
			SetBool(m_Data.anim, value: false);
		}
		else if (!GetBool("Lasering"))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_StartTime > float.Epsilon)
		{
			StopSkill(m_Data.skillID);
			SetBool(m_Data.anim, value: false);
			MoveDirection(Vector3.zero);
			FaceDirection(Vector3.zero);
			m_StartTime = 0f;
		}
	}
}
