using System.Collections.Generic;
using PETools;
using UnityEngine;

namespace Pathea.Operate;

public abstract class Operation_Multiple : Operation
{
	private Bounds m_Bounds;

	private Bounds m_OperateBounds;

	public abstract List<Operation_Single> Singles { get; }

	public Bounds LocalBounds
	{
		get
		{
			if (m_Bounds.size == Vector3.zero)
			{
				m_Bounds = PEUtil.GetLocalViewBoundsInChildren(base.gameObject);
			}
			return m_Bounds;
		}
	}

	public Bounds OperateBoundsBounds
	{
		get
		{
			if (m_OperateBounds.size == Vector3.zero)
			{
				m_OperateBounds = PEUtil.GetLocalViewBoundsInChildren(base.gameObject);
				if (m_OperateBounds.size != Vector3.zero)
				{
					m_OperateBounds.Expand(3f);
				}
			}
			return m_OperateBounds;
		}
	}

	public Operation_Single GetStartOperate(EOperationMask mask)
	{
		if (Singles != null)
		{
			return Singles.Find((Operation_Single ret) => ret != null && ret.CanOperateMask(mask));
		}
		return null;
	}

	private Operation_Single GetStopOperate(IOperator oper, EOperationMask mask)
	{
		if (Singles != null)
		{
			return Singles.Find((Operation_Single ret) => ret != null && ret.m_Mask == mask && ret.ContainsOperator(oper));
		}
		return null;
	}

	public override bool CanOperate(Transform trans)
	{
		return OperateBoundsBounds.Contains(base.transform.InverseTransformPoint(trans.position));
	}

	public override bool IsIdle()
	{
		foreach (Operation_Single single in Singles)
		{
			if (!single.IsIdle())
			{
				return false;
			}
		}
		return true;
	}

	public override EOperationMask GetOperateMask()
	{
		EOperationMask eOperationMask = EOperationMask.None;
		if (Singles != null)
		{
			foreach (Operation_Single single in Singles)
			{
				if (single != null)
				{
					eOperationMask |= single.m_Mask;
				}
			}
		}
		return eOperationMask;
	}

	public override bool CanOperateMask(EOperationMask mask)
	{
		if (Singles != null)
		{
			foreach (Operation_Single single in Singles)
			{
				if (single != null && single.CanOperateMask(mask))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool ContainsOperator(IOperator oper)
	{
		if (Singles != null)
		{
			foreach (Operation_Single single in Singles)
			{
				if (single != null && single.ContainsOperator(oper))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool StartOperate(IOperator oper, EOperationMask mask)
	{
		Operation_Single startOperate = GetStartOperate(mask);
		if (startOperate != null)
		{
			return startOperate.StartOperate(oper, mask);
		}
		return false;
	}

	public override bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para)
	{
		Operation_Single startOperate = GetStartOperate(mask);
		if (startOperate != null)
		{
			return startOperate.StartOperate(oper, mask, para);
		}
		return false;
	}

	public override bool StopOperate(IOperator oper, EOperationMask mask)
	{
		Operation_Single stopOperate = GetStopOperate(oper, mask);
		if (stopOperate != null)
		{
			return stopOperate.StopOperate(oper, mask);
		}
		return true;
	}

	public void OnDrawGizmosSelected()
	{
		PEUtil.DrawBounds(base.transform, OperateBoundsBounds, Color.red);
	}
}
