using System;

namespace Pathea;

[Serializable]
public class Action_AlienDeath : Action_Death
{
	public override PEActionType ActionType => PEActionType.AlienDeath;
}
