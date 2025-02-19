using System;

namespace Pathea;

[Serializable]
public class Action_BowReload : PEAction
{
	private PEBow m_Bow;

	private int m_TargetAmmoIndex;

	private AudioController m_Audio;

	private bool m_AnimEnd;

	public bool m_IgnoreItem;

	public override PEActionType ActionType => PEActionType.BowReload;

	public PEBow bow
	{
		get
		{
			return m_Bow;
		}
		set
		{
			if (null == value)
			{
				base.motionMgr.EndImmediately(ActionType);
			}
			m_Bow = value;
		}
	}

	public override bool CanDoAction(PEActionParam para = null)
	{
		PEActionParamN pEActionParamN = para as PEActionParamN;
		int n = pEActionParamN.n;
		if (m_IgnoreItem)
		{
			return null != bow && null != base.packageCmpt;
		}
		return null != bow && null != base.packageCmpt && (n != bow.curItemIndex || base.packageCmpt.GetItemCount(bow.curItemID) > 0);
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.BowReload, state: true);
		PEActionParamN pEActionParamN = para as PEActionParamN;
		m_TargetAmmoIndex = pEActionParamN.n;
		if (null != bow && null != base.anim && base.motionMgr.IsActionRunning(PEActionType.BowHold))
		{
			base.anim.SetTrigger(bow.m_ReloadAnim);
			m_AnimEnd = false;
		}
		if (null != bow && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
		{
			base.ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
		}
	}

	public override bool Update()
	{
		if (null == bow || null == base.anim || base.motionMgr.GetMaskState(PEActionMask.InWater))
		{
			base.motionMgr.SetMaskState(PEActionMask.BowReload, state: false);
			return true;
		}
		if (null == base.anim || m_AnimEnd)
		{
			base.motionMgr.SetMaskState(PEActionMask.BowReload, state: false);
			bow.curItemIndex = m_TargetAmmoIndex;
			if (null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
			{
				base.ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
			}
			return true;
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.BowReload, state: false);
		if (null != base.anim)
		{
			if (null != bow)
			{
				base.anim.ResetTrigger(bow.m_ReloadAnim);
			}
			base.anim.SetTrigger("ResetUpbody");
		}
		if (null != bow && null != base.ikCmpt && null != base.ikCmpt.m_IKAimCtrl)
		{
			base.ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
		}
		if (null != bow && base.motionMgr.IsActionRunning(PEActionType.BowHold))
		{
			bow.SetArrowShowState(show: true);
			bow.SetBowOpenState(openBow: true);
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if ("ReloadEnd" == eventParam)
		{
			m_AnimEnd = true;
		}
	}
}
