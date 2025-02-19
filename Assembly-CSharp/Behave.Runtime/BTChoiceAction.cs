using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTChoiceAction), "ChoiceAction")]
public class BTChoiceAction : BTNormal
{
	private static float X = 3f;

	private float mStartActionTime;

	private float mActiomTime = 1f;

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

	private void DoStep()
	{
		Vector3 vec = base.position - base.selectattackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		DoAction(PEActionType.Step, param);
	}

	private void DoSheid()
	{
		DoAction(PEActionType.HoldShield);
		Vector3 dir = base.selectattackEnemy.position - base.position;
		FaceDirection(dir);
	}

	private bool EndSheid()
	{
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		return !IsMotionRunning(PEActionType.HoldShield);
	}

	private bool CanStep()
	{
		Vector3 vec = base.position - base.selectattackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		return InRadiu(base.position, base.selectattackEnemy.position, 3f) && IsInEnemyFoward(base.selectattackEnemy, base.entity) && CanDoAction(PEActionType.Step, param);
	}

	private bool RunAway()
	{
		if (InRadiu(base.entity.position, base.selectattackEnemy.position, X))
		{
			Vector3 vector = base.position - base.selectattackEnemy.position;
			vector.y = 0f;
			Vector3 pos = base.position + vector * X;
			MoveToPosition(pos, SpeedState.Sprint);
			return true;
		}
		return false;
	}

	private BehaveResult Init(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		mStartActionTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			if (EndSheid())
			{
				return BehaveResult.Success;
			}
			return BehaveResult.Running;
		}
		bool flag = CanDoAction(PEActionType.HoldShield);
		bool flag2 = CanStep();
		if (!flag && !flag2)
		{
			if (RunAway())
			{
				return BehaveResult.Running;
			}
			return BehaveResult.Success;
		}
		if (!flag && flag2)
		{
			DoStep();
		}
		if (flag && !flag2)
		{
			DoSheid();
		}
		if (flag && flag2)
		{
			if (Random.value > 0.5f)
			{
				DoStep();
			}
			else
			{
				DoSheid();
			}
		}
		if (Time.time - mStartActionTime > mActiomTime)
		{
			if (EndSheid())
			{
				return BehaveResult.Success;
			}
			return BehaveResult.Running;
		}
		return BehaveResult.Running;
	}
}
