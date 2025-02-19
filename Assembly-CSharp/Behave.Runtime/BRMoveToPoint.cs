using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRMoveToPoint), "RMoveToPoint")]
public class BRMoveToPoint : BTNormal
{
	private Vector3 m_Position;

	private BehaveResult Init(Tree sender)
	{
		if (!(GetRequest(EReqType.MoveToPoint) is RQMoveToPoint rQMoveToPoint) || !rQMoveToPoint.CanRun())
		{
			return BehaveResult.Failure;
		}
		SetNpcUpdateCampsite(value: false);
		MoveToPosition(rQMoveToPoint.position, rQMoveToPoint.speedState);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		RQMoveToPoint rQMoveToPoint = GetRequest(EReqType.MoveToPoint) as RQMoveToPoint;
		if (rQMoveToPoint == null || !rQMoveToPoint.CanRun())
		{
			if (rQMoveToPoint == null && StroyManager.Instance != null)
			{
				StroyManager.Instance.EntityReach(base.entity, trigger: false);
			}
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.RMoveToPoint);
		if (base.AskStop)
		{
			StopMove();
			if (null != StroyManager.Instance)
			{
				FaceDirection(StroyManager.Instance.GetPlayerPos() - base.position);
			}
			return BehaveResult.Running;
		}
		if (PEUtil.MagnitudeH(base.position, rQMoveToPoint.position) < rQMoveToPoint.stopRadius)
		{
			RemoveRequest(rQMoveToPoint);
			MoveToPosition(Vector3.zero);
			return BehaveResult.Success;
		}
		if (Stucking(5f))
		{
			if (rQMoveToPoint.isForce)
			{
				SetPosition(rQMoveToPoint.position);
				RemoveRequest(rQMoveToPoint);
				rQMoveToPoint.ReachPoint(base.entity);
				return BehaveResult.Success;
			}
			rQMoveToPoint.Addmask(EReqMask.Stucking);
		}
		MoveToPosition(rQMoveToPoint.position, rQMoveToPoint.speedState);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		SetNpcUpdateCampsite(value: true);
	}
}
