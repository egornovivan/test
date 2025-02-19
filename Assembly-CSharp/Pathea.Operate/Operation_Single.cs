using System;
using UnityEngine;

namespace Pathea.Operate;

public abstract class Operation_Single : Operation
{
	private static EOperationMask[] pairOperationTypes = new EOperationMask[1] { EOperationMask.ClimbLadder };

	private static PEActionType[] pairActionTypes = new PEActionType[1] { PEActionType.Climb };

	private IOperator m_Operator;

	public IOperator Operator
	{
		get
		{
			if (m_Operator == null || m_Operator.Equals(null))
			{
				return null;
			}
			return m_Operator;
		}
		set
		{
			m_Operator = value;
		}
	}

	public abstract bool Do(IOperator oper);

	public abstract bool Do(IOperator oper, PEActionParam para);

	public abstract bool UnDo(IOperator oper);

	public override bool ContainsOperator(IOperator oper)
	{
		if (IsIdle())
		{
			return false;
		}
		if (Operator == null)
		{
			return false;
		}
		return m_Operator.Equals(oper);
	}

	public override bool CanOperateMask(EOperationMask mask)
	{
		if (m_Mask == mask)
		{
			return IsIdle();
		}
		return false;
	}

	public override bool IsIdle()
	{
		if (Operator == null)
		{
			return true;
		}
		int num = Array.IndexOf(pairOperationTypes, GetOperateMask());
		if (num >= 0 && !m_Operator.IsActionRunning(pairActionTypes[num]))
		{
			m_Operator = null;
			return true;
		}
		return false;
	}

	public override bool StartOperate(IOperator oper, EOperationMask mask)
	{
		if (oper == null || oper.Equals(null))
		{
			return false;
		}
		if (m_Mask == mask && !ContainsOperator(oper))
		{
			Operator = oper;
			oper.Operate = this;
			if (!Do(oper))
			{
				Operator = null;
				oper.Operate = null;
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para)
	{
		if (oper == null || oper.Equals(null))
		{
			return false;
		}
		if (m_Mask == mask && !ContainsOperator(oper))
		{
			Operator = oper;
			oper.Operate = this;
			if (!Do(oper, para))
			{
				Operator = null;
				oper.Operate = null;
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool StopOperate(IOperator oper, EOperationMask mask)
	{
		if (oper == null || oper.Equals(null))
		{
			return false;
		}
		if (m_Mask == mask && ContainsOperator(oper))
		{
			if (UnDo(oper))
			{
				m_Operator = null;
				oper.Operate = null;
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool CanOperate(Transform trans)
	{
		return false;
	}

	public override EOperationMask GetOperateMask()
	{
		return EOperationMask.None;
	}
}
