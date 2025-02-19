namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsCrouchReady), "IsCrouchReady")]
public class BTIsCrouchReady : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.animCmpt != null && base.entity.animCmpt.ContainsParameter("Crouch") && !base.entity.animCmpt.GetBool("Crouch") && !base.entity.animCmpt.GetBool("CrouchRunning"))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
