using System.Collections.Generic;
using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTMeleeAttack), "MeleeAttack")]
public class BTMeleeAttack : BTAttackBase
{
	private class Data : IAttack
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
		public float minHpPercent;

		[Behave]
		public float maxHpPercent = 1f;

		[Behave]
		public Vector3 axis = Vector3.zero;

		[Behave]
		public float angle;

		[Behave]
		public bool isBlock = true;

		[Behave]
		public int[] skillStr = new int[0];

		private List<int> m_Skills = new List<int>();

		public int m_SkillID;

		public float m_LastCDTime;

		public float m_StartTime;

		public float Weight => prob;

		private bool IsInHpRange(Enemy enemy)
		{
			return enemy.entity.HPPercent >= minHpPercent && enemy.entity.HPPercent <= maxHpPercent;
		}

		public int GetRandomSkill(Enemy enemy)
		{
			m_Skills.Clear();
			for (int i = 0; i < skillStr.Length; i++)
			{
				if (enemy.entity.IsSkillRunable(skillStr[i]))
				{
					m_Skills.Add(skillStr[i]);
				}
			}
			if (m_Skills.Count > 0)
			{
				return m_Skills[Random.Range(0, m_Skills.Count)];
			}
			return 0;
		}

		public bool IsReadyCD(Enemy enemy)
		{
			if (Time.time - m_LastCDTime > cdTime)
			{
				for (int i = 0; i < skillStr.Length; i++)
				{
					if (enemy.entity.IsSkillRunable(skillStr[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool ReadyAttack(Enemy enemy)
		{
			if (!IsInHpRange(enemy))
			{
				return false;
			}
			if (enemy.SqrDistanceLogic < minRange * minRange || enemy.SqrDistanceLogic > maxRange * maxRange)
			{
				return false;
			}
			if (!enemy.Inside && !PEUtil.IsScopeAngle(enemy.Direction, enemy.entity.tr.forward, Vector3.up, minAngle, maxAngle))
			{
				return false;
			}
			if (axis != Vector3.zero)
			{
				Vector3 from = enemy.entity.tr.TransformDirection(axis);
				if (Vector3.Angle(from, enemy.Direction) > angle)
				{
					return false;
				}
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
			return !IsBlocked(enemy);
		}

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(m_SkillID, cdInclude: false);
		}

		public bool IsBlocked(Enemy enemy)
		{
			return isBlock && PEUtil.IsBlocked(enemy.entity, enemy.entityTarget);
		}

		public bool CanInterrupt()
		{
			return true;
		}
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (PEUtil.IsDamageBlock(base.entity))
		{
			m_Data.m_SkillID = m_Data.GetRandomSkill(base.attackEnemy);
			if (m_Data.m_SkillID <= 0)
			{
				return BehaveResult.Failure;
			}
			if (!base.entity.IsSkillRunable(m_Data.m_SkillID))
			{
				return BehaveResult.Failure;
			}
			StartSkillSkEntity(Block45Man.self, m_Data.m_SkillID);
			return BehaveResult.Running;
		}
		if (base.attackEnemy.Attack == null || !base.attackEnemy.Attack.Equals(m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.attackEnemy.Attack.ReadyAttack(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_SkillID = m_Data.GetRandomSkill(base.attackEnemy);
		if (m_Data.m_SkillID <= 0)
		{
			return BehaveResult.Failure;
		}
		StopMove();
		m_Data.m_LastCDTime = Time.time;
		m_Data.m_StartTime = Time.time;
		StartSkill(null, m_Data.m_SkillID);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (IsSkillRunning(m_Data.m_SkillID) || base.entity.IsAttacking)
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_Data.m_StartTime > float.Epsilon)
		{
			if (IsSkillRunning(m_Data.m_SkillID))
			{
				StopSkill(m_Data.m_SkillID);
			}
			m_Data.m_StartTime = 0f;
		}
	}
}
