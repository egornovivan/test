using UnityEngine;

namespace Pathea.Operate;

public abstract class Operation : MonoBehaviour, IOperation
{
	internal EOperationMask m_Mask;

	public Transform Trans => base.transform;

	public void Awake()
	{
		m_Mask = GetOperateMask();
	}

	public bool ContainsMask(EOperationMask mask)
	{
		return (m_Mask & mask) != 0;
	}

	public virtual bool IsIdle()
	{
		return true;
	}

	public abstract bool CanOperate(Transform trans);

	public abstract EOperationMask GetOperateMask();

	public abstract bool CanOperateMask(EOperationMask mask);

	public abstract bool ContainsOperator(IOperator oper);

	public abstract bool StartOperate(IOperator oper, EOperationMask mask);

	public abstract bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para);

	public abstract bool StopOperate(IOperator oper, EOperationMask mask);
}
