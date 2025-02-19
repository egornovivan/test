using Pathea;
using PETools;
using UnityEngine;

namespace Behave.Runtime;

[BehaveAction(typeof(BTGuard), "Guard")]
public class BTGuard : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.spawnPosition == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		if (!Enemy.IsNullOrInvalid(base.attackEnemy))
		{
			return BehaveResult.Success;
		}
		float num = 0f;
		num = ((!(base.gravity > float.Epsilon)) ? PEUtil.SqrMagnitude(base.position, base.spawnPosition) : PEUtil.SqrMagnitudeH(base.position, base.spawnPosition));
		if (num > 1f)
		{
			if (Stucking(3f))
			{
				SetPosition(base.spawnPosition);
			}
			else
			{
				MoveToPosition(base.spawnPosition, SpeedState.Run);
			}
			return BehaveResult.Running;
		}
		MoveToPosition(Vector3.zero);
		if (PEUtil.AngleH(base.transform.forward, base.spawnForward) > 5f)
		{
			FaceDirection(base.spawnForward);
			return BehaveResult.Running;
		}
		FaceDirection(Vector3.zero);
		return BehaveResult.Success;
	}
}
