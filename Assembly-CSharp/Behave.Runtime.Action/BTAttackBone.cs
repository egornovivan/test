using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTAttackBone), "AttackBone")]
public class BTAttackBone : BTAttackBase
{
	private class Data : IAttack
	{
		[Behave]
		public string boneName = string.Empty;

		[Behave]
		public float prob;

		[Behave]
		public float cdTime;

		[Behave]
		public float minRange;

		[Behave]
		public float maxRange;

		[Behave]
		public float angle;

		[Behave]
		public int skillID;

		public float m_LastCDTime;

		public float Weight => prob;

		public bool IsRunning(Enemy enemy)
		{
			return enemy.entity.IsSkillRunning(skillID, cdInclude: false);
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

	private PEBoneRotation m_Bone;

	private bool m_CanAttack;

	private bool m_Attacked;

	private PEBoneRotation GetBone()
	{
		if (base.entity != null)
		{
			BiologyViewCmpt biologyViewCmpt = base.entity.biologyViewCmpt;
			if (biologyViewCmpt != null)
			{
				Transform modelTransform = biologyViewCmpt.GetModelTransform(m_Data.boneName);
				if (modelTransform != null)
				{
					return modelTransform.GetComponent<PEBoneRotation>();
				}
			}
		}
		return null;
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
		if (m_Bone == null)
		{
			m_Bone = GetBone();
		}
		if (m_Bone == null)
		{
			return BehaveResult.Failure;
		}
		m_CanAttack = false;
		m_Attacked = false;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (m_CanAttack)
		{
			if (!m_Attacked)
			{
				m_Attacked = true;
				m_Data.m_LastCDTime = Time.time;
				StartSkill(base.attackEnemy.entityTarget, m_Data.skillID);
				return BehaveResult.Running;
			}
			if (IsSkillRunning(m_Data.skillID))
			{
				return BehaveResult.Running;
			}
			return BehaveResult.Success;
		}
		Vector3 from = Vector3.ProjectOnPlane(base.attackEnemy.position - base.position, Vector3.up);
		Vector3 to = Vector3.ProjectOnPlane(m_Bone.transform.forward, Vector3.up);
		if (Vector3.Angle(from, to) > m_Data.angle)
		{
			m_Bone.target = base.attackEnemy.modelTrans;
		}
		else
		{
			m_CanAttack = true;
			m_Bone.target = null;
		}
		return BehaveResult.Running;
	}
}
