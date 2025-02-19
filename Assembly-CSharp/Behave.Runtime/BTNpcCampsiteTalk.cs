using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTNpcCampsiteTalk), "NpcCampsiteTalk")]
public class BTNpcCampsiteTalk : BTNormal
{
	private class Data
	{
		[Behave]
		public string PlayState;

		[Behave]
		public string AnimName;

		[Behave]
		public float TalkRadius;

		[Behave]
		public float TargetAngle;

		[Behave]
		public float Probability;

		[Behave]
		public float TalkTime;

		public float startTalkTime;

		private bool mInit;

		public string[] Anims;

		public void Init()
		{
			if (!mInit)
			{
				Anims = PEUtil.ToArrayString(AnimName, ',');
			}
		}
	}

	private Data m_Data;

	private float mStarTime;

	private Vector3 mDirPostion;

	private Vector3 mDir;

	private PeEntity mTarget;

	private bool mHasAction;

	private bool TargetCanTalk()
	{
		if (mTarget == null || mTarget.NpcCmpt == null)
		{
			return false;
		}
		if (!CantainTalkTarget(mTarget))
		{
			return false;
		}
		if (mTarget.NpcCmpt.hasAnyRequest)
		{
			return false;
		}
		return true;
	}

	private void startTalk()
	{
		SetBool((!(Random.value > 0.5f)) ? "Talk1" : "Talk0", value: true);
		mHasAction = true;
	}

	private void endTalk()
	{
		mHasAction = false;
		SetBool("Talk0", value: false);
		SetBool("Talk1", value: false);
	}

	private void EndAnims()
	{
		for (int i = 0; i < m_Data.Anims.Length; i++)
		{
			if (GetBool(m_Data.Anims[i]))
			{
				SetBool(m_Data.Anims[i], value: false);
			}
		}
		SetBool(m_Data.PlayState, value: false);
	}

	private BehaveResult Init(Tree sender)
	{
		if (!GetData(sender, ref m_Data))
		{
			return BehaveResult.Failure;
		}
		m_Data.Init();
		if (!base.IsNpcCampsite)
		{
			return BehaveResult.Failure;
		}
		if (base.hasAnyRequest)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Interaction))
		{
			StopMove();
			return BehaveResult.Failure;
		}
		if (Random.value > m_Data.Probability)
		{
			return BehaveResult.Failure;
		}
		if (!base.entity.NpcCmpt.NpcCanChat && !base.Campsite.CalculatePostion(base.entity, m_Data.TalkRadius))
		{
			return BehaveResult.Failure;
		}
		if (!base.entity.NpcCmpt.NpcCanChat)
		{
			return BehaveResult.Failure;
		}
		StopMove();
		SetBool(m_Data.PlayState, value: true);
		SetBool(m_Data.Anims[Random.Range(0, m_Data.Anims.Length)], value: true);
		m_Data.startTalkTime = Time.time;
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (!base.IsNpcCampsite || base.hasAnyRequest || !Enemy.IsNullOrInvalid(base.attackEnemy) || !base.entity.NpcCmpt.NpcCanChat)
		{
			EndAnims();
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Interaction))
		{
			EndAnims();
			return BehaveResult.Failure;
		}
		if (Time.time - m_Data.startTalkTime > m_Data.TalkTime)
		{
			EndAnims();
		}
		if (Time.time - m_Data.startTalkTime > m_Data.TalkTime && Time.time - m_Data.startTalkTime < m_Data.TalkTime + 3f)
		{
			MoveDirection(base.position - base.entity.NpcCmpt.ChatTarget.position);
			return BehaveResult.Running;
		}
		if (Time.time - m_Data.startTalkTime > m_Data.TalkTime + 3f)
		{
			base.entity.NpcCmpt.ChatTarget = null;
			return BehaveResult.Success;
		}
		FaceDirection(base.entity.NpcCmpt.ChatTarget.position - base.position);
		return BehaveResult.Running;
	}
}
