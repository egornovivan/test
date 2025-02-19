using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTAttcakingAway), "AttcakingAway")]
public class BTAttcakingAway : BTNormal
{
	private class Data
	{
		[Behave]
		public float awayTime;

		[Behave]
		public float awayRadiu;

		public float StartTime;
	}

	private Data m_Data;

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

	private bool CanStep()
	{
		Vector3 vec = base.position - base.selectattackEnemy.position;
		PEActionParamV param = PEActionParamV.param;
		param.vec = vec;
		return InRadiu(base.position, base.selectattackEnemy.position, 3f) && CanDoAction(PEActionType.Step, param);
	}

	private void RunAway()
	{
		Vector3 vector = base.position - base.selectattackEnemy.position;
		Vector3 pos = base.position + vector * 3f;
		MoveToPosition(pos, SpeedState.Sprint);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.StartTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (Time.time - m_Data.StartTime > m_Data.awayTime)
		{
			return BehaveResult.Success;
		}
		if (Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Failure;
		}
		bool flag = CanDoAction(PEActionType.HoldShield);
		bool flag2 = CanStep();
		if (!flag && !flag2)
		{
			RunAway();
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
		return BehaveResult.Running;
	}
}
