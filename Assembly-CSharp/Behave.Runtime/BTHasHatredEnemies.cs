using Pathea;

namespace Behave.Runtime;

[BehaveAction(typeof(BTHasHatredEnemies), "hasHatredEnemies")]
public class BTHasHatredEnemies : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.Enemies != null && base.Enemies.Count > 0)
		{
			for (int i = 0; i < base.Enemies.Count; i++)
			{
				if (Enemy.IsNullOrInvalid(base.Enemies[i]))
				{
					base.Enemies.Remove(base.Enemies[i]);
				}
			}
			if (base.Enemies.Count > 0)
			{
				return BehaveResult.Success;
			}
			return BehaveResult.Failure;
		}
		return BehaveResult.Failure;
	}
}
