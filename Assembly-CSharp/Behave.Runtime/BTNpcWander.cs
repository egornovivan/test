using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcWander), "NpcWander")]
public class BTNpcWander : BTNormal
{
	private class Data
	{
		[Behave]
		public float WanderRadius;

		[Behave]
		public float Probability;

		public float m_StartWanderTime;

		public bool GetWanderPos(Vector3 center, float radiu, out Vector3 walkpos)
		{
			Vector3 randomPositionOnGroundForWander = PEUtil.GetRandomPositionOnGroundForWander(center, 0f, radiu);
			if (AiUtil.GetNearNodePosWalkable(randomPositionOnGroundForWander, out walkpos))
			{
				return true;
			}
			walkpos = Vector3.zero;
			return false;
		}

		public bool IsReached(Vector3 pos, Vector3 targetPos, float radiu = 1f)
		{
			float num = PEUtil.SqrMagnitudeH(pos, targetPos);
			return num < radiu * radiu;
		}
	}

	private Data m_Data;

	private Vector3 mWanderPos;

	private Vector3 avaidPos;

	private float avaidStatTime;

	private float avaidTime = 2f;

	private BehaveResult Init(Tree sender)
	{
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsNpcInDinnerTime || base.entity.IsNpcInSleepTime)
		{
			return BehaveResult.Failure;
		}
		if (Random.value < m_Data.Probability)
		{
			return BehaveResult.Failure;
		}
		if (m_Data.GetWanderPos(base.position, m_Data.WanderRadius, out mWanderPos))
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartWanderTime = Time.time;
		avaidPos = Vector3.zero;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (base.entity.NpcCmpt.NpcInAlert)
		{
			return BehaveResult.Failure;
		}
		if (base.entity.IsNpcInDinnerTime || base.entity.IsNpcInSleepTime)
		{
			return BehaveResult.Failure;
		}
		if (GetBool("OperateMed"))
		{
			SetBool("OperateMed", value: false);
		}
		if (GetBool("OperateCom"))
		{
			SetBool("OperateCom", value: false);
		}
		if (GetBool("FixLifeboat"))
		{
			SetBool("FixLifeboat", value: false);
		}
		if (m_Data.IsReached(base.position, mWanderPos))
		{
			return BehaveResult.Success;
		}
		MoveToPosition(mWanderPos);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
		StopMove();
	}
}
