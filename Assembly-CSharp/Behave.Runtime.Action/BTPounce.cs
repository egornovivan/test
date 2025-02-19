using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTPounce), "Pounce")]
public class BTPounce : BTAttackBase
{
	private class Data : IAttack
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public string pounce = string.Empty;

		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minAngle;

		[Behave]
		public float maxAngle;

		[Behave]
		public float startTime;

		[Behave]
		public float endTime;

		[Behave]
		public float stopTime;

		[Behave]
		public int skillID;

		public float m_Speed;

		public float m_StartTime;

		public float m_StopTime;

		public float m_StartRotateTime;

		public bool m_Started;

		public bool m_Stoped;

		public bool m_Face;

		private float m_LastCDTime;

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
			Vector3 v = enemy.position - enemy.entity.position;
			if (!PEUtil.IsScopeAngle(v, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
			{
				return false;
			}
			return !IsBlocked(enemy);
		}

		public bool CanAttack(Enemy enemy)
		{
			if (enemy.entity.Field == MovementField.Sky && enemy.IsInWater)
			{
				return false;
			}
			if (enemy.entity.Field == MovementField.Land && !enemy.IsOnLand)
			{
				return false;
			}
			if (enemy.entity.Field == MovementField.water && !enemy.IsInWater)
			{
				return false;
			}
			return !IsBlocked(enemy);
		}

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID, cdInclude: false);
		}

		public bool IsBlocked(Enemy enemy)
		{
			return PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
		}

		public bool CanInterrupt()
		{
			return true;
		}
	}

	private Data m_Data;

	private float m_Gravity;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null || base.attackEnemy.Attack == null || !base.attackEnemy.Attack.Equals(m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.attackEnemy.Attack.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_Started = false;
		m_Data.m_Stoped = false;
		m_Data.m_Face = false;
		m_Data.m_Speed = 0f;
		m_Data.m_StartRotateTime = Time.time;
		m_Gravity = base.entity.gravity;
		base.entity.gravity = 0f;
		if (base.entity.motionMove != null && base.entity.motionMove is Motion_Move_Motor)
		{
			Motion_Move_Motor motion_Move_Motor = base.entity.motionMove as Motion_Move_Motor;
			if (motion_Move_Motor != null && motion_Move_Motor.motor != null)
			{
				motion_Move_Motor.motor.desiredMovementEffect = Vector3.zero;
			}
		}
		StopMove();
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = base.attackEnemy.position - base.position;
		if (!m_Data.m_Face)
		{
			m_Data.m_Face = PEUtil.IsScopeAngle(vector, base.transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle);
		}
		if (!m_Data.m_Face)
		{
			FaceDirection(vector);
			if (Time.time - m_Data.m_StartRotateTime > 5f)
			{
				return BehaveResult.Failure;
			}
			return BehaveResult.Running;
		}
		if (!m_Data.m_Started)
		{
			m_Data.m_Started = true;
			m_Data.m_StartTime = Time.time;
			m_Data.SetCDTime(Time.time);
			SetBool(m_Data.pounce, value: true);
			StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
			return BehaveResult.Running;
		}
		if (m_Data.m_Stoped && Time.time - m_Data.m_StartTime > m_Data.stopTime)
		{
			return BehaveResult.Success;
		}
		if (Time.time - m_Data.m_StartTime < m_Data.startTime)
		{
			StopMove();
		}
		else if (Time.time - m_Data.m_StartTime > m_Data.endTime)
		{
			if (!m_Data.m_Stoped)
			{
				StopMove();
				m_Data.m_Stoped = true;
				m_Data.m_StopTime = Time.time;
				SetBool(m_Data.pounce, value: false);
				MoveDirection(Vector3.zero);
				FaceDirection(Vector3.zero);
				MoveToPosition(base.attackEnemy.position, SpeedState.Run);
				return BehaveResult.Success;
			}
		}
		else
		{
			if (m_Data.m_Speed < float.Epsilon)
			{
				m_Data.m_Speed = (PEUtil.MagnitudeH(base.position, base.attackEnemy.position) + 1f) / (m_Data.endTime - m_Data.startTime);
			}
			SetSpeed(m_Data.m_Speed);
			MoveDirection(base.transform.forward);
			FaceDirection(base.attackEnemy.DirectionXZ);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartRotateTime > float.Epsilon)
		{
			SetSpeed(0f);
			SetBool(m_Data.pounce, value: false);
			MoveDirection(Vector3.zero);
			FaceDirection(Vector3.zero);
			base.entity.gravity = m_Gravity;
			m_Data.m_StartRotateTime = 0f;
		}
	}
}
