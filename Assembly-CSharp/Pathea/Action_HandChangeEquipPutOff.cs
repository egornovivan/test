using System;

namespace Pathea;

[Serializable]
public class Action_HandChangeEquipPutOff : PEAction
{
	public override PEActionType ActionType => PEActionType.EquipmentPutOff;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.EndAction(PEActionType.EquipmentHold);
	}
}
