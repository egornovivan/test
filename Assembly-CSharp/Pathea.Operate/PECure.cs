namespace Pathea.Operate;

public class PECure : Operation_Single
{
	public string CureAnim;

	public override bool Do(IOperator oper)
	{
		PEActionParamVQS param = PEActionParamVQS.param;
		param.vec = base.transform.position;
		param.q = base.transform.rotation;
		param.str = CureAnim;
		return oper.DoAction(PEActionType.Operation, param);
	}

	public override bool Do(IOperator oper, PEActionParam para)
	{
		return true;
	}

	public override bool UnDo(IOperator oper)
	{
		oper.EndAction(PEActionType.Operation);
		return !oper.IsActionRunning(PEActionType.Operation);
	}

	public override EOperationMask GetOperateMask()
	{
		return EOperationMask.Cure;
	}
}
