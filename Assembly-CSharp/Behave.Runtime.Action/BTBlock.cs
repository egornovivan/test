using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTBlock), "Block")]
public class BTBlock : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (Random.value <= 0.7f)
		{
			return BehaveResult.Failure;
		}
		if (!NpcTypeDb.CanRun(base.NpcCmdId, ENpcControlType.Block))
		{
			return BehaveResult.Failure;
		}
		DoAction(PEActionType.HoldShield);
		return BehaveResult.Success;
	}
}
