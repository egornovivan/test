using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTBurrowIdle), "BurrowIdle")]
public class BTBurrowIdle : BTNormal
{
	private BehaveResult Init(Tree sender)
	{
		bool flag = !Enemy.IsNullOrInvalid(base.attackEnemy) || !Enemy.IsNullOrInvalid(base.escapeEnemy);
		if (flag == GetBool("Burrow"))
		{
			return BehaveResult.Success;
		}
		StopMove();
		SetBool("Burrow", flag);
		return BehaveResult.Running;
	}

	private BehaveResult Tick(Tree sender)
	{
		if (GetBool("Burrowing"))
		{
			return BehaveResult.Running;
		}
		return BehaveResult.Success;
	}
}
