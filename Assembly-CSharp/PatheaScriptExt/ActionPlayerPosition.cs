using System.Collections.Generic;
using Pathea;
using PatheaScript;
using UnityEngine;

namespace PatheaScriptExt;

public class ActionPlayerPosition : ActionImmediate
{
	protected override bool Exec()
	{
		VarRef playerId = PeType.GetPlayerId(mInfo, mTrigger);
		VarRef varRefOrValue = PatheaScript.Util.GetVarRefOrValue(mInfo, "pos", VarValue.EType.Vector3, mTrigger);
		List<PeEntity> player = PeType.GetPlayer((int)playerId.Value);
		foreach (PeEntity item in player)
		{
			PeTrans cmpt = item.GetCmpt<PeTrans>();
			cmpt.position = (Vector3)varRefOrValue.Value;
		}
		return true;
	}
}
