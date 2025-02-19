using System;

namespace Pathea;

[Serializable]
public class Action_Stuned : PEAction
{
	public override PEActionType ActionType => PEActionType.Stuned;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Stuned, state: true);
		if (null != base.anim)
		{
			base.anim.speed = 0f;
		}
	}

	public override bool Update()
	{
		return false;
	}

	public override void EndImmediately()
	{
		if (null != base.anim)
		{
			base.anim.speed = 1f;
		}
		base.motionMgr.SetMaskState(PEActionMask.Stuned, state: false);
	}
}
