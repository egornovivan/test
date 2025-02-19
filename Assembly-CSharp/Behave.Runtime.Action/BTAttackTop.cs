using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackTop), "AttackTop")]
public class BTAttackTop : BTAttackBase
{
	private class Data : IAttack, IAttackPositive, IAttackTop
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
		public float angle;

		[Behave]
		public float minHpPercent;

		[Behave]
		public float maxHpPercent = 1f;

		[Behave]
		public int skillID;

		[Behave]
		public bool isBlock;

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

		public bool IsReadyCD(Enemy enemy)
		{
			return Time.time - m_LastCDTime >= cdTime && enemy.entity.IsSkillRunable(skillID);
		}

		public bool ReadyAttack(Enemy enemy)
		{
			if (!IsInRange(enemy))
			{
				return false;
			}
			if (!IsInAngle(enemy))
			{
				return false;
			}
			if (!IsInHpRange(enemy))
			{
				return false;
			}
			if (IsBlocked(enemy))
			{
				return false;
			}
			if (!Physics.Raycast(enemy.entityTarget.position + Vector3.up, Vector3.down, 6f, 4096))
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

		public Vector3 GetAttackPosition(Enemy enemy)
		{
			float num = (minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius;
			float num2 = (minHeight + maxHeight) * 0.5f + enemy.entityTarget.maxHeight;
			Vector3 result = enemy.entityTarget.position - enemy.DirectionXZ.normalized * num;
			if (Physics.Raycast(enemy.entityTarget.position + Vector3.up * 128f, Vector3.down, out var hitInfo, 256f, 12))
			{
				result = hitInfo.point + Vector3.up * num2;
			}
			return result;
		}

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID, cdInclude: false);
		}

		public bool IsInRange(Enemy enemy)
		{
			float num = enemy.entity.position.y - enemy.entityTarget.position.y - enemy.entityTarget.maxHeight;
			return enemy.SqrDistanceXZ >= minRange * minRange && enemy.SqrDistanceXZ <= maxRange * maxRange && num <= maxHeight && num >= minHeight;
		}

		public bool IsInAngle(Enemy enemy)
		{
			return Vector3.Angle(-enemy.entity.tr.up, enemy.Direction) <= angle;
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
		if (base.attackEnemy.Attack == null || !base.attackEnemy.Attack.Equals(m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.attackEnemy.Attack.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!base.entity.IsSkillRunable(m_Data.skillID))
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
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.m_StartTime < 0.5f)
		{
			return BehaveResult.Running;
		}
		if (!IsSkillRunning(m_Data.skillID))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			if (IsSkillRunning(m_Data.skillID))
			{
				StopSkill(m_Data.skillID);
			}
			m_Data.m_StartTime = 0f;
		}
	}
}
