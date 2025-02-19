using UnityEngine;

namespace Pathea.Operate;

public class PELay : Operation_Single
{
	public string LayAnim;

	public Transform m_StandTrans;

	public override bool Do(IOperator oper)
	{
		PEActionParamVFVFS param = PEActionParamVFVFS.param;
		param.vec1 = m_StandTrans.position;
		param.f1 = m_StandTrans.rotation.eulerAngles.y;
		param.vec2 = base.transform.position;
		param.f2 = base.transform.rotation.eulerAngles.y;
		param.str = LayAnim;
		return oper.DoAction(PEActionType.Cure, param);
	}

	public override bool Do(IOperator oper, PEActionParam para)
	{
		return true;
	}

	public override bool UnDo(IOperator oper)
	{
		oper.EndAction(PEActionType.Cure);
		return true;
	}

	public override EOperationMask GetOperateMask()
	{
		return EOperationMask.Lay;
	}
}
