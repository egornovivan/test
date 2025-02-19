using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTStrikeTerrain), "StrikeTerrain")]
public class BTStrikeTerrain : BTAttackBase
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
		public float radius;

		[Behave]
		public float height;

		[Behave]
		public int skillID;

		public float m_LastCDTime;

		public float m_StartTime;

		public float Weight => prob;

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
			return enemy.SqrDistanceXZ <= radius * radius;
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

	private Vector3 m_TargetPosition;

	private bool m_Up;

	private bool m_Skill;

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
		m_Up = false;
		m_Skill = false;
		m_Data.m_LastCDTime = Time.time;
		m_TargetPosition = base.attackEnemy.position + Vector3.up * 15f;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!m_Up)
		{
			MoveDirection(m_TargetPosition - base.position, SpeedState.Run);
			if (PEUtil.SqrMagnitude(base.position, m_TargetPosition) < 1f)
			{
				m_Up = true;
				SetBool(m_Data.anim, value: true);
				return BehaveResult.Running;
			}
		}
		else
		{
			MoveDirection(Vector3.down, SpeedState.Sprint);
			if (!m_Skill)
			{
				if (base.grounded)
				{
					m_Skill = true;
					SetBool(m_Data.anim, value: false);
					if (base.attackEnemy != null)
					{
						StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
					}
				}
			}
			else if (!IsSkillRunning(m_Data.skillID))
			{
				return BehaveResult.Success;
			}
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (GetData(sender, ref m_Data) && m_TargetPosition != Vector3.zero)
		{
			StopSkill(m_Data.skillID);
			SetBool(m_Data.anim, value: false);
			MoveDirection(Vector3.zero);
			m_TargetPosition = Vector3.zero;
		}
	}
}
