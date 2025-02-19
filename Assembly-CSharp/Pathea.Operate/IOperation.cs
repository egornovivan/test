using UnityEngine;

namespace Pathea.Operate;

public interface IOperation
{
	Transform Trans { get; }

	bool CanOperate(Transform trans);

	bool CanOperateMask(EOperationMask mask);

	bool ContainsMask(EOperationMask mask);

	bool ContainsOperator(IOperator oper);

	bool StartOperate(IOperator oper, EOperationMask mask);

	bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para);

	bool StopOperate(IOperator oper, EOperationMask mask);
}
