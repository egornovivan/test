using System;

namespace Pathea;

[Serializable]
public class Action_Build : PEAction
{
	private const string AnimName = "Build";

	private bool m_EndAction;

	public override PEActionType ActionType => PEActionType.Build;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Build, state: true);
		m_EndAction = false;
		if (null != base.anim)
		{
			base.anim.ResetTrigger("ResetUpbody");
			base.anim.SetBool("Build", value: true);
		}
		base.motionMgr.Entity.SendMsg(EMsg.Build_BuildMode, true);
	}

	public override bool Update()
	{
		if (m_EndAction)
		{
			base.motionMgr.SetMaskState(PEActionMask.Build, state: false);
			base.motionMgr.Entity.SendMsg(EMsg.Build_BuildMode, false);
			return true;
		}
		return false;
	}

	public override void EndAction()
	{
		m_EndAction = true;
		if (null != base.anim)
		{
			base.anim.SetBool("Build", value: false);
		}
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.SetTrigger("ResetUpbody");
			base.anim.SetBool("Build", value: false);
		}
		base.motionMgr.SetMaskState(PEActionMask.Build, state: false);
		base.motionMgr.Entity.SendMsg(EMsg.Build_BuildMode, false);
	}
}
