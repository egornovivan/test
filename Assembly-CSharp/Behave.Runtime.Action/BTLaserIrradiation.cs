using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTLaserIrradiation), "LaserIrradiation")]
public class BTLaserIrradiation : BTAttackBase
{
	private class Data : IAttack, IAttackPositive
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
		public float minAngle;

		[Behave]
		public float maxAngle;

		[Behave]
		public float minHeight;

		[Behave]
		public float maxHeight;

		[Behave]
		public float minHpPercent;

		[Behave]
		public float maxHpPercent = 1f;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float speed;

		[Behave]
		public int skillID;

		[Behave]
		public bool isBlock;

		public float m_LastCDTime;

		public float m_StartTime;

		public float m_Time;

		public Vector3 m_Direction;

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
			return Time.time - m_LastCDTime > cdTime && enemy.entity.IsSkillRunable(skillID);
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
			if (!Physics.Raycast(enemy.entityTarget.position + Vector3.up, Vector3.down, 6f, 4096))
			{
				return false;
			}
			return true;
		}

		public Vector3 GetAttackPosition(Enemy enemy)
		{
			float num = (minRange + maxRange) * 0.5f + enemy.entity.maxRadius + enemy.entityTarget.maxRadius;
			float num2 = (minHeight + maxHeight) * 0.5f + enemy.entity.maxHeight;
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
			float sqrDistanceXZ = enemy.SqrDistanceXZ;
			return sqrDistanceXZ >= minRange * minRange && sqrDistanceXZ <= maxRange * maxRange;
		}

		public bool IsInAngle(Enemy enemy)
		{
			if (enemy.Inside)
			{
				return true;
			}
			return PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle);
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
		m_Data.m_LastCDTime = Time.time;
		m_Data.m_StartTime = Time.time;
		if (m_Data.speed <= Mathf.Epsilon)
		{
			m_Data.m_Time = Random.Range(m_Data.minTime, m_Data.maxTime);
		}
		else
		{
			m_Data.m_Time = (base.attackEnemy.DistanceXZ + 10f) / m_Data.speed;
		}
		m_Data.m_Direction = base.attackEnemy.DirectionXZ;
		SetSpeed(m_Data.speed);
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
		if (Time.time - m_Data.m_StartTime > m_Data.m_Time)
		{
			return BehaveResult.Success;
		}
		FaceDirection(m_Data.m_Direction);
		MoveDirection(m_Data.m_Direction);
		Debug.DrawRay(base.position, m_Data.m_Direction.normalized * 5f, Color.grey);
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
			SetSpeed(0f);
			m_Data.m_StartTime = 0f;
			FaceDirection(Vector3.zero);
			MoveDirection(Vector3.zero);
		}
	}
}
