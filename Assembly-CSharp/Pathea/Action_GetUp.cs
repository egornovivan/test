using System;

namespace Pathea;

[Serializable]
public class Action_GetUp : PEAction
{
	public override PEActionType ActionType => PEActionType.GetUp;

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.viewCmpt)
		{
			base.viewCmpt.DeactivateRagdoll();
			base.viewCmpt.ActivateInjured(value: false);
		}
		base.motionMgr.FreezePhyState(GetType(), v: true);
		base.motionMgr.SetMaskState(PEActionMask.GetUp, state: true);
	}

	public override void ResetAction(PEActionParam para)
	{
		DoAction(para);
	}

	public override bool Update()
	{
		if (null != base.viewCmpt && base.viewCmpt.IsRagdoll)
		{
			return false;
		}
		OnEndAction();
		return true;
	}

	public override void EndImmediately()
	{
		OnEndAction();
	}

	private void OnEndAction()
	{
		base.motionMgr.SetMaskState(PEActionMask.GetUp, state: false);
		base.motionMgr.FreezePhyState(GetType(), v: false);
		if (null != base.viewCmpt)
		{
			base.viewCmpt.DeactivateRagdoll(immediately: true);
			base.viewCmpt.ActivateInjured(value: true, 1.5f);
		}
	}
}
