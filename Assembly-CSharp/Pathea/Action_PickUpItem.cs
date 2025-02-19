using System;

namespace Pathea;

[Serializable]
public class Action_PickUpItem : PEAction
{
	private bool m_EndAnim;

	public override PEActionType ActionType => PEActionType.PickUpItem;

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetFullBody");
			base.anim.SetTrigger("Gather");
		}
		base.motionMgr.SetMaskState(PEActionMask.PickUpItem, state: true);
	}

	public override bool Update()
	{
		if (null != base.anim && !m_EndAnim)
		{
			return false;
		}
		base.motionMgr.SetMaskState(PEActionMask.PickUpItem, state: false);
		return true;
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
			base.anim.ResetTrigger("Gather");
		}
		base.motionMgr.SetMaskState(PEActionMask.PickUpItem, state: false);
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (base.motionMgr.IsActionRunning(ActionType) && "GatherEnd" == eventParam)
		{
			m_EndAnim = true;
		}
	}
}
