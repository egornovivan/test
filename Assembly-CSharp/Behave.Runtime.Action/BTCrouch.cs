using UnityEngine;

namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTCrouch), "Crouch")]
public class BTCrouch : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (!base.hasAttackEnemy && !(base.TDObj != null) && !(base.TDpos != Vector3.zero))
		{
			if (!GetBool("Crouch"))
			{
				SetBool("Crouch", value: true);
				return BehaveResult.Running;
			}
		}
		else if (GetBool("Crouch"))
		{
			SetBool("Crouch", value: false);
			return BehaveResult.Running;
		}
		if (GetBool("CrouchRunning"))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Failure;
	}
}
