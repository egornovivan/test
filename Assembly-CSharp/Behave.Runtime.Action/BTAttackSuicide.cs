using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackSuicide), "AttackSuicide")]
public class BTAttackSuicide : BTAttackBase
{
	private class Data : IAttack
	{
		[Behave]
		public string anim = string.Empty;

		[Behave]
		public float hpPercent;

		[Behave]
		public float prob;

		[Behave]
		public float time;

		[Behave]
		public float radius;

		[Behave]
		public int skillID;

		public float Weight => prob;

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID);
		}

		public bool IsReadyCD(Enemy enemy)
		{
			return enemy.entity.IsSkillRunable(skillID);
		}

		public bool ReadyAttack(Enemy enemy)
		{
			return enemy.entity.HPPercent <= hpPercent;
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

	private void Explode()
	{
		SetViewActive(value: false);
		Object.Destroy(base.entity.gameObject, 0.2f);
		StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
		BehaveCmpt component = base.entity.GetComponent<BehaveCmpt>();
		if (component != null)
		{
			component.Pause(value: true);
		}
	}

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
		m_StartTime = Time.time;
		SetBool(m_Data.anim, value: true);
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
		if (Time.time - m_StartTime > m_Data.time || base.attackEnemy.SqrDistanceLogic < m_Data.radius * m_Data.radius)
		{
			Explode();
			return BehaveResult.Success;
		}
		FaceDirection(base.attackEnemy.position - base.position);
		MoveDirection(base.attackEnemy.position - base.position, SpeedState.Sprint);
		return BehaveResult.Running;
	}
}
