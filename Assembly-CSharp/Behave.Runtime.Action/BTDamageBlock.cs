namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTDamageBlock), "BTDamageBlock")]
public class BTDamageBlock : BTNormal
{
	private static int SkillID;

	private static float Radius = 5f;

	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.maxRadius > Radius)
		{
		}
		return BehaveResult.Running;
	}
}
