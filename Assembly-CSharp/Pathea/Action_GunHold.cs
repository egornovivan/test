using System;

namespace Pathea;

[Serializable]
public class Action_GunHold : Action_AimEquipHold
{
	public override PEActionType ActionType => PEActionType.GunHold;
}
