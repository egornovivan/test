using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRRTalkMove), "RTalkMove")]
public class BRRTalkMove : BTNormal
{
	private Vector3 m_Position;

	private BehaveResult Init(Tree sender)
	{
		if (!(GetRequest(EReqType.TalkMove) is RQTalkMove rQTalkMove) || !rQTalkMove.CanRun())
		{
			return BehaveResult.Failure;
		}
		MoveToPosition(rQTalkMove.position, rQTalkMove.speedState);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!(GetRequest(EReqType.TalkMove) is RQTalkMove rQTalkMove) || !rQTalkMove.CanRun())
		{
			return BehaveResult.Failure;
		}
		if (PEUtil.MagnitudeH(base.position, rQTalkMove.position) < rQTalkMove.stopRadius)
		{
			RemoveRequest(rQTalkMove);
			rQTalkMove.ReachPoint(base.entity);
			MoveToPosition(Vector3.zero);
			return BehaveResult.Success;
		}
		if (Stucking(5f))
		{
			if (rQTalkMove.isForce)
			{
				SetPosition(rQTalkMove.position);
				RemoveRequest(rQTalkMove);
				rQTalkMove.ReachPoint(base.entity);
				return BehaveResult.Success;
			}
			rQTalkMove.Addmask(EReqMask.Stucking);
		}
		MoveToPosition(rQTalkMove.position, rQTalkMove.speedState);
		return BehaveResult.Running;
	}
}
