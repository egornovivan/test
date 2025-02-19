using UnityEngine;
using WhiteCat;

namespace Pathea.Operate;

public class PESit : Operation_Single
{
	public string Ainm;

	[SerializeField]
	private int m_BuffID;

	public override bool Do(IOperator oper)
	{
		PEActionParamVQSN param = PEActionParamVQSN.param;
		param.vec = base.transform.position;
		param.q = base.transform.rotation;
		param.str = Ainm;
		param.n = m_BuffID;
		return oper.DoAction(PEActionType.Sit, param);
	}

	public override bool Do(IOperator oper, PEActionParam para)
	{
		return true;
	}

	public override bool UnDo(IOperator oper)
	{
		oper.EndAction(PEActionType.Sit);
		return true;
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
		return EOperationMask.Sit;
	}
}
