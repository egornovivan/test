namespace Behave.Runtime;

[BehaveAction(typeof(BTIsNpcCampsite), "IsNpcCampsite")]
public class BTIsNpcCampsite : BTNormal
{
	private BehaveResult Tick(Tree sender)
	{
		if (base.IsNpcCampsite)
		{
			return BehaveResult.Success;
		}
		return BehaveResult.Failure;
	}
}
