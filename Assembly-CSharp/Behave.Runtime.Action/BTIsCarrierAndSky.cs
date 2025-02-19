namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTIsCarrierAndSky), "IsCarrierAndSky")]
public class BTIsCarrierAndSky : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.attackEnemy == null)
		{
			return BehaveResult.Failure;
		}
		if (base.attackEnemy.isCarrierAndSky)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
