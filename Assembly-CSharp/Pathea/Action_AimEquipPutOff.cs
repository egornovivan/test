using System;

namespace Pathea;

[Serializable]
public class Action_AimEquipPutOff : PEAction
{
	public override PEActionType ActionType => PEActionType.AimEquipPutOff;

	public override bool CanDoAction(PEActionParam para = null)
	{
		return base.motionMgr.IsActionRunning(PEActionType.AimEquipHold);
	}

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.EndAction(PEActionType.AimEquipHold);
	}
}
