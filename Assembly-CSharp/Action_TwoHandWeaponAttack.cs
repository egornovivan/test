using System;
using Pathea;

[Serializable]
public class Action_TwoHandWeaponAttack : Action_SwordAttack
{
	public override PEActionType ActionType => PEActionType.TwoHandSwordAttack;
}
