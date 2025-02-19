using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTCsNoIdleRun), "CsNoIdleRun")]
public class BTCsNoIdleRun : BTNormal
{
	private class Data
	{
		[Behave]
		public float RunRadius;
	}

	private Data m_Data;

	private FindHidePos mFind;

	private float startRunTime;

	private float CHECK_TIME = 5f;

	private float startHideTime;

	private float CHECK_Hide_TIME = 1f;

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		if (!base.IsNpcBase || base.entity.NpcCmpt.csCanIdle)
		{
			return BehaveResult.Failure;
		}
		mFind = new FindHidePos(m_Data.RunRadius, needHide: false, 15f);
		startRunTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.NpcCmpt.csCanIdle)
		{
			return BehaveResult.Failure;
		}
		if (Time.time - startRunTime > CHECK_TIME)
		{
			base.entity.NpcCmpt.SetCanIdle(_canIdle: true);
			return BehaveResult.Failure;
		}
		if (Time.time - startHideTime > CHECK_Hide_TIME)
		{
			Vector3 hideDir = mFind.GetHideDir(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position, base.position, base.Enemies);
			if (mFind.bNeedHide)
			{
				Vector3 pos = base.position + hideDir.normalized * 15f;
				MoveToPosition(pos, SpeedState.Run);
			}
			else if (base.entity.target.beSkillTarget)
			{
				MoveDirection(base.transform.right, SpeedState.Run);
			}
			else
			{
				StopMove();
				FaceDirection(PeSingleton<PeCreature>.Instance.mainPlayer.peTrans.position - base.position);
			}
			startHideTime = Time.time;
		}
		return BehaveResult.Running;
	}
}
