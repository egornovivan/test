namespace Pathea.Operate;

public class PEWork : Operation_Single
{
	public string workAnim;

	public override bool Do(IOperator oper)
	{
		PEActionParamVQS param = PEActionParamVQS.param;
		param.vec = base.transform.position;
		param.q = base.transform.rotation;
		param.str = workAnim;
		return oper.DoAction(PEActionType.Operation, param);
	}

	public override bool Do(IOperator oper, PEActionParam para)
	{
		return true;
	}

	public override bool UnDo(IOperator oper)
	{
		oper.EndAction(PEActionType.Operation);
		return true;
	}

	public override EOperationMask GetOperateMask()
	{
		return EOperationMask.Work;
	}
}
