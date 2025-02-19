using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTRunAway), "RunAway")]
public class BTRunAway : BTNormal
{
	private class Data
	{
		[Behave]
		public float Radius;

		[Behave]
		public float RunAwayTime;

		public EAttackCheck m_AttackCheck;

		public Vector3 m_Dirction;

		public float m_StarRunTime;
	}

	private Data m_Data;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (Enemy.IsNullOrInvalid(base.attackEnemy) || base.attackEnemy.entityTarget == null || base.attackEnemy.entityTarget.animCmpt == null)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_AttackCheck = (EAttackCheck)base.attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
		if (m_Data.m_AttackCheck == EAttackCheck.All)
		{
			m_Data.m_AttackCheck = ((!(Random.value > 0.5f)) ? (m_Data.m_AttackCheck = EAttackCheck.Roll) : (m_Data.m_AttackCheck = EAttackCheck.RunAway));
		}
		if (m_Data.m_AttackCheck != EAttackCheck.RunAway)
		{
			return BehaveResult.Failure;
		}
		float num = PEUtil.SqrMagnitudeH(base.position, base.attackEnemy.position);
		if (num > m_Data.Radius * m_Data.Radius)
		{
			return BehaveResult.Failure;
		}
		Vector3 vector = base.position - base.attackEnemy.position;
		m_Data.m_Dirction = base.position + vector * m_Data.Radius;
		m_Data.m_StarRunTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_AttackCheck = (EAttackCheck)base.attackEnemy.entityTarget.animCmpt.GetInteger("attackCheck");
		if (m_Data.m_AttackCheck != EAttackCheck.RunAway)
		{
			return BehaveResult.Failure;
		}
		if (!IsReached(base.position, m_Data.m_Dirction))
		{
			MoveToPosition(m_Data.m_Dirction, SpeedState.Run);
		}
		else
		{
			StopMove();
			FaceDirection(base.attackEnemy.position - base.position);
		}
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Data != null)
		{
			m_Data.m_Dirction = Vector3.zero;
			m_Data.m_AttackCheck = EAttackCheck.None;
		}
	}
}
