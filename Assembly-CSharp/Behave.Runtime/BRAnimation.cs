using Pathea;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BRAnimation), "RAnimation")]
public class BRAnimation : BTNormal
{
	private RQAnimation m_Animation;

	private float m_StartTime;

	private BehaveResult Init(Tree sender)
	{
		m_Animation = GetRequest(EReqType.Animation) as RQAnimation;
		if (m_Animation == null)
		{
			return BehaveResult.Failure;
		}
		if (!m_Animation.play)
		{
			SetBool(m_Animation.animName, value: false);
			RemoveRequest(m_Animation);
			return BehaveResult.Failure;
		}
		m_StartTime = Time.time;
		SetBool(m_Animation.animName, value: true);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		SetNpcAiType(ENpcAiType.RAnimation);
		m_Animation = GetRequest(EReqType.Animation) as RQAnimation;
		if (m_Animation == null)
		{
			return BehaveResult.Failure;
		}
		if (m_Animation != null && !m_Animation.CanRun())
		{
			return BehaveResult.Failure;
		}
		if (!base.entity.hasView)
		{
			SetBool(m_Animation.animName, value: false);
			RemoveRequest(m_Animation);
			return BehaveResult.Success;
		}
		if (base.entity.animCmpt == null || base.entity.animCmpt.animator == null)
		{
			return BehaveResult.Failure;
		}
		if (m_Animation != null && m_Animation.play)
		{
			if (m_Animation.animTime > float.Epsilon && Time.time - m_StartTime > m_Animation.animTime)
			{
				RemoveRequest(m_Animation);
				return BehaveResult.Success;
			}
			if (m_Animation.animTime < float.Epsilon && !base.entity.animCmpt.animator.GetBool(m_Animation.animName))
			{
				RemoveRequest(m_Animation);
				return BehaveResult.Success;
			}
		}
		if (m_Animation != null && !m_Animation.play)
		{
			SetBool(m_Animation.animName, value: false);
			RemoveRequest(m_Animation);
			return BehaveResult.Success;
		}
		return BehaveResult.Running;
	}
}
