namespace Behave.Runtime.Action;

[BehaveAction(typeof(BTChangeWeapon), "ChangeWeapon")]
public class BTChangeWeapon : BTNormal
{
	private class Data
	{
		[Behave]
		public float checkTime;

		[Behave]
		public float prob;
	}

	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Running;
	}
}
