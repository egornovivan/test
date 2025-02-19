namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTUnfold), "Unfold")]
public class BTUnfold : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (!base.hasAttackEnemy)
		{
			if (GetBool("Unfold"))
			{
				SetBool("Unfold", value: false);
				return BehaveResult.Running;
			}
		}
		else if (!GetBool("Unfold"))
		{
			SetBool("Unfold", value: true);
			return BehaveResult.Running;
		}
		if (GetBool("Unfolding"))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
