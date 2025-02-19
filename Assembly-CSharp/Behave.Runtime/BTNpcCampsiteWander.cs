using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcCampsiteWander), "NpcCampsiteWander")]
public class BTNpcCampsiteWander : BTNormal
{
	private class Data
	{
		[Behave]
		public float WanderTime;

		[Behave]
		public float WanderRadius;

		[Behave]
		public float Probability;
	}

	private float mStartTime;

	private float mWanderTime = 6f;

	private Vector3 m_CurWanderPos;

	private Vector3 GetWanderPos()
	{
		return PEUtil.GetRandomPositionOnGroundForWander(base.Campsite.Pos, 0f, base.Campsite.Radius);
	}

	private bool CanWalkPos(out Vector3 walkpos)
	{
		Vector3 wanderPos = GetWanderPos();
		if (AiUtil.GetNearNodePosWalkable(wanderPos, out walkpos))
		{
			return true;
		}
		walkpos = Vector3.zero;
		return false;
	}

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (base.attackEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (!NpcCanWalkPos(base.Campsite.Pos, base.Campsite.Radius, out m_CurWanderPos))
		{
			return BehaveResult.Failure;
		}
		mStartTime = Time.time;
		mWanderTime = 6f;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.NpcCampsiteWander);
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (base.attackEnemy != null)
		{
			return BehaveResult.Failure;
		}
		if (Stucking(5f))
		{
			if (NpcCanWalkPos(base.Campsite.Pos, base.Campsite.Radius, out m_CurWanderPos) && m_CurWanderPos != Vector3.zero)
			{
				SetPosition(m_CurWanderPos);
			}
			return BehaveResult.Failure;
		}
		if (PEUtil.SqrMagnitudeH(base.position, m_CurWanderPos) < 0.25f)
		{
			return BehaveResult.Success;
		}
		if (Time.time - mStartTime > mWanderTime)
		{
			return BehaveResult.Success;
		}
		MoveToPosition(m_CurWanderPos);
		return BehaveResult.Running;
	}
}
