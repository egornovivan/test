using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTSprint), "Sprint")]
public class BTSprint : BTAttackBase
{
	private class Data : IAttack
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public string sprint = string.Empty;

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
		public float speed;

		[Behave]
		public int skillID;

		[Behave]
		public bool auto;

		public float m_StartTime;

		public float m_StopTime;

		public float m_EndTime;

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

		public void SetEndTime(float time)
		{
			m_EndTime = time;
		}

		public bool Ready()
		{
			if (Cooldown())
			{
				return Random.value <= prob;
			}
			return false;
		}

		public float GetEndTime()
		{
			if (auto)
			{
				return m_EndTime;
			}
			return endTime;
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
			return true;
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
			return true;
		}

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID, cdInclude: false);
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
		m_Data.m_Started = false;
		m_Data.m_Stoped = false;
		m_Data.m_Face = false;
		m_Data.m_StartRotateTime = Time.time;
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
			float num = PEUtil.MagnitudeH(base.position, base.attackEnemy.position) - base.radius - base.attackEnemy.radius;
			m_Data.SetEndTime(num / m_Data.speed);
			SetBool(m_Data.sprint, value: true);
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
				m_Data.m_Stoped = true;
				m_Data.m_StopTime = Time.time;
				SetBool(m_Data.sprint, value: false);
				MoveDirection(Vector3.zero);
				FaceDirection(Vector3.zero);
			}
		}
		else
		{
			SetSpeed(m_Data.speed);
			MoveDirection(base.transform.forward, SpeedState.Sprint);
			FaceDirection(base.transform.forward);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			SetSpeed(0f);
			SetBool(m_Data.sprint, value: false);
			StopSkill(m_Data.skillID);
			MoveDirection(Vector3.zero);
			FaceDirection(Vector3.zero);
			m_Data.m_StartTime = 0f;
		}
	}
}
