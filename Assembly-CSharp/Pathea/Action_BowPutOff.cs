using System;

namespace Pathea;

[Serializable]
public class Action_BowPutOff : PEAction
{
	public override PEActionType ActionType => PEActionType.BowPutOff;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.EndAction(PEActionType.BowHold);
	}
}
