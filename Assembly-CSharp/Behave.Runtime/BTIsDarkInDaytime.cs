namespace Behave.Runtime;

[BehaveAction(typeof(BTIsDarkInDaytime), "IsDarkInLight")]
public class BTIsDarkInDaytime : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.entity.IsDark && LightMgr.Instance.GetLight(base.entity.tr, base.entity.bounds) != null)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
