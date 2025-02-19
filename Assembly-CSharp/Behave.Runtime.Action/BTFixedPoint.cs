using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTFixedPoint), "FixedPoint")]
public class BTFixedPoint : BTNormal
{
	private class Data
	{
		[Behave]
		public float prob;

		[Behave]
		public float minTime;

		[Behave]
		public float maxTime;

		[Behave]
		public float minRadius;

		[Behave]
		public float maxRadius;

		public float mBackPointTime;

		public float m_StartPatrolTime;
	}

	private const float Fix_Dis_min = 2f;

	private const float Fix_Dis_max = 5f;

	private Data m_Data;

	private float dis;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		if (PeGameMgr.IsMultiStory)
		{
			return BehaveResult.Failure;
		}
		if (base.FixedPointPostion == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		dis = PEUtil.MagnitudeH(base.FixedPointPostion, base.transform.position);
		if (Mathf.Abs(dis) <= 5f)
		{
			return BehaveResult.Failure;
		}
		m_Data.m_StartPatrolTime = Time.time;
		m_Data.mBackPointTime = Random.Range(m_Data.minTime, m_Data.maxTime);
		return BehaveResult.Success;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (PeGameMgr.IsMultiStory)
		{
			return BehaveResult.Failure;
		}
		SetNpcAiType(ENpcAiType.FieldNpcIdle_FixePoint);
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest || base.hasAttackEnemy)
		{
			return BehaveResult.Failure;
		}
		if (base.FixedPointPostion == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Stroll))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (Stucking(5f))
		{
			SetPosition(base.FixedPointPostion);
		}
		float f = PEUtil.MagnitudeH(base.FixedPointPostion, base.transform.position);
		if (Mathf.Abs(f) <= 2f)
		{
			float y = ((!((double)Random.value < 0.5)) ? 270f : 90f);
			SetRotation(Quaternion.Euler(0f, y, 0f));
			return BehaveResult.Success;
		}
		MoveToPosition(base.FixedPointPostion);
		return BehaveResult.Running;
	}

	private void Reset(Tree sender)
	{
	}
}
