using System.Collections.Generic;
using UnityEngine;

namespace Pathea.Operate;

public class PERides : Operation_Multiple
{
	[SerializeField]
	private PERide[] Rides;

	public override List<Operation_Single> Singles => (Rides != null && Rides.Length != 0) ? new List<Operation_Single>(Rides) : null;

	public PERide GetUseable()
	{
		for (int i = 0; i < Singles.Count; i++)
		{
			if (Singles[i].CanOperate(null))
			{
				return Singles[i] as PERide;
			}
		}
		return null;
	}

	public bool HasRide()
	{
		for (int i = 0; i < Singles.Count; i++)
		{
			if (Singles[i].CanOperate(null))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasOperater(IOperator op)
	{
		for (int i = 0; i < Singles.Count; i++)
		{
			if (Singles[i].ContainsOperator(op))
			{
				return true;
			}
		}
		return false;
	}

	public PERide GetRideByOperater(OperateCmpt op)
	{
		for (int i = 0; i < Singles.Count; i++)
		{
			if (Singles[i].ContainsOperator(op))
			{
				return (PERide)Singles[i];
			}
		}
		return null;
	}
}
