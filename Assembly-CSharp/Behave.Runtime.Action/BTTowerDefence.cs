using Pathea;
using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTTowerDefence), "TowerDefence")]
public class BTTowerDefence : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.TDObj == null && base.TDpos == Vector3.zero)
		{
			return BehaveResult.Failure;
		}
		if ((base.entity.ProtoID == 90 || base.entity.ProtoID == 91) && !GetBool("Fly"))
		{
			return BehaveResult.Failure;
		}
		if ((base.entity.ProtoID == 93 || base.entity.ProtoID == 94) && GetBool("Crouch"))
		{
			return BehaveResult.Failure;
		}
		if (base.hasAttackEnemy)
		{
			return BehaveResult.Success;
		}
		if (base.TDpos != Vector3.zero)
		{
			MoveToPosition(base.TDpos, SpeedState.Run);
		}
		else if (base.TDObj != null)
		{
			MoveToPosition(base.TDObj.transform.position, SpeedState.Run);
		}
		return BehaveResult.Running;
	}
}
