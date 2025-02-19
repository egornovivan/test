using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTStrike), "Strike")]
public class BTStrike : BTAttackBase
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
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minHpPercent;

		[Behave]
		public float maxHpPercent = 1f;

		[Behave]
		public float speed;

		[Behave]
		public int skillID;

		public float m_LastCDTime;

		public float m_StartTime;

		private bool IsInHpRange(Enemy enemy)
		{
			return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
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
			return enemy.SqrDistance >= minRange * minRange && enemy.SqrDistanceXZ <= maxRange * maxRange && IsInHpRange(enemy);
		}

		public bool CanAttack(Enemy enemy)
		{
			return true;
		}

		public bool IsProbability(float randomValue)
		{
			return randomValue <= prob;
		}

		public bool IsBlocked(Enemy enemy)
		{
			return false;
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
	}

	private Data m_Data;

	private Vector3 m_TargetPosition;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.Ready())
		{
			return BehaveResult.Failure;
		}
		float sqrDistance = base.attackEnemy.SqrDistance;
		if (sqrDistance > m_Data.maxRange * m_Data.maxRange || sqrDistance < m_Data.minRange * m_Data.minRange)
		{
			return BehaveResult.Failure;
		}
		if (!IsSkillRunnable(m_Data.skillID))
		{
			return BehaveResult.Failure;
		}
		SetSpeed(m_Data.speed);
		m_Data.m_LastCDTime = Time.time;
		m_TargetPosition = base.attackEnemy.position;
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
		if (PEUtil.SqrMagnitude(base.position, m_TargetPosition) < 1f)
		{
			SetBool(m_Data.anim, value: false);
			return BehaveResult.Success;
		}
		MoveDirection(m_TargetPosition - base.position, SpeedState.Run);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_TargetPosition != Vector3.zero)
		{
			SetSpeed(0f);
			m_TargetPosition = Vector3.zero;
			MoveDirection(Vector3.zero);
			SetBool(m_Data.anim, value: false);
		}
	}
}
