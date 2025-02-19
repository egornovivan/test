using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTRunRoll), "RunRoll")]
public class BTRunRoll : BTNormal
{
	private class Data
	{
		[Behave]
		public float Radius;

		[Behave]
		public float RunAwayTime;

		public EAttackCheck m_AttackCheck;

		public bool m_HasRoll;
	}

	private Data m_Data;

	private bool IsInEnemyFoward(Enemy enemy, PeEntity self)
	{
		Vector3 vector = self.position;
		Vector3 vector2 = enemy.position;
		Vector3 forward = enemy.entityTarget.peTrans.forward;
		Vector3 normalized = (vector - vector2).normalized;
		float num = Mathf.Abs(PEUtil.Angle(forward, normalized));
		return num <= 90f;
	}

	private bool InRadiu(Vector3 self, Vector3 target, float radiu)
	{
		float num = PEUtil.SqrMagnitudeH(self, target);
		return num < radiu * radiu;
	}

	private BehaveResult Tick(Tree sender)
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
		if (m_Data.m_AttackCheck != EAttackCheck.Roll)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.m_HasRoll)
		{
			return BehaveResult.Failure;
		}
		if (!InRadiu(base.position, base.attackEnemy.position, 3f))
		{
			return BehaveResult.Failure;
		}
		if (!IsInEnemyFoward(base.attackEnemy, base.entity))
		{
			return BehaveResult.Failure;
		}
		Vector3 vec = base.position - base.attackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		if (!CanDoAction(PEActionType.Step, param))
		{
			return BehaveResult.Failure;
		}
		PEActionParamV param2 = PEActionParamV.param;
		param2.vec = vec;
		m_Data.m_HasRoll = DoAction(PEActionType.Step, param2);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		if (m_Data != null)
		{
			m_Data.m_AttackCheck = EAttackCheck.None;
			m_Data.m_HasRoll = false;
		}
	}
}
