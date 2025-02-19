using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTHasSelectEnemy), "HasSelectEnemy")]
public class BTHasSelectEnemy : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (!Enemy.IsNullOrInvalid(base.selectattackEnemy))
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
