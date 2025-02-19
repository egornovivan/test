namespace Behave.Runtime;

[BehaveAction(typeof(BTDefend), "Defend")]
public class BTDefend : BTNormal
{
	private class Data
	{
	}

	private Data m_Data;

	private BehaveResult Tick(Tree sender)
	{
		return BehaveResult.Failure;
	}
}
