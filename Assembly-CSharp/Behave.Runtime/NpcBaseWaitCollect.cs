using Pathea;
using Pathfinding;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(NpcBaseWaitCollect), "NpcBaseWaitCollect")]
public class NpcBaseWaitCollect : BTNormal
{
	private Vector3 mWaitPos;

	private void OnPathComplete(Path path)
	{
		if (PEUtil.IsInAstarGrid(base.position) && path != null && path.vectorPath.Count > 0)
		{
			Vector3 target = path.vectorPath[path.vectorPath.Count - 1];
			if (NpcMgr.IsIncenterAraound(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, target))
			{
				mWaitPos = target;
			}
			else if (base.Creater != null && base.Creater.Assembly != null)
			{
				NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out mWaitPos);
			}
		}
		else if (base.Creater != null && base.Creater.Assembly != null)
		{
			NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out mWaitPos);
		}
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Processor)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (base.IsNpcProcessing || base.WorkEntity == null)
		{
			return BehaveResult.Failure;
		}
		if (base.Creater == null || base.Creater.Assembly == null)
		{
			return BehaveResult.Failure;
		}
		if (!NpcCanWalkPos(base.Creater.Assembly.Position, base.Creater.Assembly.Radius, out mWaitPos))
		{
			return BehaveResult.Failure;
		}
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcBase || base.NpcJob != ENpcJob.Processor)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy) || base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (base.IsNpcProcessing || base.WorkEntity == null)
		{
			return BehaveResult.Failure;
		}
		if (IsMotionRunning(PEActionType.HoldShield))
		{
			EndAction(PEActionType.HoldShield);
		}
		if (IsReached(base.position, mWaitPos))
		{
			return BehaveResult.Failure;
		}
		if (Stucking())
		{
			SetPosition(mWaitPos);
			return BehaveResult.Failure;
		}
		MoveToPosition(mWaitPos);
		return BehaveResult.Running;
	}
}
