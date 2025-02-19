using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTowerAttack), "TowerAttack")]
public class BTTowerAttack : BTAttackBase
{
	private class Data
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
		public bool isPitch;

		[Behave]
		public int skillID;

		private float m_LastCDTime;

		public bool m_CanAttack;

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
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!TowerIsEnable() || !TowerHaveCost() || TowerSkillRunning())
		{
			return BehaveResult.Failure;
		}
		if (!m_Data.Ready())
		{
			return BehaveResult.Failure;
		}
		float attackDistance = base.attackEnemy.AttackDistance;
		float minRange = m_Data.minRange;
		float num = Mathf.Max(m_Data.maxRange, attackDistance * 1.5f);
		if (base.attackEnemy.Distance < minRange || base.attackEnemy.Distance > num)
		{
			return BehaveResult.Failure;
		}
		Vector3 v = base.attackEnemy.position - base.position;
		if (!PEUtil.IsScopeAngle(v, base.transform.forward, Vector3.up, m_Data.minAngle, m_Data.maxAngle))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_CanAttack = true;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!TowerIsEnable())
		{
			return BehaveResult.Failure;
		}
		if (!base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		Transform centerBone = base.attackEnemy.CenterBone;
		if (centerBone == null)
		{
			return BehaveResult.Failure;
		}
		SetTowerAimPosition(centerBone);
		if (centerBone != null && !TowerAngle(centerBone.position, 5f))
		{
			return BehaveResult.Running;
		}
		if (m_Data.isPitch && !TowerPitchAngle(centerBone.position, 5f))
		{
			return BehaveResult.Running;
		}
		if (centerBone != null && !TowerCanAttack(centerBone.position, base.attackEnemy.trans))
		{
			return BehaveResult.Running;
		}
		if (m_Data.m_CanAttack)
		{
			m_Data.SetCDTime(Time.time);
			TowerFire(base.attackEnemy.skTarget);
			m_Data.m_CanAttack = false;
			return BehaveResult.Running;
		}
		if (TowerSkillRunning())
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
