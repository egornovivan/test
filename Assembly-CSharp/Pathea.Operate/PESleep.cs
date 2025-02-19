using UnityEngine;
using WhiteCat;

namespace Pathea.Operate;

public class PESleep : Operation_Single
{
	private int id = 30200055;

	private string Ainm = "Sleep";

	public PECureSleep mCurSleep;

	public override bool Do(IOperator oper)
	{
		PEActionParamVQNS param = PEActionParamVQNS.param;
		param.vec = base.transform.position;
		param.q = base.transform.rotation;
		param.n = id;
		param.str = Ainm;
		return oper.DoAction(PEActionType.Sleep, param);
	}

	public override bool Do(IOperator oper, PEActionParam para)
	{
		bool flag = oper.DoAction(PEActionType.Sleep, para);
		if (flag && mCurSleep != null)
		{
			mCurSleep.AddOperator(oper);
		}
		return flag;
	}

	public override bool UnDo(IOperator oper)
	{
		oper.EndAction(PEActionType.Sleep);
		bool flag = true;
		if (mCurSleep != null && flag)
		{
			mCurSleep.RemveOperator();
		}
		return flag;
	}

	public override bool CanOperateMask(EOperationMask mask)
	{
		if (base.CanOperateMask(mask))
		{
			float num = Vector3.Angle(base.transform.up, Vector3.up);
			if (num < PEVCConfig.instance.bedLimitAngle)
			{
				return true;
			}
		}
		return false;
	}

	public override EOperationMask GetOperateMask()
	{
		return EOperationMask.Sleep;
	}
}
