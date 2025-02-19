using System;

namespace Pathea;

[Serializable]
public class Action_Death : PEAction
{
	public override PEActionType ActionType => PEActionType.Death;

	public override void DoAction(PEActionParam para = null)
	{
		if (null != base.viewCmpt)
		{
			if (!base.viewCmpt.IsRagdoll)
			{
				base.viewCmpt.ActivateRagdoll(null, isGetupReady: false);
			}
			base.motionMgr.FreezePhyState(GetType(), v: true);
		}
		base.motionMgr.SetMaskState(PEActionMask.Death, state: true);
		base.motionMgr.Entity.SendMsg(EMsg.State_Die);
	}

	public override void ResetAction(PEActionParam para = null)
	{
		if (null != base.viewCmpt && !base.viewCmpt.IsRagdoll)
		{
			base.viewCmpt.ActivateRagdoll(null, isGetupReady: false);
		}
	}

	public override bool Update()
	{
		return false;
	}

	public override void OnModelBuild()
	{
		if (null != base.viewCmpt && !base.viewCmpt.IsRagdoll)
		{
			base.viewCmpt.ActivateRagdoll(null, isGetupReady: false);
		}
	}

	public override void OnModelDestroy()
	{
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Death, state: false);
		base.motionMgr.FreezePhyState(GetType(), v: false);
		if (null != base.viewCmpt)
		{
			base.viewCmpt.ActivateInjured(value: true);
			base.viewCmpt.DeactivateRagdoll();
		}
	}
}
