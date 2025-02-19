using System;
using Pathea;

[Serializable]
public class Action_TwoHandWeaponPutOff : PEAction
{
	public override PEActionType ActionType => PEActionType.TwoHandSwordPutOff;

	public override void DoAction(PEActionParam para = null)
	{
		base.motionMgr.EndAction(PEActionType.TwoHandSwordHold);
	}
}
