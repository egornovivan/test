using System;

namespace Pathea;

[Serializable]
public class Action_Whacked : PEAction
{
	public override PEActionType ActionType => PEActionType.Whacked;

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("BeHit");
		}
		base.motionMgr.SetMaskState(PEActionMask.Whacked, state: true);
	}

	public override bool Update()
	{
		if (null == base.anim || null == base.anim.animator)
		{
			return true;
		}
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Whacked, state: false);
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetFullBody");
			base.anim.ResetTrigger("BeHit");
		}
	}

	protected override void OnAnimEvent(string eventParam)
	{
		if (eventParam == "HitAnimEnd")
		{
			base.motionMgr.EndImmediately(ActionType);
		}
	}
}
