using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackRanged), "AttackRanged")]
public class BTAttackRanged : BTAttackBase
{
	private class Data : IAttack, IAttackPositive, IAttackRanged
	{
		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public float minHpPercent;

		[Behave]
		public float maxHpPercent = 1f;

		[Behave]
		public float angle;

		[Behave]
		public float pitchAngle;

		[Behave]
		public string boneName = string.Empty;

		[Behave]
		public Vector3 pivot = Vector3.forward;

		[Behave]
		public bool isBlock;

		[Behave]
		public int skillID;

		public float m_LastCDTime;

		public float m_StartTime;

		public float MinRange => minRange;

		public float MaxRange => maxRange;

		public float Weight => prob;

		public float MinHeight => minHeight;

		public float MaxHeight => maxHeight;

		private bool IsInHpRange(Enemy enemy)
		{
			return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
		}

		public Vector3 GetAttackPosition(Enemy enemy)
		{
			float num = (minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius;
			float num2 = (minHeight + maxHeight) * 0.5f - enemy.entity.maxHeight * 0.5f;
			return enemy.entityTarget.position - enemy.DirectionXZ.normalized * num;
		}

		public bool IsReadyCD(Enemy enemy)
		{
			return enemy.entity.IsSkillRunable(skillID) && Time.time - m_LastCDTime > cdTime;
		}

		public bool ReadyAttack(Enemy enemy)
		{
			return IsInRange(enemy) && IsInAngle(enemy) && IsInHpRange(enemy) && !IsBlocked(enemy);
		}

		public bool CanAttack(Enemy enemy)
		{
			return true;
		}

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID, cdInclude: false);
		}

		public bool IsInRange(Enemy enemy)
		{
			return enemy.SqrDistanceLogic >= minRange * minRange && enemy.SqrDistanceLogic <= maxRange * maxRange;
		}

		public bool IsInAngle(Enemy enemy)
		{
			return PEUtil.Angle(enemy.Direction, enemy.entity.tr.forward, Vector3.up) <= angle;
		}

		public bool IsBlocked(Enemy enemy)
		{
			return isBlock && PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
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
		if (base.attackEnemy == null || base.attackEnemy.Attack == null || !base.attackEnemy.Attack.Equals(m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.attackEnemy.Attack.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		StopMove();
		m_Data.m_StartTime = Time.time;
		m_Data.m_LastCDTime = Time.time;
		StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (IsSkillRunning(m_Data.skillID) || base.entity.IsAttacking)
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			if (IsSkillRunning(m_Data.skillID))
			{
				StopSkill(m_Data.skillID);
			}
			if (base.entity.IsAttacking)
			{
				SetBool("Interrupt", value: true);
			}
			m_Data.m_StartTime = 0f;
		}
	}
}
