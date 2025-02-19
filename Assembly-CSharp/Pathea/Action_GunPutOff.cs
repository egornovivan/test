using System;

namespace Pathea;

[Serializable]
public class Action_GunPutOff : PEAction
{
	public override PEActionType ActionType => PEActionType.GunPutOff;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.EndAction(PEActionType.GunHold);
	}
}
