namespace Pathea.Operate;

public class PEEat : Operation_Single
{
	public string sitAnim;

	public override bool Do(IOperator oper)
	{
		PEActionParamVQS param = PEActionParamVQS.param;
		param.vec = base.transform.position;
		param.q = base.transform.rotation;
		param.str = sitAnim;
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
		return EOperationMask.Eat;
	}
}
