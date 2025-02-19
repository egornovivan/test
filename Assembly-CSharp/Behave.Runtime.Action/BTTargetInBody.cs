using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTargetInBody), "TargetInBody")]
public class BTTargetInBody : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy != null && InBody(base.attackEnemy.position + Vector3.up * 0.5f))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
