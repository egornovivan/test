using System;

namespace Pathea;

[Serializable]
public class Action_Cutscene : PEAction
{
	public override PEActionType ActionType => PEActionType.Cutscene;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.SetMaskState(PEActionMask.Cutscene, state: true);
	}

	public override bool Update()
	{
		return false;
	}

	public override void EndImmediately()
	{
		base.motionMgr.SetMaskState(PEActionMask.Cutscene, state: false);
	}
}
